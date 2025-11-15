using Microsoft.EntityFrameworkCore;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OIS.Compliance;
using OIS.Compliance.DTOs;
using OIS.Compliance.Services;
using OIS.Compliance.Infrastructure;
using MassTransit;
using OIS.Contracts.Events;
using Serilog;
using System.Text;
using System.Text.Json;
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
        .AddService("compliance-service"))
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

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("sensitive", httpContext =>
    {
        var key = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(key)) key = $"user:{key}"; else key = $"ip:{httpContext.Connection.RemoteIpAddress}";
        return RateLimitPartition.GetTokenBucketLimiter(key!, _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 10,
            TokensPerPeriod = 10,
            ReplenishmentPeriod = TimeSpan.FromSeconds(1),
            AutoReplenishment = true,
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });
});

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

    builder.Services.AddHostedService<OIS.Compliance.Background.OutboxPublisher>();
}

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
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapPrometheusScrapingEndpoint("/metrics");
app.UseRateLimiter();

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

// (Kafka rider configured above)

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
.RequireRateLimiting("sensitive")
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

// KYC admin endpoints
var kycApi = app.MapGroup("/v1/compliance/kyc").WithTags("KYC").RequireAuthorization("role:backoffice");

kycApi.MapPost("/investors/{id:guid}/approve", async (
    Guid id,
    HttpContext http,
    IComplianceService service,
    CancellationToken ct) =>
{
    Guid? actor = http.User.Identity?.IsAuthenticated == true ?
        http.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value is string s && Guid.TryParse(s, out var g) ? g : null
        : null;

    var result = await service.UpdateKycStatusAsync(id, "pass", actor, null, ct);
    return Results.Ok(result);
})
.WithName("ApproveKyc")
.RequireRateLimiting("sensitive")
.WithOpenApi();

kycApi.MapPost("/investors/{id:guid}/reject", async (
    Guid id,
    HttpContext http,
    IComplianceService service,
    CancellationToken ct) =>
{
    Guid? actor = http.User.Identity?.IsAuthenticated == true ?
        http.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value is string s && Guid.TryParse(s, out var g) ? g : null
        : null;

    var result = await service.UpdateKycStatusAsync(id, "fail", actor, null, ct);
    return Results.Ok(result);
})
.WithName("RejectKyc")
.RequireRateLimiting("sensitive")
.WithOpenApi();

// KYC tasks queue
var kycTasks = app.MapGroup("/v1/kyc/tasks").WithTags("KYC Tasks").RequireAuthorization("role:backoffice");

kycTasks.MapPost("", async (
    CreateKycTaskRequest req,
    IComplianceService service,
    CancellationToken ct) =>
{
    var task = await service.CreateKycTaskAsync(req.InvestorId, req.Reason, ct);
    return Results.Created($"/v1/kyc/tasks/{task.Id}", task);
})
.WithName("CreateKycTask")
.RequireRateLimiting("sensitive")
.WithOpenApi();

kycTasks.MapGet("", async (
    string? status,
    IComplianceService service,
    CancellationToken ct) =>
{
    var list = await service.ListKycTasksAsync(status, ct);
    return Results.Ok(list);
})
.WithName("ListKycTasks")
.WithOpenApi();

kycTasks.MapPost("/{id:guid}/approve", async (
    Guid id,
    HttpContext http,
    IComplianceService service,
    CancellationToken ct) =>
{
    Guid? actor = http.User.Identity?.IsAuthenticated == true ?
        http.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value is string s && Guid.TryParse(s, out var g) ? g : null
        : null;

    var task = await service.ResolveKycTaskAsync(id, "approve", actor, null, ct);
    return task != null ? Results.Ok(task) : Results.NotFound();
})
.WithName("ApproveKycTask")
.RequireRateLimiting("sensitive")
.WithOpenApi();

