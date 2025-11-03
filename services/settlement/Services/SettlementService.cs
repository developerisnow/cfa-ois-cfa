using Microsoft.EntityFrameworkCore;
using OIS.Settlement;
using OIS.Settlement.DTOs;
using System.Diagnostics;
using System.Text.Json;

namespace OIS.Settlement.Services;

public interface ISettlementService
{
    Task<SettlementResponse> RunSettlementAsync(DateOnly? date, CancellationToken ct);
    Task<PayoutsReportResponse> GetPayoutsReportAsync(DateOnly from, DateOnly to, CancellationToken ct);
}

public class SettlementService : ISettlementService
{
    private readonly SettlementDbContext _db;
    private readonly ILogger<SettlementService> _logger;
    private readonly IRegistryClient _registry;
    private readonly IIssuanceClient _issuance;
    private readonly IBankNominalClient _bank;
    private readonly IOutboxService _outbox;

    public SettlementService(
        SettlementDbContext db,
        ILogger<SettlementService> logger,
        IRegistryClient registry,
        IIssuanceClient issuance,
        IBankNominalClient bank,
        IOutboxService outbox)
    {
        _db = db;
        _logger = logger;
        _registry = registry;
        _issuance = issuance;
        _bank = bank;
        _outbox = outbox;
    }

    public async Task<SettlementResponse> RunSettlementAsync(DateOnly? date, CancellationToken ct)
    {
        var runDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var idemKey = $"settlement-{runDate:yyyy-MM-dd}";

        // Check idempotency
        var existingBatch = await _db.PayoutBatches
            .FirstOrDefaultAsync(b => b.IdemKey == idemKey, ct);

        if (existingBatch != null)
        {
            _logger.LogInformation("Settlement batch for {RunDate} already exists: {BatchId}", runDate, existingBatch.Id);
            return MapToSettlementResponse(existingBatch);
        }

        var stopwatch = Stopwatch.StartNew();

        // (a) Find due issuances & holders from registry
        // TODO: Query registry for issuances with schedule items due on runDate
        // For now: mock - get all published issuances
        var issuanceIds = await GetDueIssuancesAsync(runDate, ct);
        _logger.LogInformation("Found {Count} issuances due for settlement on {RunDate}", issuanceIds.Count, runDate);

        if (issuanceIds.Count == 0)
        {
            _logger.LogInformation("No issuances due for settlement on {RunDate}", runDate);
            throw new InvalidOperationException($"No issuances due for settlement on {runDate:yyyy-MM-dd}");
        }

        var batch = new PayoutBatchEntity
        {
            Id = Guid.NewGuid(),
            RunDate = runDate,
            Status = "pending",
            IdemKey = idemKey,
            CreatedAt = DateTime.UtcNow
        };

        var items = new List<PayoutItemEntity>();
        decimal totalAmount = 0;

        foreach (var issuanceId in issuanceIds)
        {
            // Get issuance schedule
            var issuance = await _issuance.GetIssuanceAsync(issuanceId, ct);
            if (issuance == null || issuance.ScheduleJson == null)
            {
                _logger.LogWarning("Issuance {IssuanceId} not found or has no schedule", issuanceId);
                continue;
            }

            // Parse schedule and find due items
            var dueAmount = CalculateDueAmount(issuance.ScheduleJson, runDate);
            if (dueAmount <= 0)
                continue;

            // Get holders for this issuance
            var holdings = await _registry.GetHoldingsByIssuanceAsync(issuanceId, ct);
            _logger.LogInformation("Found {Count} holders for issuance {IssuanceId}", holdings.Count, issuanceId);

            foreach (var holding in holdings)
            {
                // Calculate payout per holder (proportional to quantity)
                var payoutAmount = dueAmount * (holding.Quantity / issuance.TotalAmount);
                if (payoutAmount <= 0)
                    continue;

                var itemId = GenerateDeterministicId(batch.Id, holding.InvestorId, issuanceId);
                items.Add(new PayoutItemEntity
                {
                    Id = itemId,
                    BatchId = batch.Id,
                    InvestorId = holding.InvestorId,
                    Amount = payoutAmount,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow
                });

                totalAmount += payoutAmount;
            }

            if (batch.IssuanceId == null)
                batch.IssuanceId = issuanceId;
        }

        batch.TotalAmount = totalAmount;
        batch.Status = "processing";

        _db.PayoutBatches.Add(batch);
        _db.PayoutItems.AddRange(items);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created batch {BatchId} with {ItemCount} items, total amount {TotalAmount}",
            batch.Id, items.Count, totalAmount);

