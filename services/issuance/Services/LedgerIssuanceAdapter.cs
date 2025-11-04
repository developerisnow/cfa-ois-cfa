using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Retry;

namespace OIS.Issuance.Services;

/// <summary>
/// Adapter for Hyperledger Fabric chaincode (with mock mode)
/// </summary>
public class LedgerIssuanceAdapter : ILedgerIssuance
{
    private readonly ILogger<LedgerIssuanceAdapter> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly bool _useMock;
    private readonly string? _chaincodeEndpoint;
    private readonly AsyncRetryPolicy _retryPolicy;

    public LedgerIssuanceAdapter(
        ILogger<LedgerIssuanceAdapter> logger,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
        _chaincodeEndpoint = _configuration["Ledger:ChaincodeEndpoint"];
        _useMock = string.IsNullOrEmpty(_chaincodeEndpoint) || 
                   _configuration.GetValue<bool>("Ledger:UseMock", true);

        if (!_useMock && !string.IsNullOrEmpty(_chaincodeEndpoint))
        {
            _httpClient.BaseAddress = new Uri(_chaincodeEndpoint);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        // Retry policy with exponential backoff
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Retry {RetryCount} after {Delay}ms for {Operation}",
                        retryCount, timeSpan.TotalMilliseconds, context.OperationKey);
                });

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
        return await _retryPolicy.ExecuteAsync(async (context) =>
        {
            var payload = new
            {
                chaincode = "issuance",
                function = "Issue",
                args = new[]
                {
                    id.ToString(),
                    assetId.ToString(),
                    issuerId.ToString(),
                    totalAmount.ToString(),
                    nominal.ToString(),
                    issueDate.ToString("yyyy-MM-dd"),
                    maturityDate.ToString("yyyy-MM-dd"),
                    scheduleJson ?? "{}"
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/chaincode/invoke", content, ct);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<ChaincodeResponse>(responseContent);

            if (result?.TransactionHash == null)
            {
                throw new InvalidOperationException($"Failed to get transaction hash from ledger: {result?.Error ?? "Unknown error"}");
            }

            _logger.LogInformation("Issued issuance {IssuanceId} on ledger with txHash {TxHash}", id, result.TransactionHash);

            return result.TransactionHash;
        }, new Context("Issue"));
    }

    private async Task<string> RealCloseAsync(Guid id, CancellationToken ct)
    {
        return await _retryPolicy.ExecuteAsync(async (context) =>
        {
            var payload = new
            {
                chaincode = "issuance",
                function = "Close",
                args = new[] { id.ToString() }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/chaincode/invoke", content, ct);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<ChaincodeResponse>(responseContent);

            if (result?.TransactionHash == null)
            {
                throw new InvalidOperationException($"Failed to get transaction hash from ledger: {result?.Error ?? "Unknown error"}");
            }

            _logger.LogInformation("Closed issuance {IssuanceId} on ledger with txHash {TxHash}", id, result.TransactionHash);

            return result.TransactionHash;
        }, new Context("Close"));
    }

    private async Task<LedgerIssuanceInfo?> RealGetAsync(Guid id, CancellationToken ct)
    {
        return await _retryPolicy.ExecuteAsync(async (context) =>
        {
            var payload = new
            {
                chaincode = "issuance",
                function = "Get",
                args = new[] { id.ToString() }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/chaincode/query", content, ct);
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(ct);
            var issuance = JsonSerializer.Deserialize<ChaincodeIssuance>(responseContent);

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
        }, new Context("Get"));
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

