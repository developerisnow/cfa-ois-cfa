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
builder.Services.AddDbContext<ComplianceDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<IWatchlistsService, WatchlistsServiceStub>();
builder.Services.AddScoped<IQualificationPolicyService, QualificationPolicyService>();
builder.Services.AddScoped<IOutboxService, OutboxService>();
builder.Services.AddScoped<IComplianceService, ComplianceService>();

// API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

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

api.MapPost("/compliance/qualification/evaluate", async (
    QualificationEvaluateRequest request,
    IComplianceService service,
    CancellationToken ct) =>
{
    var result = await service.EvaluateQualificationAsync(request, ct);
    return Results.Ok(result);
})
.WithName("EvaluateQualification")
.WithOpenApi();

api.MapGet("/compliance/investors/{id:guid}/status", async (
    Guid id,
    IComplianceService service,
    CancellationToken ct) =>
{
    var result = await service.GetInvestorStatusAsync(id, ct);
    return result != null ? Results.Ok(result) : Results.NotFound();
})
.WithName("GetInvestorStatus")
.WithOpenApi();

var complaintsApi = app.MapGroup("/v1/complaints").WithTags("Complaints");

complaintsApi.MapPost("", async (
    CreateComplaintRequest request,
    HttpContext httpContext,
    IComplianceService service,
    CancellationToken ct) =>
{
    string? idemKey = null;
    if (httpContext.Request.Headers.TryGetValue("Idempotency-Key", out var idemKeyValues))
    {
        idemKey = idemKeyValues.FirstOrDefault();
    }

    var result = await service.CreateComplaintAsync(request, idemKey, ct);
    return Results.Created($"/v1/complaints/{result.Id}", result);
})
.WithName("CreateComplaint")
.WithOpenApi();

complaintsApi.MapGet("/{id:guid}", async (
    Guid id,
    IComplianceService service,
    CancellationToken ct) =>
{
    var result = await service.GetComplaintAsync(id, ct);
    return result != null ? Results.Ok(result) : Results.NotFound();
})
.WithName("GetComplaint")
.WithOpenApi();

app.Run();
