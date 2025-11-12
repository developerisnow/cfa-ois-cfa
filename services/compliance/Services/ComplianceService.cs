using Microsoft.EntityFrameworkCore;
using OIS.Compliance;
using OIS.Compliance.DTOs;
using System.Text.Json;

namespace OIS.Compliance.Services;

public interface IComplianceService
{
    Task<KycResult> CheckKycAsync(KycCheckRequest request, CancellationToken ct);
    Task<QualificationResult> EvaluateQualificationAsync(QualificationEvaluateRequest request, CancellationToken ct);
    Task<InvestorStatusResponse?> GetInvestorStatusAsync(Guid investorId, CancellationToken ct);
    Task<ComplaintResponse> CreateComplaintAsync(CreateComplaintRequest request, string? idempotencyKey, CancellationToken ct);
    Task<ComplaintResponse?> GetComplaintAsync(Guid id, CancellationToken ct);
    Task<KycResult> UpdateKycStatusAsync(Guid investorId, string status, Guid? actorId, string? reason, CancellationToken ct);
    Task<KycTaskDto> CreateKycTaskAsync(Guid investorId, string? reason, CancellationToken ct);
    Task<IReadOnlyList<KycTaskDto>> ListKycTasksAsync(string? status, CancellationToken ct);
    Task<KycTaskDto?> ResolveKycTaskAsync(Guid taskId, string action, Guid? actorId, string? reason, CancellationToken ct);
}

public class ComplianceService : IComplianceService
{
    private readonly ComplianceDbContext _db;
    private readonly ILogger<ComplianceService> _logger;
    private readonly IWatchlistsService _watchlists;
    private readonly IQualificationPolicyService _qualificationPolicy;
    private readonly IOutboxService _outbox;

    public ComplianceService(
        ComplianceDbContext db,
        ILogger<ComplianceService> logger,
        IWatchlistsService watchlists,
        IQualificationPolicyService qualificationPolicy,
        IOutboxService outbox)
    {
        _db = db;
        _logger = logger;
        _watchlists = watchlists;
        _qualificationPolicy = qualificationPolicy;
        _outbox = outbox;
    }

    public async Task<KycResult> UpdateKycStatusAsync(Guid investorId, string status, Guid? actorId, string? reason, CancellationToken ct)
    {
        var compliance = await GetOrCreateComplianceAsync(investorId, ct);

        if (status is not ("pass" or "fail" or "pending" or "review"))
            throw new InvalidOperationException($"Unsupported KYC status '{status}'");

        compliance.Kyc = status;
        compliance.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        // Events: kyc.updated
        await _outbox.AddAsync("ois.kyc.updated", new
        {
            investorId,
            status,
            reason,
            updatedAt = compliance.UpdatedAt
        }, ct);

        // Audit
        await _outbox.AddAsync("ois.audit.logged", new
        {
            id = Guid.NewGuid(),
            actor = actorId,
            action = "kyc.update",
            entity = "investor",
            entityId = investorId,
            payload = new { status, reason },
            timestamp = DateTime.UtcNow,
            result = "success"
        }, ct);

        await _db.SaveChangesAsync(ct);

        return new KycResult
        {
            InvestorId = investorId,
            Status = compliance.Kyc,
            CheckedAt = compliance.UpdatedAt,
            Reason = reason
        };
    }

    public async Task<KycTaskDto> CreateKycTaskAsync(Guid investorId, string? reason, CancellationToken ct)
    {
        var task = new KycTaskEntity
        {
            Id = Guid.NewGuid(),
            InvestorId = investorId,
            Status = "open",
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };
        _db.KycTasks.Add(task);
        await _db.SaveChangesAsync(ct);
        return MapToDto(task);
    }

    public async Task<IReadOnlyList<KycTaskDto>> ListKycTasksAsync(string? status, CancellationToken ct)
    {
        var query = _db.KycTasks.AsQueryable();
        if (!string.IsNullOrEmpty(status))
            query = query.Where(t => t.Status == status);
        var items = await query.OrderBy(t => t.CreatedAt).ToListAsync(ct);
        return items.Select(MapToDto).ToList();
    }

