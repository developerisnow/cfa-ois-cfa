namespace OIS.Compliance.DTOs;

public record InvestorStatusResponse
{
    public Guid InvestorId { get; init; }
    public string Kyc { get; init; } = string.Empty;
    public string QualificationTier { get; init; } = string.Empty;
    public decimal? QualificationLimit { get; init; }
    public decimal? QualificationUsed { get; init; }
    public DateTime UpdatedAt { get; init; }
}