        // (c) Call bank-nominal: payout batch (idempotent)
        try
        {
            var bankRequest = new BatchPayoutRequest
            {
                BatchId = batch.Id,
                RunDate = runDate,
                Items = items.Select(i => new PayoutItemRequest
                {
                    ItemId = i.Id,
                    InvestorId = i.InvestorId,
                    Amount = i.Amount
                }).ToList()
            };

            var bankResponse = await _bank.ExecuteBatchPayoutAsync(bankRequest, idemKey, ct);
            _logger.LogInformation("Bank nominal batch payout completed: batchRef={BatchRef}, items={ItemCount}",
                bankResponse.BatchRef, bankResponse.Items.Count);

            // Update items with bank refs
            var itemMap = bankResponse.Items.ToDictionary(i => i.ItemId);
            foreach (var item in items)
            {
                if (itemMap.TryGetValue(item.Id, out var bankItem))
                {
                    item.BankRef = bankItem.BankRef;
                    item.Status = bankItem.Status;
                    item.FailureReason = bankItem.FailureReason;
                }
            }

            // (d) Mark on ledger via registry (record payout tx)
            // TODO: Call registry to mark payouts on ledger

            // (e) Emit event and write reconciliation log
            var completedCount = items.Count(i => i.Status == "completed");
            var failedCount = items.Count(i => i.Status == "failed");

            batch.Status = failedCount == items.Count ? "failed" : (completedCount == items.Count ? "completed" : "processing");

            await _outbox.AddAsync("ois.payout.executed", new
            {
                batchId = batch.Id,
                issuanceId = batch.IssuanceId,
                executedAt = DateTime.UtcNow,
                totalAmount = totalAmount,
                itemCount = items.Count,
                items = items.Select(i => new
                {
                    itemId = i.Id,
                    investorId = i.InvestorId,
                    amount = i.Amount,
                    status = i.Status,
                    bankRef = i.BankRef,
                    dltTxHash = i.DltTxHash,
                    failureReason = i.FailureReason
                }).ToList()
            }, ct);

            // Write reconciliation log
            await WriteReconciliationLogAsync(batch.Id, bankResponse, ct);

            await _db.SaveChangesAsync(ct);

            stopwatch.Stop();
            _logger.LogInformation("Settlement batch {BatchId} completed in {Duration}ms: {Completed}/{Total} items",
                batch.Id, stopwatch.ElapsedMilliseconds, completedCount, items.Count);

            return MapToSettlementResponse(batch, items.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Settlement batch {BatchId} failed", batch.Id);
            batch.Status = "failed";
            await _db.SaveChangesAsync(ct);
            throw;
        }
    }