    public async Task<KycTaskDto?> ResolveKycTaskAsync(Guid taskId, string action, Guid? actorId, string? reason, CancellationToken ct)
    {
        var task = await _db.KycTasks.FindAsync(new object[] { taskId }, ct);
        if (task == null) return null;
        if (task.Status != "open") return MapToDto(task);

        string status = action == "approve" ? "pass" : action == "reject" ? "fail" : throw new InvalidOperationException("Unknown action");
        task.Status = action == "approve" ? "approved" : "rejected";
        task.ResolvedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        // Update KYC
        await UpdateKycStatusAsync(task.InvestorId, status, actorId, reason, ct);
        return MapToDto(task);
    }

    private static KycTaskDto MapToDto(KycTaskEntity e) => new()
    {
        Id = e.Id,
        InvestorId = e.InvestorId,
        Status = e.Status,
        Reason = e.Reason,
        CreatedAt = e.CreatedAt,
        ResolvedAt = e.ResolvedAt
    };

    public async Task<KycResult> CheckKycAsync(KycCheckRequest request, CancellationToken ct)
    {
        var compliance = await GetOrCreateComplianceAsync(request.InvestorId, ct);

        // Check watchlists
        var watchlistResult = await _watchlists.CheckAsync(request.InvestorId, ct);
        if (watchlistResult.Matched)
        {
            compliance.Kyc = "fail";
            compliance.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            // Emit flagged event
            await _outbox.AddAsync("ois.compliance.flagged", new
            {
                investorId = request.InvestorId,
                reason = "watchlist_match",
                severity = watchlistResult.Severity,
                flaggedAt = DateTime.UtcNow,
                details = new { watchlistReason = watchlistResult.Reason }
            }, ct);

            await _db.SaveChangesAsync(ct);

            return new KycResult
            {
                InvestorId = request.InvestorId,
                Status = "fail",
                CheckedAt = DateTime.UtcNow,
                Reason = watchlistResult.Reason ?? "Watchlist match"
            };
        }

        // If KYC is already pass, return it
        if (compliance.Kyc == "pass")
        {
            return new KycResult
            {
                InvestorId = request.InvestorId,
                Status = "pass",
                CheckedAt = compliance.UpdatedAt,
                Reason = null
            };
        }

        // For demo: set KYC to pass if not already set
        if (compliance.Kyc == "pending")
        {
            compliance.Kyc = "pass";
            compliance.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        return new KycResult
        {
            InvestorId = request.InvestorId,
            Status = compliance.Kyc,
            CheckedAt = compliance.UpdatedAt,
            Reason = null
        };
    }

    public async Task<QualificationResult> EvaluateQualificationAsync(QualificationEvaluateRequest request, CancellationToken ct)
    {
        var compliance = await GetOrCreateComplianceAsync(request.InvestorId, ct);

        // Evaluate tier if not set
        if (compliance.QualificationTier == "unqualified")
        {
            var tier = await _qualificationPolicy.EvaluateTierAsync(request.InvestorId, ct);
            compliance.QualificationTier = tier.ToString().ToLowerInvariant();
            compliance.QualLimit = _qualificationPolicy.GetLimitForTier(compliance.QualificationTier);
            compliance.QualUsed = 0;
            compliance.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        // Check if amount is allowed
        var limit = compliance.QualLimit;
        var used = compliance.QualUsed ?? 0;
        var allowed = true;
        string? reason = null;

        if (limit.HasValue && used + request.Amount > limit.Value)
        {
            allowed = false;
            reason = $"Qualification limit exceeded: used {used}, limit {limit}, requested {request.Amount}";

            // Emit flagged event
            await _outbox.AddAsync("ois.compliance.flagged", new
            {
                investorId = request.InvestorId,
                reason = "qualification_exceeded",
                severity = "high",
                flaggedAt = DateTime.UtcNow,
                details = new { used = used, limit = limit, requested = request.Amount }
            }, ct);

            await _db.SaveChangesAsync(ct);
        }

        return new QualificationResult
        {
            InvestorId = request.InvestorId,
            Tier = compliance.QualificationTier,
            Limit = compliance.QualLimit,
            Used = compliance.QualUsed,
            Allowed = allowed,
            Reason = reason,
            EvaluatedAt = DateTime.UtcNow
        };
    }

    public async Task<InvestorStatusResponse?> GetInvestorStatusAsync(Guid investorId, CancellationToken ct)
    {
        var compliance = await _db.InvestorsCompliance
            .FirstOrDefaultAsync(c => c.InvestorId == investorId, ct);

        if (compliance == null)
            return null;

        return new InvestorStatusResponse
        {
            InvestorId = compliance.InvestorId,
            Kyc = compliance.Kyc,
            QualificationTier = compliance.QualificationTier,
            QualificationLimit = compliance.QualLimit,
            QualificationUsed = compliance.QualUsed,
            UpdatedAt = compliance.UpdatedAt
        };
    }

    public async Task<ComplaintResponse> CreateComplaintAsync(CreateComplaintRequest request, string? idempotencyKey, CancellationToken ct)
    {
        // Check idempotency if key provided
        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            var existing = await _db.Complaints
                .FirstOrDefaultAsync(c => c.IdemKey == idempotencyKey, ct);

            if (existing != null)
            {
                _logger.LogInformation("Complaint with idempotency key {IdemKey} already exists: {ComplaintId}",
                    idempotencyKey, existing.Id);
                return MapToComplaintResponse(existing);
            }
        }

        var complaint = new ComplaintEntity
        {
            Id = Guid.NewGuid(),
            InvestorId = request.InvestorId,
            Category = request.Category,
            Text = request.Text,
            Status = "open",
            SlaDue = DateTime.UtcNow.AddDays(7), // 7-day SLA
            CreatedAt = DateTime.UtcNow,
            IdemKey = idempotencyKey
        };

        _db.Complaints.Add(complaint);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created complaint {ComplaintId} for investor {InvestorId}, category {Category}",
            complaint.Id, request.InvestorId, request.Category);

        return MapToComplaintResponse(complaint);
    }

