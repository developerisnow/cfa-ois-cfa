using Microsoft.EntityFrameworkCore;
using OIS.Domain;
using OIS.Issuance.DTOs;
using System.Text.Json;

namespace OIS.Issuance.Services;

public interface IIssuanceService
{
    Task<IssuanceResponse> CreateAsync(CreateIssuanceRequest request, CancellationToken ct);
    Task<IssuanceResponse?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IssuanceResponse> PublishAsync(Guid id, CancellationToken ct);
    Task<IssuanceResponse> CloseAsync(Guid id, CancellationToken ct);
}

public class IssuanceService : IIssuanceService
{
    private readonly IssuanceDbContext _db;
    private readonly ILogger<IssuanceService> _logger;
    private readonly IOutboxService _outbox;
    private readonly ILedgerIssuance _ledger;

    public IssuanceService(
        IssuanceDbContext db,
        ILogger<IssuanceService> logger,
        IOutboxService outbox,
        ILedgerIssuance ledger)
    {
        _db = db;
        _logger = logger;
        _outbox = outbox;
        _ledger = ledger;
    }

    public async Task<IssuanceResponse> CreateAsync(CreateIssuanceRequest request, CancellationToken ct)
    {
        var issuance = new IssuanceEntity
        {
            Id = Guid.NewGuid(),
            AssetId = request.AssetId,
            IssuerId = request.IssuerId,
            TotalAmount = request.TotalAmount,
            Nominal = request.Nominal,
            IssueDate = request.IssueDate,
            MaturityDate = request.MaturityDate,
            Status = IssuanceStatus.Draft,
            ScheduleJson = request.ScheduleJson != null 
                ? JsonSerializer.Serialize(request.ScheduleJson) 
                : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Issuances.Add(issuance);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created issuance {IssuanceId} for asset {AssetId}", issuance.Id, issuance.AssetId);

        return MapToResponse(issuance);
    }

    public async Task<IssuanceResponse?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var issuance = await _db.Issuances.FindAsync(new object[] { id }, ct);
        return issuance != null ? MapToResponse(issuance) : null;
    }

    public async Task<IssuanceResponse> PublishAsync(Guid id, CancellationToken ct)
    {
        var issuance = await _db.Issuances.FindAsync(new object[] { id }, ct);
        if (issuance == null)
            throw new InvalidOperationException($"Issuance {id} not found");

        if (issuance.Status != IssuanceStatus.Draft)
            throw new InvalidOperationException($"Cannot publish issuance in status {issuance.Status}");

        // Issue on ledger
        var scheduleJson = issuance.ScheduleJson;
        string txHash;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            txHash = await _ledger.IssueAsync(
                issuance.Id,
                issuance.AssetId,
                issuance.IssuerId,
                issuance.TotalAmount,
                issuance.Nominal,
                issuance.IssueDate,
                issuance.MaturityDate,
                scheduleJson,
                ct);

            stopwatch.Stop();
            _logger.LogInformation(
                "Ledger Issue successful for {IssuanceId}: txHash={TxHash}, duration={Duration}ms",
                issuance.Id, txHash, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Ledger Issue failed for {IssuanceId} after {Duration}ms",
                issuance.Id, stopwatch.ElapsedMilliseconds);
            throw new InvalidOperationException($"Failed to issue on ledger: {ex.Message}", ex);
        }

        // Update database
        issuance.Status = IssuanceStatus.Published;
        issuance.PublishedAt = DateTime.UtcNow;
        issuance.UpdatedAt = DateTime.UtcNow;
        issuance.DltTxHash = txHash;

        await _db.SaveChangesAsync(ct);

        // Publish event via outbox
        var schedule = scheduleJson != null
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(scheduleJson)
            : null;

        await _outbox.AddAsync("ois.issuance.published", new
        {
            issuanceId = issuance.Id,
            assetId = issuance.AssetId,
            issuerId = issuance.IssuerId,
            totalAmount = issuance.TotalAmount,
            schedule = schedule,
            publishedAt = issuance.PublishedAt,
            dltTxHash = txHash
        }, ct);

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Published issuance {IssuanceId} with txHash {TxHash}", issuance.Id, txHash);

        return MapToResponse(issuance);
    }

    public async Task<IssuanceResponse> CloseAsync(Guid id, CancellationToken ct)
    {
        var issuance = await _db.Issuances.FindAsync(new object[] { id }, ct);
        if (issuance == null)
            throw new InvalidOperationException($"Issuance {id} not found");

        if (issuance.Status != IssuanceStatus.Published)
            throw new InvalidOperationException($"Cannot close issuance in status {issuance.Status}");

        // Close on ledger
        string txHash;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            txHash = await _ledger.CloseAsync(issuance.Id, ct);

            stopwatch.Stop();
            _logger.LogInformation(
                "Ledger Close successful for {IssuanceId}: txHash={TxHash}, duration={Duration}ms",
                issuance.Id, txHash, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Ledger Close failed for {IssuanceId} after {Duration}ms",
                issuance.Id, stopwatch.ElapsedMilliseconds);
            throw new InvalidOperationException($"Failed to close on ledger: {ex.Message}", ex);
        }

        // Update database
        issuance.Status = IssuanceStatus.Closed;
        issuance.ClosedAt = DateTime.UtcNow;
        issuance.UpdatedAt = DateTime.UtcNow;
        issuance.DltTxHash = txHash; // Update with latest transaction hash

        await _db.SaveChangesAsync(ct);

        await _outbox.AddAsync("ois.issuance.closed", new
        {
            issuanceId = issuance.Id,
            closedAt = issuance.ClosedAt,
            dltTxHash = txHash
        }, ct);

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Closed issuance {IssuanceId} with txHash {TxHash}", issuance.Id, txHash);

        return MapToResponse(issuance);
    }

    private static IssuanceResponse MapToResponse(IssuanceEntity entity)
    {
        Dictionary<string, object>? scheduleJson = null;
        if (!string.IsNullOrEmpty(entity.ScheduleJson))
        {
            scheduleJson = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.ScheduleJson);
        }

        return new IssuanceResponse
        {
            Id = entity.Id,
            AssetId = entity.AssetId,
            IssuerId = entity.IssuerId,
            TotalAmount = entity.TotalAmount,
            Nominal = entity.Nominal,
            IssueDate = entity.IssueDate,
            MaturityDate = entity.MaturityDate,
            Status = entity.Status.ToStringValue(),
            ScheduleJson = scheduleJson,
            DltTxHash = entity.DltTxHash,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            PublishedAt = entity.PublishedAt,
            ClosedAt = entity.ClosedAt
        };
    }
}

