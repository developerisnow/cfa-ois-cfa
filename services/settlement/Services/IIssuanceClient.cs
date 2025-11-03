using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OIS.Settlement.Services;

public interface IIssuanceClient
{
    Task<IssuanceInfo?> GetIssuanceAsync(Guid issuanceId, CancellationToken ct);
}

public record IssuanceInfo
{
    public Guid Id { get; init; }
    public Guid AssetId { get; init; }
    public Guid IssuerId { get; init; }
    public decimal TotalAmount { get; init; }
    public DateOnly IssueDate { get; init; }
    public DateOnly MaturityDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? ScheduleJson { get; init; }
}

public class IssuanceClient : IIssuanceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IssuanceClient> _logger;
    private readonly string _baseUrl;

    public IssuanceClient(HttpClient httpClient, ILogger<IssuanceClient> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["Issuance:BaseUrl"] ?? "http://issuance-service:8080";
    }

    public async Task<IssuanceInfo?> GetIssuanceAsync(Guid issuanceId, CancellationToken ct)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/v1/issuances/{issuanceId}", ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get issuance {IssuanceId}: {StatusCode}", issuanceId, response.StatusCode);
                return null;
            }

            var issuance = await response.Content.ReadFromJsonAsync<IssuanceInfo>(cancellationToken: ct);
            return issuance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting issuance {IssuanceId}", issuanceId);
            return null;
        }
    }
}