kycTasks.MapPost("/{id:guid}/reject", async (
    Guid id,
    HttpContext http,
    IComplianceService service,
    CancellationToken ct) =>
{
    Guid? actor = http.User.Identity?.IsAuthenticated == true ?
        http.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value is string s && Guid.TryParse(s, out var g) ? g : null
        : null;

    var task = await service.ResolveKycTaskAsync(id, "reject", actor, null, ct);
    return task != null ? Results.Ok(task) : Results.NotFound();
})
.WithName("RejectKycTask")
.RequireRateLimiting("sensitive")
.WithOpenApi();

// Audit reporting (immutable, from outbox write-ahead log)
var auditApi = app.MapGroup("/v1/audit").WithTags("Audit").RequireAuthorization("role:backoffice");

auditApi.MapGet("", async (
    Guid? actor,
    string? action,
    string? entity,
    DateTime? from,
    DateTime? to,
    int? limit,
    int? offset,
    ComplianceDbContext db,
    CancellationToken ct) =>
{
    var q = db.OutboxMessages
        .Where(m => m.Topic == "ois.audit.logged")
        .OrderByDescending(m => m.CreatedAt)
        .AsEnumerable();

    IEnumerable<OutboxMessage> filtered = q;

    filtered = filtered.Where(m =>
    {
        try
        {
            using var doc = JsonDocument.Parse(m.Payload);
            var root = doc.RootElement;
            if (actor.HasValue)
            {
                var actorVal = root.TryGetProperty("actor", out var el) ? el.GetString() : null;
                if (!Guid.TryParse(actorVal, out var a) || a != actor.Value) return false;
            }
            if (!string.IsNullOrEmpty(action))
            {
                var act = root.TryGetProperty("action", out var el) ? el.GetString() : null;
                if (!string.Equals(act, action, StringComparison.OrdinalIgnoreCase)) return false;
            }
            if (!string.IsNullOrEmpty(entity))
            {
                var ent = root.TryGetProperty("entity", out var el) ? el.GetString() : null;
                if (!string.Equals(ent, entity, StringComparison.OrdinalIgnoreCase)) return false;
            }
            var ts = root.TryGetProperty("timestamp", out var tsEl) && tsEl.ValueKind == JsonValueKind.String
                ? DateTime.Parse(tsEl.GetString()!)
                : m.CreatedAt;
            if (from.HasValue && ts < from.Value) return false;
            if (to.HasValue && ts > to.Value) return false;
            return true;
        }
        catch
        {
            return false;
        }
    });

    var take = Math.Clamp(limit ?? 20, 1, 100);
    var skip = Math.Max(offset ?? 0, 0);
    var page = filtered.Skip(skip).Take(take).Select(m => MapAudit(m)).ToList();

    return Results.Ok(new { items = page });
})
.WithName("GetAuditEvents")
.WithOpenApi();

auditApi.MapGet("/{id:guid}", async (
    Guid id,
    ComplianceDbContext db,
    CancellationToken ct) =>
{
    var msg = await db.OutboxMessages
        .Where(m => m.Topic == "ois.audit.logged")
        .OrderByDescending(m => m.CreatedAt)
        .ToListAsync(ct);

    foreach (var m in msg)
    {
        try
        {
            using var doc = JsonDocument.Parse(m.Payload);
            if (doc.RootElement.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.String)
            {
                if (Guid.TryParse(idEl.GetString(), out var aid) && aid == id)
                    return Results.Ok(MapAudit(m));
            }
        }
        catch { }
    }
    return Results.NotFound();
})
.WithName("GetAuditEvent")
.WithOpenApi();

