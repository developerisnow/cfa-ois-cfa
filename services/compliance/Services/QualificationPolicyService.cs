using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OIS.Compliance.Services;

public interface IQualificationPolicyService
{
    Task<QualificationTier> EvaluateTierAsync(Guid investorId, CancellationToken ct);
    decimal? GetLimitForTier(string tier);
}

public enum QualificationTier
{
    Unqualified,
    Qualified,
    Professional
}

public class QualificationPolicyService : IQualificationPolicyService
{
    private readonly ILogger<QualificationPolicyService> _logger;
    private readonly IConfiguration _configuration;

    public QualificationPolicyService(ILogger<QualificationPolicyService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public Task<QualificationTier> EvaluateTierAsync(Guid investorId, CancellationToken ct)
    {
        // Config-driven policy evaluation
        // For demo: deterministic based on investor ID
        var lastByte = investorId.ToByteArray().Last();
        var tier = lastByte switch
        {
            >= 200 => QualificationTier.Professional,
            >= 100 => QualificationTier.Qualified,
            _ => QualificationTier.Unqualified
        };

        _logger.LogInformation("Evaluated tier for investor {InvestorId}: {Tier}", investorId, tier);
        return Task.FromResult(tier);
    }

    public decimal? GetLimitForTier(string tier)
    {
        var limits = _configuration.GetSection("Qualification:Limits").Get<Dictionary<string, decimal?>>()
            ?? new Dictionary<string, decimal?>
            {
                { "unqualified", null },
                { "qualified", 60000m },
                { "professional", null } // unlimited
            };

        return limits.GetValueOrDefault(tier.ToLowerInvariant(), null);
    }
}

