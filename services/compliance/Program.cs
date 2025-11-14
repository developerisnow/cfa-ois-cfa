using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OIS.Compliance;
using OIS.Compliance.DTOs;
using OIS.Compliance.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter()));

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("compliance-service"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());

// Database
var complianceMigrationsAssembly = typeof(ComplianceDbContext).Assembly.GetName().Name;
builder.Services.AddDbContext<ComplianceDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly(complianceMigrationsAssembly)));

// Services
builder.Services.AddScoped<IWatchlistsService, WatchlistsServiceStub>();
builder.Services.AddScoped<IQualificationPolicyService, QualificationPolicyService>();
builder.Services.AddScoped<IOutboxService, OutboxService>();
builder.Services.AddScoped<IComplianceService, ComplianceService>();

// API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ComplianceDbContext>();

var app = builder.Build();

// Apply migrations (optional, via MIGRATE_ON_STARTUP=true)
var migrateOnStartup = Environment.GetEnvironmentVariable("MIGRATE_ON_STARTUP");
if (string.Equals(migrateOnStartup, "true", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
    db.Database.Migrate();
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapHealthChecks("/health");

// API Endpoints
var api = app.MapGroup("/v1").WithTags("Compliance");

api.MapPost("/compliance/kyc/check", async (
    KycCheckRequest request,
    IComplianceService service,
    CancellationToken ct) =>
{
    var result = await service.CheckKycAsync(request, ct);
    return Results.Ok(result);
})
.WithName("CheckKyc")
.WithOpenApi();

api.MapGet("/complaints", async (
    IComplaintsService service,
    CancellationToken ct) =>
{
    var complaints = await service.GetComplaintsAsync(ct);
    return Results.Ok(complaints);
})
.WithName("GetComplaints")
.WithOpenApi();

api.MapPost("/complaints", async (
    CreateComplaintRequest request,
    IComplaintsService service,
    CancellationToken ct) =>
{
    var complaintId = await service.CreateComplaintAsync(request, ct);
    return Results.Ok(new { complaintId });
})
.WithName("CreateComplaint")
.WithOpenApi();

app.Run();
