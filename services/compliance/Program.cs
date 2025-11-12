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
        .AddConsoleExporter()
        .AddPrometheusExporter());

// Database
builder.Services.AddDbContext<ComplianceDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("OIS.Compliance")));

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
    options.AddPolicy("role:investor", p => p.RequireRole("investor"));
    options.AddPolicy("role:backoffice", p => p.RequireRole("backoffice"));
    options.AddPolicy("role:investor-or-backoffice", p =>
        p.RequireAssertion(ctx => ctx.User.IsInRole("investor") || ctx.User.IsInRole("backoffice")));
});

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
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
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapPrometheusScrapingEndpoint("/metrics");

// API Endpoints
var api = app.MapGroup("/v1").WithTags("Compliance").RequireAuthorization();

api.MapPost("/compliance/kyc/check", async (
    KycCheckRequest request,
    IComplianceService service,
    CancellationToken ct) =>
{
    var result = await service.CheckKycAsync(request, ct);
    return Results.Ok(result);
})
.WithName("CheckKyc")
.RequireAuthorization("role:investor-or-backoffice")
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
.RequireAuthorization("role:investor-or-backoffice")
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
.RequireAuthorization("role:investor-or-backoffice")
.WithOpenApi();

var complaintsApi = app.MapGroup("/v1/complaints").WithTags("Complaints").RequireAuthorization();

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
.RequireAuthorization("role:investor")
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
.RequireAuthorization("role:investor-or-backoffice")
.WithOpenApi();

app.Run();

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

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