    public async Task<PayoutsReportResponse> GetPayoutsReportAsync(DateOnly from, DateOnly to, CancellationToken ct)
    {
        var batches = await _db.PayoutBatches
            .Where(b => b.RunDate >= from && b.RunDate <= to)
            .ToListAsync(ct);

        var batchIds = batches.Select(b => b.Id).ToList();

        var items = await _db.PayoutItems
            .Where(i => batchIds.Contains(i.BatchId))
            .ToListAsync(ct);

        var itemGroups = items.GroupBy(i => i.BatchId).ToDictionary(g => g.Key, g => g.ToList());

        var batchDtos = batches.Select(b =>
        {
            var batchItems = itemGroups.GetValueOrDefault(b.Id, new List<PayoutItemEntity>());
            return new PayoutBatchDto
            {
                Id = b.Id,
                RunDate = b.RunDate,
                IssuanceId = b.IssuanceId,
                TotalAmount = b.TotalAmount,
                Status = b.Status,
                ItemCount = batchItems.Count,
                CompletedCount = batchItems.Count(i => i.Status == "completed"),
                FailedCount = batchItems.Count(i => i.Status == "failed"),
                CreatedAt = b.CreatedAt
            };
        }).ToList();

        var allItems = items.ToList();
        return new PayoutsReportResponse
        {
            From = from,
            To = to,
            TotalBatches = batches.Count,
            TotalAmount = batches.Sum(b => b.TotalAmount),
            TotalItems = allItems.Count,
            CompletedItems = allItems.Count(i => i.Status == "completed"),
            FailedItems = allItems.Count(i => i.Status == "failed"),
            Batches = batchDtos
        };
    }

    private async Task<List<Guid>> GetDueIssuancesAsync(DateOnly runDate, CancellationToken ct)
    {
        // TODO: Query issuance service for published issuances with schedule items due on runDate
        // Mock for now - return empty list
        return new List<Guid>();
    }

    private decimal CalculateDueAmount(string scheduleJson, DateOnly runDate)
    {
        try
        {
            var schedule = JsonSerializer.Deserialize<ScheduleDto>(scheduleJson);
            if (schedule?.Items == null)
                return 0;

            var dueItem = schedule.Items.FirstOrDefault(i => i.Date == runDate);
            return dueItem?.Amount ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse schedule JSON");
            return 0;
        }
    }

    private Guid GenerateDeterministicId(Guid batchId, Guid investorId, Guid issuanceId)
    {
        // Deterministic ID generation based on batch, investor, and issuance
        var input = $"{batchId}:{investorId}:{issuanceId}";
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(input));
        return new Guid(hash.Take(16).ToArray());
    }

    private async Task WriteReconciliationLogAsync(Guid batchId, BatchPayoutResponse bankResponse, CancellationToken ct)
    {
        var log = new ReconciliationLogEntity
        {
            Id = Guid.NewGuid(),
            BatchId = batchId,
            PayloadJson = JsonSerializer.Serialize(new
            {
                batchRef = bankResponse.BatchRef,
                items = bankResponse.Items.Select(i => new
                {
                    itemId = i.ItemId,
                    bankRef = i.BankRef,
                    status = i.Status,
                    failureReason = i.FailureReason
                })
            }),
            CreatedAt = DateTime.UtcNow
        };

        _db.ReconciliationLogs.Add(log);
    }

    private static SettlementResponse MapToSettlementResponse(PayoutBatchEntity batch, int itemCount = 0)
    {
        return new SettlementResponse
        {
            BatchId = batch.Id,
            RunDate = batch.RunDate,
            IssuanceId = batch.IssuanceId,
            TotalAmount = batch.TotalAmount,
            Status = batch.Status,
            ItemCount = itemCount,
            CreatedAt = batch.CreatedAt
        };
    }

    private record ScheduleDto
    {
        public List<ScheduleItemDto>? Items { get; init; }
    }

    private record ScheduleItemDto
    {
        public DateOnly Date { get; init; }
        public decimal Amount { get; init; }
    }
}

public interface IOutboxService
{
    Task AddAsync(string topic, object payload, CancellationToken ct);
}

public class OutboxService : IOutboxService
{
    private readonly SettlementDbContext _db;

    public OutboxService(SettlementDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(string topic, object payload, CancellationToken ct)
    {
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Topic = topic,
            Payload = JsonSerializer.Serialize(payload),
            CreatedAt = DateTime.UtcNow
        };

        _db.OutboxMessages.Add(message);
    }
}

