using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace OIS.Registry.Services;

public interface IComplianceService
{
    Task<bool> CheckKycAsync(Guid investorId, CancellationToken ct);
    Task<bool> CheckQualificationAsync(Guid investorId, decimal amount, CancellationToken ct);
}

public class ComplianceServiceClient : IComplianceService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ComplianceServiceClient> _logger;
    private readonly string _baseUrl;

    public ComplianceServiceClient(
        HttpClient httpClient,
        ILogger<ComplianceServiceClient> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["Compliance:BaseUrl"] ?? "http://compliance-service:8080";
    }

    public async Task<bool> CheckKycAsync(Guid investorId, CancellationToken ct)
    {
        try
        {
            var request = new { investorId = investorId };
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/v1/compliance/kyc/check", request, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("KYC check failed for investor {InvestorId}: {StatusCode}", investorId, response.StatusCode);
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<KycResult>(cancellationToken: ct);
            return result?.Status == "pass";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking KYC for investor {InvestorId}", investorId);
            return false;
        }
    }

    public async Task<bool> CheckQualificationAsync(Guid investorId, decimal amount, CancellationToken ct)
    {
        try
        {
            var request = new { investorId = investorId, amount = amount };
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/v1/compliance/qualification/evaluate", request, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Qualification check failed for investor {InvestorId}: {StatusCode}", investorId, response.StatusCode);
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<QualificationResult>(cancellationToken: ct);
            return result?.Allowed == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking qualification for investor {InvestorId}", investorId);
            return false;
        }
    }

    private record KycResult
    {
        public string Status { get; init; } = string.Empty;
    }

    private record QualificationResult
    {
        public bool Allowed { get; init; }
    }
}

