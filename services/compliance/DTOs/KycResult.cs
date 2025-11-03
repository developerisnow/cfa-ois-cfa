namespace OIS.Compliance.DTOs;

public record KycResult
{
    public Guid InvestorId { get; init; }
    public string Status { get; init; } = string.Empty; // pass, fail, pending, review
    public DateTime CheckedAt { get; init; }
    public string? Reason { get; init; }
}

public record KycCheckRequest
{
    public Guid InvestorId { get; init; }
}