    public async Task<ComplaintResponse?> GetComplaintAsync(Guid id, CancellationToken ct)
    {
        var complaint = await _db.Complaints.FindAsync(new object[] { id }, ct);
        return complaint != null ? MapToComplaintResponse(complaint) : null;
    }

    private async Task<InvestorComplianceEntity> GetOrCreateComplianceAsync(Guid investorId, CancellationToken ct)
    {
        var compliance = await _db.InvestorsCompliance
            .FirstOrDefaultAsync(c => c.InvestorId == investorId, ct);

        if (compliance == null)
        {
            compliance = new InvestorComplianceEntity
            {
                InvestorId = investorId,
                Kyc = "pending",
                QualificationTier = "unqualified",
                UpdatedAt = DateTime.UtcNow
            };
            _db.InvestorsCompliance.Add(compliance);
            await _db.SaveChangesAsync(ct);
        }

        return compliance;
    }

    private static ComplaintResponse MapToComplaintResponse(ComplaintEntity entity)
    {
        return new ComplaintResponse
        {
            Id = entity.Id,
            InvestorId = entity.InvestorId,
            Category = entity.Category,
            Text = entity.Text,
            Status = entity.Status,
            SlaDue = entity.SlaDue,
            CreatedAt = entity.CreatedAt,
            ResolvedAt = entity.ResolvedAt
        };
    }
}

public record KycTaskDto
{
    public Guid Id { get; init; }
    public Guid InvestorId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
}

public record CreateKycTaskRequest
{
    public Guid InvestorId { get; init; }
    public string? Reason { get; init; }
}

public interface IOutboxService
{
    Task AddAsync(string topic, object payload, CancellationToken ct);
}

public class OutboxService : IOutboxService
{
    private readonly ComplianceDbContext _db;

    public OutboxService(ComplianceDbContext db)
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

