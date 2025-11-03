namespace OIS.Issuance.DTOs;

public record IssuanceResponse
{
    public Guid Id { get; init; }
    public Guid AssetId { get; init; }
    public Guid IssuerId { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal Nominal { get; init; }
    public DateOnly IssueDate { get; init; }
    public DateOnly MaturityDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public Dictionary<string, object>? ScheduleJson { get; init; }
    public string? DltTxHash { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
}

