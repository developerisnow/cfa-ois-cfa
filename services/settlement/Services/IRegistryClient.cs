using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OIS.Settlement.Services;

public interface IRegistryClient
{
    Task<IReadOnlyList<HoldingInfo>> GetHoldingsByIssuanceAsync(Guid issuanceId, CancellationToken ct);
}

public record HoldingInfo
{
    public Guid InvestorId { get; init; }
    public Guid IssuanceId { get; init; }
    public decimal Quantity { get; init; }
}

public class RegistryClient : IRegistryClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RegistryClient> _logger;
    private readonly string _baseUrl;

    public RegistryClient(HttpClient httpClient, ILogger<RegistryClient> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["Registry:BaseUrl"] ?? "http://registry-service:8080";
    }

    public async Task<IReadOnlyList<HoldingInfo>> GetHoldingsByIssuanceAsync(Guid issuanceId, CancellationToken ct)
    {
        // TODO: Add endpoint to registry service for this query
        // For now, return empty list (mock)
        _logger.LogWarning("RegistryClient.GetHoldingsByIssuanceAsync is not yet implemented - returning empty list");
        return Array.Empty<HoldingInfo>();
    }
}

