using Microsoft.EntityFrameworkCore;
using FluentValidation;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OIS.Registry;
using OIS.Registry.DTOs;
using OIS.Registry.Services;
using OIS.Registry.Validators;
using Serilog;
using System.Diagnostics;

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
        .AddConsoleExporter());

// Database
builder.Services.AddDbContext<RegistryDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

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
// Auto validation can be enabled once package alignment confirmed
// builder.Services.AddFluentValidationAutoValidation();

// API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

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
app.MapHealthChecks("/health");

// API Endpoints
var api = app.MapGroup("/v1").WithTags("Registry");

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
.WithOpenApi();

app.Run();
