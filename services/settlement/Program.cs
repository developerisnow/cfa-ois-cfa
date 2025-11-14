using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OIS.Settlement;
using OIS.Settlement.DTOs;
using OIS.Settlement.Services;
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
        .AddService("settlement-service"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());

// Database
var settlementMigrationsAssembly = typeof(SettlementDbContext).Assembly.GetName().Name;
builder.Services.AddDbContext<SettlementDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly(settlementMigrationsAssembly)));

// HTTP Clients
builder.Services.AddHttpClient<IRegistryClient, RegistryClient>();
builder.Services.AddHttpClient<IIssuanceClient, IssuanceClient>();
builder.Services.AddHttpClient<IBankNominalClient, BankNominalClient>();

// Services
builder.Services.AddScoped<IOutboxService, OutboxService>();
builder.Services.AddScoped<ISettlementService, SettlementService>();

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
app.MapHealthChecks("/health");

// API Endpoints
var api = app.MapGroup("/v1").WithTags("Settlement");

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
.WithOpenApi();

app.Run();
