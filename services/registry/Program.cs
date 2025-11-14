using Microsoft.EntityFrameworkCore;
using FluentValidation;
using OIS.Registry.Validators;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OIS.Registry;
using OIS.Registry.DTOs;
using OIS.Registry.Services;
using Serilog;
using System.Diagnostics;
using MassTransit;
using OIS.Contracts.Events;
using OIS.Registry.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter()));

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("registry-service"))
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
builder.Services.AddDbContext<RegistryDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("OIS.Registry")));

// HTTP Clients
builder.Services.AddHttpClient<IBankNominalService, BankNominalServiceClient>();
builder.Services.AddHttpClient<IComplianceService, ComplianceServiceClient>();
builder.Services.AddHttpClient<LedgerRegistryAdapter>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));

// Services
builder.Services.AddScoped<ILedgerRegistry, LedgerRegistryAdapter>();
builder.Services.AddScoped<IOutboxService, OutboxService>();
builder.Services.AddScoped<IRegistryService, RegistryService>();

// Validation
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

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
            OnTokenValidated = ctx =>
            {
                MapKeycloakRoles(ctx);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("role:investor", p => p.RequireRole("investor"));
    options.AddPolicy("role:issuer", p => p.RequireRole("issuer"));
    options.AddPolicy("role:broker", p => p.RequireRole("broker"));
    options.AddPolicy("role:backoffice", p => p.RequireRole("backoffice"));
    options.AddPolicy("role:investor-or-backoffice", p =>
        p.RequireAssertion(ctx => ctx.User.IsInRole("investor") || ctx.User.IsInRole("backoffice")));
    options.AddPolicy("scope:orders.write", p => p.RequireAssertion(HasScope("orders.write")));
    options.AddPolicy("scope:orders.read", p => p.RequireAssertion(HasScope("orders.read")));
});

// MassTransit + Kafka for publishing
if (builder.Configuration.GetValue<bool>("Kafka:Enabled", true))
{
    builder.Services.AddMassTransit(x =>
    {
        x.AddRider(rider =>
        {
            rider.UsingKafka((context, cfg) =>
            {
                cfg.Host(builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092");
            });
        });
    });

    builder.Services.AddHostedService<OIS.Registry.Background.OutboxPublisher>();
}

// API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<RegistryDbContext>();

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("sensitive", httpContext =>
    {
        var key = GetPartitionKey(httpContext);
        return RateLimitPartition.GetTokenBucketLimiter(key, _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 20,
            TokensPerPeriod = 20,
            ReplenishmentPeriod = TimeSpan.FromSeconds(1),
            AutoReplenishment = true,
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });
});

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RegistryDbContext>();
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
app.UseRateLimiter();
app.MapHealthChecks("/health");
app.MapPrometheusScrapingEndpoint("/metrics");

// Correlation + request metrics
app.Use(async (ctx, next) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
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
        var tags = new System.Collections.Generic.KeyValuePair<string, object?>[]
        {
            new("route", route),
            new("method", ctx.Request.Method),
            new("status", status.ToString())
        };
        Metrics.RequestDurationMs.Record(sw.Elapsed.TotalMilliseconds, tags);
        if (status >= 500)
        {
            var errTags = new System.Collections.Generic.KeyValuePair<string, object?>[]
            {
                new("route", route),
                new("method", ctx.Request.Method)
            };
            Metrics.RequestErrors.Add(1, errTags);
        }
    }
});

// API Endpoints
var api = app.MapGroup("/v1").WithTags("Registry").RequireAuthorization();

api.MapPost("/orders", async (
    CreateOrderRequest request,
    HttpContext httpContext,
    IRegistryService service,
    CancellationToken ct) =>
{
    // Get idempotency key from header
    if (!httpContext.Request.Headers.TryGetValue("Idempotency-Key", out var idemKeyValues) ||
        !Guid.TryParse(idemKeyValues.FirstOrDefault(), out var idemKeyGuid))
    {
        return Results.Problem(
            detail: "Idempotency-Key header is required and must be a valid UUID",
            statusCode: 400,
            title: "Bad Request");
    }

    var idemKey = idemKeyGuid.ToString();
    var result = await service.PlaceOrderAsync(request, idemKey, ct);
    return Results.Accepted($"/v1/orders/{result.Id}", result);
})
.WithName("PlaceOrder")
.RequireAuthorization("role:investor")
.RequireRateLimiting("sensitive")
.WithOpenApi();

api.MapGet("/orders/{id:guid}", async (
    Guid id,
    IRegistryService service,
    CancellationToken ct) =>
{
    var result = await service.GetOrderAsync(id, ct);
    return result != null ? Results.Ok(result) : Results.NotFound();
})
.WithName("GetOrder")
.RequireAuthorization("role:investor-or-backoffice")
.WithOpenApi();

api.MapGet("/wallets/{investorId:guid}", async (
    Guid investorId,
    IRegistryService service,
    CancellationToken ct) =>
{
    var result = await service.GetWalletAsync(investorId, ct);
    return result != null ? Results.Ok(result) : Results.NotFound();
})
.WithName("GetWallet")
.RequireAuthorization("role:investor-or-backoffice")
.WithOpenApi();

api.MapPost("/issuances/{id:guid}/redeem", async (
    Guid id,
    RedeemRequest request,
    IRegistryService service,
    CancellationToken ct) =>
{
    try
    {
        var result = await service.RedeemAsync(id, request, ct);
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
.WithName("RedeemIssuance")
.RequireAuthorization("role:investor")
.RequireRateLimiting("sensitive")
.WithOpenApi();

api.MapPost("/orders/{id:guid}/cancel", async (
    Guid id,
    IRegistryService service,
    CancellationToken ct) =>
{
    try
    {
        var result = await service.CancelOrderAsync(id, ct);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ? 404 : 400,
            title: ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ? "Not Found" : "Bad Request");
    }
})
.WithName("CancelOrder")
.RequireAuthorization("role:investor-or-backoffice")
.RequireRateLimiting("sensitive")
.WithOpenApi();

api.MapPost("/orders/{id:guid}/mark-paid", async (
    Guid id,
    IRegistryService service,
    CancellationToken ct) =>
{
    try
    {
        var result = await service.MarkPaidAsync(id, null, ct);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ? 404 : 400,
            title: ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ? "Not Found" : "Bad Request");
    }
})
.WithName("MarkOrderPaid")
.RequireAuthorization("role:investor-or-backoffice")
.RequireRateLimiting("sensitive")
.WithOpenApi();

app.Run();
static Func<Microsoft.AspNetCore.Authorization.AuthorizationHandlerContext, bool> HasScope(string scope) => ctx =>
{
    var scp = ctx.User.FindFirst("scope")?.Value ?? ctx.User.FindFirst("scp")?.Value;
    if (string.IsNullOrWhiteSpace(scp)) return false;
    return scp.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Any(s => string.Equals(s, scope, StringComparison.OrdinalIgnoreCase));
};

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
    catch { /* ignore parsing errors */ }
}

static string GetPartitionKey(HttpContext ctx)
{
    var sub = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (!string.IsNullOrEmpty(sub)) return $"user:{sub}";
    return $"ip:{ctx.Connection.RemoteIpAddress}";
}
