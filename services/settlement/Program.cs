using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OIS.Settlement;
using OIS.Settlement.DTOs;
using OIS.Settlement.Services;
using Serilog;
using OIS.Settlement.Background;
using OIS.Settlement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter()));

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("settlement-service"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter()
        .AddPrometheusExporter()
        .AddMeter(Metrics.MeterName));

// Database
builder.Services.AddDbContext<SettlementDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("OIS.Settlement")));

// HTTP Clients
builder.Services.AddHttpClient<IRegistryClient, RegistryClient>();
builder.Services.AddHttpClient<IIssuanceClient, IssuanceClient>();
builder.Services.AddHttpClient<IBankNominalClient, BankNominalClient>();

// Services
builder.Services.AddScoped<IOutboxService, OutboxService>();
builder.Services.AddScoped<ISettlementService, SettlementService>();

// Background workers (Kafka)
var kafkaEnabled = builder.Configuration.GetValue<bool>("Kafka:Enabled", true);
if (kafkaEnabled)
{
    builder.Services.AddHostedService<OrderPaidConsumer>();
    builder.Services.AddHostedService<OutboxPublisher>();
}

// AuthN/Z
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var authority = builder.Configuration["Keycloak:Authority"];
        if (!string.IsNullOrEmpty(authority))
        {
            options.Authority = authority;
        }
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
    options.AddPolicy("role:backoffice", p => p.RequireRole("backoffice"));
    options.AddPolicy("role:issuer-or-backoffice", p =>
        p.RequireAssertion(ctx => ctx.User.IsInRole("issuer") || ctx.User.IsInRole("backoffice")));
});

// API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<SettlementDbContext>();

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SettlementDbContext>();
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
var api = app.MapGroup("/v1").WithTags("Settlement").RequireAuthorization();

api.MapPost("/settlement/run", async (
    DateOnly? date,
    ISettlementService service,
    CancellationToken ct) =>
{
    try
    {
        var result = await service.RunSettlementAsync(date, ct);
        return Results.Accepted($"/v1/settlement/batches/{result.BatchId}", result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 400,
            title: "Bad Request");
    }
})
.WithName("RunSettlement")
.RequireAuthorization("role:backoffice")
.WithOpenApi();

api.MapGet("/reports/payouts", async (
    DateOnly? from,
    DateOnly? to,
    ISettlementService service,
    CancellationToken ct) =>
{
    var fromDate = from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
    var toDate = to ?? DateOnly.FromDateTime(DateTime.UtcNow);

    if (fromDate > toDate)
    {
        return Results.Problem(
            detail: "from date must be less than or equal to to date",
            statusCode: 400,
            title: "Bad Request");
    }

    var result = await service.GetPayoutsReportAsync(fromDate, toDate, ct);
    return Results.Ok(result);
})
.WithName("GetPayoutsReport")
.RequireAuthorization("role:issuer-or-backoffice")
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