auditApi.MapGet("/export.csv", async (
    Guid? actor,
    string? action,
    string? entity,
    DateTime? from,
    DateTime? to,
    ComplianceDbContext db,
    CancellationToken ct) =>
{
    var sb = new StringBuilder();
    // Stable header
    sb.AppendLine("id,actor,actorName,action,entity,entityId,result,timestamp,ip,userAgent");

    var rows = db.OutboxMessages
        .Where(m => m.Topic == "ois.audit.logged")
        .OrderBy(m => m.CreatedAt) // chronological for exports
        .AsEnumerable();

    foreach (var m in rows)
    {
        try
        {
            using var doc = JsonDocument.Parse(m.Payload);
            var r = doc.RootElement;
            var idStr = r.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
            var actorStr = r.TryGetProperty("actor", out var actEl) ? actEl.GetString() : null;
            var actorName = r.TryGetProperty("actorName", out var anEl) ? anEl.GetString() : null;
            var actionStr = r.TryGetProperty("action", out var acEl) ? acEl.GetString() : null;
            var entityStr = r.TryGetProperty("entity", out var enEl) ? enEl.GetString() : null;
            var entityId = r.TryGetProperty("entityId", out var eiEl) ? eiEl.GetString() : null;
            var result = r.TryGetProperty("result", out var resEl) ? resEl.GetString() : null;
            var ts = r.TryGetProperty("timestamp", out var tsEl) ? tsEl.GetString() : m.CreatedAt.ToString("O");
            var ip = r.TryGetProperty("ip", out var ipEl) ? ipEl.GetString() : null;
            var ua = r.TryGetProperty("userAgent", out var uaEl) ? uaEl.GetString() : null;

            // Filter checks
            if (actor.HasValue && (!Guid.TryParse(actorStr, out var a) || a != actor.Value)) continue;
            if (!string.IsNullOrEmpty(action) && !string.Equals(actionStr, action, StringComparison.OrdinalIgnoreCase)) continue;
            if (!string.IsNullOrEmpty(entity) && !string.Equals(entityStr, entity, StringComparison.OrdinalIgnoreCase)) continue;
            if (from.HasValue && DateTime.Parse(ts) < from.Value) continue;
            if (to.HasValue && DateTime.Parse(ts) > to.Value) continue;

            sb.AppendLine(string.Join(',', new[]
            {
                Csv(idStr), Csv(actorStr), Csv(actorName), Csv(actionStr), Csv(entityStr), Csv(entityId), Csv(result), Csv(ts), Csv(ip), Csv(ua)
            }));
        }
        catch { }
    }

    return Results.Text(sb.ToString(), "text/csv", Encoding.UTF8);
})
.WithName("ExportAuditCsv")
.WithOpenApi();

static string Csv(string? s)
{
    if (s is null) return "";
    var needsQuotes = s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r');
    var escaped = s.Replace("\"", "\"\"");
    return needsQuotes ? $"\"{escaped}\"" : escaped;
}

static object MapAudit(OutboxMessage m)
{
    try
    {
        using var doc = JsonDocument.Parse(m.Payload);
        var r = doc.RootElement;
        return new
        {
            id = Guid.TryParse(r.TryGetProperty("id", out var idEl) ? idEl.GetString() : null, out var aid) ? aid : m.Id,
            actor = r.TryGetProperty("actor", out var actEl) ? actEl.GetString() : null,
            actorName = r.TryGetProperty("actorName", out var anEl) ? anEl.GetString() : null,
            action = r.TryGetProperty("action", out var acEl) ? acEl.GetString() : null,
            entity = r.TryGetProperty("entity", out var enEl) ? enEl.GetString() : null,
            entityId = r.TryGetProperty("entityId", out var eiEl) ? eiEl.GetString() : null,
            payload = r.TryGetProperty("payload", out var pEl) ? pEl : default(JsonElement?),
            ip = r.TryGetProperty("ip", out var ipEl) ? ipEl.GetString() : null,
            userAgent = r.TryGetProperty("userAgent", out var uaEl) ? uaEl.GetString() : null,
            timestamp = r.TryGetProperty("timestamp", out var tsEl) ? tsEl.GetString() : m.CreatedAt.ToString("O"),
            result = r.TryGetProperty("result", out var resEl) ? resEl.GetString() : null
        };
    }
    catch
    {
        return new { id = m.Id, timestamp = m.CreatedAt.ToString("O") };
    }
}

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

public partial class Program { }
