namespace OIS.Compliance.DTOs;

public record ComplaintResponse
{
    public Guid Id { get; init; }
    public Guid? InvestorId { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime? SlaDue { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
}

public record CreateComplaintRequest
{
    public Guid? InvestorId { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
}

