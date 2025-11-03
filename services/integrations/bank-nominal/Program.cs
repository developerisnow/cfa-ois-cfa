using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapHealthChecks("/health");

// Mock idempotency storage (in-memory for dev)
var idempotencyStore = new Dictionary<string, ReserveResponse>();

app.MapPost("/nominal/reserve", async ([FromBody] ReserveRequest request, HttpContext ctx) =>
{
    var idemKey = ctx.Request.Headers["Idempotency-Key"].FirstOrDefault();
    
    if (string.IsNullOrEmpty(idemKey))
    {
        return Results.BadRequest(new { error = "Idempotency-Key header required" });
    }

    // Check idempotency
    if (idempotencyStore.TryGetValue(idemKey, out var existing))
    {
        return Results.Ok(existing);
    }

    // Mock reserve
    await Task.Delay(30); // Simulate latency

    var response = new ReserveResponse
    {
        TransferId = Guid.NewGuid().ToString(),
        Status = "reserved"
    };

    idempotencyStore[idemKey] = response;

    return Results.Ok(response);
})
.WithName("ReserveFunds")
.WithOpenApi();

var batchStore = new Dictionary<string, BatchPayoutResponse>();

app.MapPost("/nominal/payouts/batch", async ([FromBody] BatchPayoutRequest request, HttpContext ctx) =>
{
    var idemKey = ctx.Request.Headers["Idempotency-Key"].FirstOrDefault();
    
    if (string.IsNullOrEmpty(idemKey))
    {
        return Results.BadRequest(new { error = "Idempotency-Key header required" });
    }

    // Check idempotency
    if (batchStore.TryGetValue(idemKey, out var existing))
    {
        return Results.Ok(existing);
    }

    // Mock batch payout
    await Task.Delay(50); // Simulate latency

    var batchRef = $"BATCH-{request.BatchId:N}";
    var items = request.Items.Select(item => new ItemPayoutResponse
    {
        ItemId = item.ItemId,
        BankRef = $"PAY-{item.ItemId:N}",
        Status = "completed",
        FailureReason = null
    }).ToList();

    var response = new BatchPayoutResponse
    {
        BatchRef = batchRef,
        Items = items
    };

    batchStore[idemKey] = response;

    return Results.Ok(response);
})
.WithName("BatchPayout")
.WithOpenApi();

app.Run();

record ReserveRequest
{
    public string? InvestorId { get; init; }
    public decimal Amount { get; init; }
    public string? IdempotencyKey { get; init; }
}

record ReserveResponse
{
    public string TransferId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}

record BatchPayoutRequest
{
    public Guid BatchId { get; init; }
    public DateOnly RunDate { get; init; }
    public IReadOnlyList<PayoutItemRequest> Items { get; init; } = Array.Empty<PayoutItemRequest>();
}

record PayoutItemRequest
{
    public Guid ItemId { get; init; }
    public Guid InvestorId { get; init; }
    public decimal Amount { get; init; }
}

record BatchPayoutResponse
{
    public string BatchRef { get; init; } = string.Empty;
    public IReadOnlyList<ItemPayoutResponse> Items { get; init; } = Array.Empty<ItemPayoutResponse>();
}

record ItemPayoutResponse
{
    public Guid ItemId { get; init; }
    public string? BankRef { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? FailureReason { get; init; }
}

