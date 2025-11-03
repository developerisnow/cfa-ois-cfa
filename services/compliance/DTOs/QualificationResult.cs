namespace OIS.Compliance.DTOs;

public record QualificationResult
{
    public Guid InvestorId { get; init; }
    public string Tier { get; init; } = string.Empty; // unqualified, qualified, professional
    public decimal? Limit { get; init; }
    public decimal? Used { get; init; }
    public bool Allowed { get; init; }
    public string? Reason { get; init; }
    public DateTime EvaluatedAt { get; init; }
}

public record QualificationEvaluateRequest
{
    public Guid InvestorId { get; init; }
    public decimal Amount { get; init; }
}

