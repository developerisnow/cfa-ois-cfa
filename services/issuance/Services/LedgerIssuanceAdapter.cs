using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace OIS.Issuance.Services;

/// <summary>
/// Adapter for Hyperledger Fabric chaincode (with mock mode)
/// </summary>
public class LedgerIssuanceAdapter : ILedgerIssuance
{
    private readonly ILogger<LedgerIssuanceAdapter> _logger;
    private readonly IConfiguration _configuration;
    private readonly bool _useMock;
    private readonly string? _chaincodeEndpoint;

    public LedgerIssuanceAdapter(
        ILogger<LedgerIssuanceAdapter> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _chaincodeEndpoint = _configuration["Ledger:ChaincodeEndpoint"];
        _useMock = string.IsNullOrEmpty(_chaincodeEndpoint) || 
                   _configuration.GetValue<bool>("Ledger:UseMock", true);

        if (_useMock)
        {
            _logger.LogWarning("Ledger adapter running in MOCK mode");
        }
        else
        {
            _logger.LogInformation("Ledger adapter connected to {Endpoint}", _chaincodeEndpoint);
        }
    }

    public async Task<string> IssueAsync(
        Guid id,
        Guid assetId,
        Guid issuerId,
        decimal totalAmount,
        decimal nominal,
        DateOnly issueDate,
        DateOnly maturityDate,
        string? scheduleJson,
        CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (_useMock)
            {
                return await MockIssueAsync(id, ct);
            }

            return await RealIssueAsync(id, assetId, issuerId, totalAmount, nominal, issueDate, maturityDate, scheduleJson, ct);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "Ledger Issue completed for {IssuanceId} in {Duration}ms",
                id, stopwatch.ElapsedMilliseconds);
        }
    }

    public async Task<string> CloseAsync(Guid id, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (_useMock)
            {
                return await MockCloseAsync(id, ct);
            }

            return await RealCloseAsync(id, ct);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "Ledger Close completed for {IssuanceId} in {Duration}ms",
                id, stopwatch.ElapsedMilliseconds);
        }
    }

    public async Task<LedgerIssuanceInfo?> GetAsync(Guid id, CancellationToken ct)
    {
        if (_useMock)
        {
            return await MockGetAsync(id, ct);
        }

        return await RealGetAsync(id, ct);
    }

    private async Task<string> MockIssueAsync(Guid id, CancellationToken ct)
    {
        // Simulate network delay
        await Task.Delay(50, ct);

        // Generate mock transaction hash
        var txHash = GenerateMockTxHash();
        
        _logger.LogInformation("MOCK: Issued issuance {IssuanceId} with txHash {TxHash}", id, txHash);
        
        return txHash;
    }

    private async Task<string> MockCloseAsync(Guid id, CancellationToken ct)
    {
        // Simulate network delay
        await Task.Delay(50, ct);

        // Generate mock transaction hash
        var txHash = GenerateMockTxHash();
        
        _logger.LogInformation("MOCK: Closed issuance {IssuanceId} with txHash {TxHash}", id, txHash);
        
        return txHash;
    }

    private async Task<LedgerIssuanceInfo?> MockGetAsync(Guid id, CancellationToken ct)
    {
        await Task.Delay(30, ct);
        
        // Mock: assume issuance exists and is published
        return new LedgerIssuanceInfo
        {
            Status = "published",
            Version = 1,
            TransactionHash = GenerateMockTxHash()
        };
    }

    private async Task<string> RealIssueAsync(
        Guid id,
        Guid assetId,
        Guid issuerId,
        decimal totalAmount,
        decimal nominal,
        DateOnly issueDate,
        DateOnly maturityDate,
        string? scheduleJson,
        CancellationToken ct)
    {
        // TODO: Implement real Hyperledger Fabric client call
        // This would use Fabric SDK to invoke chaincode
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(30);

        var payload = new
        {
            id = id.ToString(),
            assetId = assetId.ToString(),
            issuerId = issuerId.ToString(),
            totalAmount = totalAmount,
            nominal = nominal,
            issueDate = issueDate.ToString("yyyy-MM-dd"),
            maturityDate = maturityDate.ToString("yyyy-MM-dd"),
            scheduleJson = scheduleJson ?? ""
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync($"{_chaincodeEndpoint}/invoke/Issue", content, ct);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<ChaincodeResponse>(responseContent);

        if (result?.TransactionHash == null)
        {
            throw new InvalidOperationException("Failed to get transaction hash from ledger");
        }

        _logger.LogInformation("Issued issuance {IssuanceId} on ledger with txHash {TxHash}", id, result.TransactionHash);

        return result.TransactionHash;
    }

    private async Task<string> RealCloseAsync(Guid id, CancellationToken ct)
    {
        // TODO: Implement real Hyperledger Fabric client call
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(30);

        var payload = new { id = id.ToString() };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync($"{_chaincodeEndpoint}/invoke/Close", content, ct);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<ChaincodeResponse>(responseContent);

        if (result?.TransactionHash == null)
        {
            throw new InvalidOperationException("Failed to get transaction hash from ledger");
        }

        _logger.LogInformation("Closed issuance {IssuanceId} on ledger with txHash {TxHash}", id, result.TransactionHash);

        return result.TransactionHash;
    }

    private async Task<LedgerIssuanceInfo?> RealGetAsync(Guid id, CancellationToken ct)
    {
        // TODO: Implement real Hyperledger Fabric client call
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(10);

        var response = await httpClient.GetAsync($"{_chaincodeEndpoint}/query/Get/{id}", ct);
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(ct);
        var issuance = JsonSerializer.Deserialize<ChaincodeIssuance>(content);

        if (issuance == null)
        {
            return null;
        }

        return new LedgerIssuanceInfo
        {
            Status = issuance.Status ?? "unknown",
            Version = issuance.Version,
            TransactionHash = issuance.TransactionHash
        };
    }

    private static string GenerateMockTxHash()
    {
        // Generate a mock transaction hash (64 hex characters)
        var random = new Random();
        var bytes = new byte[32];
        random.NextBytes(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private class ChaincodeResponse
    {
        public string? TransactionHash { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }
    }

    private class ChaincodeIssuance
    {
        public string? Status { get; set; }
        public int Version { get; set; }
        public string? TransactionHash { get; set; }
    }
}

