using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OIS.Issuance;
using OIS.Issuance.DTOs;
using OIS.Issuance.Services;
using OIS.Issuance.Validators;
using Serilog;
using MassTransit;
using OIS.Contracts.Events;
using OIS.Issuance.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
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
        .AddPrometheusExporter()
        .AddMeter(Metrics.MeterName));

// Prometheus metrics endpoint
builder.Services.AddPrometheusExporter();

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

// MassTransit + Kafka publish
if (builder.Configuration.GetValue<bool>("Kafka:Enabled", true))
{
    builder.Services.AddMassTransit(x =>
    {
        x.UsingKafka((context, cfg) =>
        {
            cfg.Host(builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092");
            cfg.Message<IssuancePublished>(m => m.SetEntityName("ois.issuance.published"));
            cfg.Message<IssuanceClosed>(m => m.SetEntityName("ois.issuance.closed"));
            cfg.Message<AuditLogged>(m => m.SetEntityName("ois.audit.logged"));
        });
    });

    builder.Services.AddHostedService<OIS.Issuance.Background.OutboxPublisher>();
}

// Validation
builder.Services.AddValidatorsFromAssemblyContaining<CreateIssuanceRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

// AuthN/Z
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var authority = builder.Configuration["Keycloak:Authority"];
        if (!string.IsNullOrEmpty(authority)) options.Authority = authority;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            RoleClaimType = ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = ctx => { MapKeycloakRoles(ctx); return Task.CompletedTask; }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("role:issuer", p => p.RequireRole("issuer"));
    options.AddPolicy("role:any-auth", p => p.RequireAuthenticatedUser());
});

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
app.MapPrometheusScrapingEndpoint("/metrics");

// Correlation + request metrics
app.Use(async (ctx, next) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    // Correlate X-Request-ID
    if (!ctx.Request.Headers.TryGetValue("X-Request-ID", out var reqId) || string.IsNullOrWhiteSpace(reqId))
    {
        reqId = Guid.NewGuid().ToString();
        ctx.Request.Headers["X-Request-ID"] = reqId;
    }
    ctx.Response.Headers["X-Request-ID"] = reqId.ToString();

    try
    {
        await next();
    }
    finally
    {
        sw.Stop();
        var status = ctx.Response.StatusCode;
        var route = ctx.GetEndpoint()?.DisplayName ?? "unknown";
        Metrics.RequestDurationMs.Record(sw.Elapsed.TotalMilliseconds, new("route", route), new("method", ctx.Request.Method), new("status", status.ToString()));
        if (status >= 500) Metrics.RequestErrors.Add(1, new("route", route), new("method", ctx.Request.Method));
    }
});

// API Endpoints
var api = app.MapGroup("/v1").WithTags("Issuances").RequireAuthorization();

api.MapPost("/issuances", async (
    CreateIssuanceRequest request,
    IIssuanceService service,
    CancellationToken ct) =>
{
    var result = await service.CreateAsync(request, ct);
    return Results.Created($"/v1/issuances/{result.Id}", result);
})
.WithName("CreateIssuance")
.RequireAuthorization("role:issuer")
.WithOpenApi();

api.MapGet("/issuances/{id:guid}", async (
    Guid id,
    IIssuanceService service,
    CancellationToken ct) =>
{
    var result = await service.GetByIdAsync(id, ct);
    return result != null ? Results.Ok(result) : Results.NotFound();
})
.WithName("GetIssuance")
.RequireAuthorization("role:any-auth")
.WithOpenApi();

api.MapPost("/issuances/{id:guid}/publish", async (
    Guid id,
    IIssuanceService service,
    CancellationToken ct) =>
{
    var existing = await service.GetByIdAsync(id, ct);
    if (existing is null)
        return Results.NotFound();
    try
    {
        var result = await service.PublishAsync(id, ct);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 400,
            title: "Bad Request");
    }
})
.WithName("PublishIssuance")
.RequireAuthorization("role:issuer")
.WithOpenApi();

api.MapPost("/issuances/{id:guid}/close", async (
    Guid id,
    IIssuanceService service,
    CancellationToken ct) =>
{
    var existing = await service.GetByIdAsync(id, ct);
    if (existing is null)
        return Results.NotFound();
    try
    {
        var result = await service.CloseAsync(id, ct);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 400,
            title: "Bad Request");
    }
})
.WithName("CloseIssuance")
.RequireAuthorization("role:issuer")
.WithOpenApi();

app.Run();

public partial class Program { }

static void MapKeycloakRoles(TokenValidatedContext ctx)
{
    try
    {
        if (ctx.Principal?.Identity is not ClaimsIdentity identity) return;
        var realmAccessJson = identity.FindFirst("realm_access")?.Value;
        if (!string.IsNullOrEmpty(realmAccessJson))
        {
            using var doc = System.Text.Json.JsonDocument.Parse(realmAccessJson);
            if (doc.RootElement.TryGetProperty("roles", out var rolesEl) && rolesEl.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var r in rolesEl.EnumerateArray())
                {
                    var role = r.GetString();
                    if (!string.IsNullOrEmpty(role))
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }
        }
    }
    catch { }
}
