using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OIS.Issuance;
using OIS.Issuance.DTOs;
using OIS.Issuance.Services;
using OIS.Issuance.Validators;
using Serilog;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter()));

// OpenTelemetry
var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://otel-collector:4317";
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("issuance-service")
        .AddAttributes(new Dictionary<string, object> { ["environment"] = builder.Environment.EnvironmentName }))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint)))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddPrometheusExporter());

// Prometheus metrics endpoint (disabled in this deployment; enable when collector scrapes directly)
// builder.Services.AddPrometheusExporter();

// Database
var issuanceMigrationsAssembly = typeof(IssuanceDbContext).Assembly.GetName().Name;
builder.Services.AddDbContext<IssuanceDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly(issuanceMigrationsAssembly)));

// HTTP Client for Ledger Adapter
builder.Services.AddHttpClient<LedgerIssuanceAdapter>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));

// Services
builder.Services.AddScoped<ILedgerIssuance, LedgerIssuanceAdapter>();
builder.Services.AddScoped<IOutboxService, OutboxService>();
builder.Services.AddScoped<IIssuanceService, IssuanceService>();

// Validation
builder.Services.AddValidatorsFromAssemblyContaining<CreateIssuanceRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

// API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<IssuanceDbContext>();

var app = builder.Build();

// Apply migrations (optional, via MIGRATE_ON_STARTUP=true)
var migrateOnStartup = Environment.GetEnvironmentVariable("MIGRATE_ON_STARTUP");
if (string.Equals(migrateOnStartup, "true", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<IssuanceDbContext>();
    db.Database.Migrate();
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");

var api = app.MapGroup("/v1/issuances").WithTags("Issuances");

api.MapPost("/", async (
    CreateIssuanceRequest request,
    IIssuanceService service,
    CancellationToken ct) =>
{
    var id = await service.CreateIssuanceAsync(request, ct);
    return Results.Ok(new { id });
})
.WithName("CreateIssuance")
.WithOpenApi();

api.MapPost("/{id:guid}/publish", async (
    Guid id,
    IIssuanceService service,
    CancellationToken ct) =>
{
    await service.PublishIssuanceAsync(id, ct);
    return Results.Accepted();
})
.WithName("PublishIssuance")
.WithOpenApi();

api.MapPost("/{id:guid}/close", async (
    Guid id,
    IIssuanceService service,
    CancellationToken ct) =>
{
    await service.CloseIssuanceAsync(id, ct);
    return Results.Accepted();
})
.WithName("CloseIssuance")
.WithOpenApi();

app.Run();
