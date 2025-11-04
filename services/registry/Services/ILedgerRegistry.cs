using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Retry;

namespace OIS.Registry.Services;

public interface ILedgerRegistry
{
    Task<string> TransferAsync(string? from, string to, Guid issuanceId, decimal amount, CancellationToken ct);
    Task<string> RedeemAsync(string holderId, Guid issuanceId, decimal amount, CancellationToken ct);
}

public class LedgerRegistryAdapter : ILedgerRegistry
{
    private readonly ILogger<LedgerRegistryAdapter> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly bool _useMock;
    private readonly string? _chaincodeEndpoint;
    private readonly AsyncRetryPolicy _retryPolicy;

    public LedgerRegistryAdapter(
        ILogger<LedgerRegistryAdapter> logger,
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
            _logger.LogWarning("Ledger registry adapter running in MOCK mode");
        }
        else
        {
            _logger.LogInformation("Ledger registry adapter connected to {Endpoint}", _chaincodeEndpoint);
        }
    }

    public async Task<string> TransferAsync(string? from, string to, Guid issuanceId, decimal amount, CancellationToken ct)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            if (_useMock)
            {
                await Task.Delay(50, ct);
                var txHash = GenerateMockTxHash();
                _logger.LogInformation(
                    "MOCK: Transfer {From} -> {To}, issuance {IssuanceId}, amount {Amount}, txHash {TxHash}",
                    from ?? "null", to, issuanceId, amount, txHash);
                return txHash;
            }

            // TODO: Real HLF call
            return await RealTransferAsync(from, to, issuanceId, amount, ct);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "Ledger Transfer completed in {Duration}ms", stopwatch.ElapsedMilliseconds);
        }
    }

    public async Task<string> RedeemAsync(string holderId, Guid issuanceId, decimal amount, CancellationToken ct)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            if (_useMock)
            {
                await Task.Delay(50, ct);
                var txHash = GenerateMockTxHash();
                _logger.LogInformation(
                    "MOCK: Redeem holder {HolderId}, issuance {IssuanceId}, amount {Amount}, txHash {TxHash}",
                    holderId, issuanceId, amount, txHash);
                return txHash;
            }

            // TODO: Real HLF call
            return await RealRedeemAsync(holderId, issuanceId, amount, ct);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "Ledger Redeem completed in {Duration}ms", stopwatch.ElapsedMilliseconds);
        }
    }

    private async Task<string> RealTransferAsync(string? from, string to, Guid issuanceId, decimal amount, CancellationToken ct)
    {
        return await _retryPolicy.ExecuteAsync(async (context) =>
        {
            var payload = new
            {
                chaincode = "registry",
                function = "Transfer",
                args = new[]
                {
                    from ?? "",
                    to,
                    issuanceId.ToString(),
                    amount.ToString()
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/chaincode/invoke", content, ct);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(ct);
            var result = System.Text.Json.JsonSerializer.Deserialize<ChaincodeResponse>(responseContent);

            if (result?.TransactionHash == null)
            {
                throw new InvalidOperationException($"Failed to get transaction hash from ledger: {result?.Error ?? "Unknown error"}");
            }

            _logger.LogInformation(
                "Transfer {From} -> {To}, issuance {IssuanceId}, amount {Amount}, txHash {TxHash}",
                from ?? "null", to, issuanceId, amount, result.TransactionHash);

            return result.TransactionHash;
        }, new Context("Transfer"));
    }

    private async Task<string> RealRedeemAsync(string holderId, Guid issuanceId, decimal amount, CancellationToken ct)
    {
        return await _retryPolicy.ExecuteAsync(async (context) =>
        {
            var payload = new
            {
                chaincode = "registry",
                function = "Redeem",
                args = new[]
                {
                    holderId,
                    issuanceId.ToString(),
                    amount.ToString()
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/chaincode/invoke", content, ct);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(ct);
            var result = System.Text.Json.JsonSerializer.Deserialize<ChaincodeResponse>(responseContent);

            if (result?.TransactionHash == null)
            {
                throw new InvalidOperationException($"Failed to get transaction hash from ledger: {result?.Error ?? "Unknown error"}");
            }

            _logger.LogInformation(
                "Redeem holder {HolderId}, issuance {IssuanceId}, amount {Amount}, txHash {TxHash}",
                holderId, issuanceId, amount, result.TransactionHash);

            return result.TransactionHash;
        }, new Context("Redeem"));
    }

    private class ChaincodeResponse
    {
        public string? TransactionHash { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }
    }

    private static string GenerateMockTxHash()
    {
        var random = new Random();
        var bytes = new byte[32];
        random.NextBytes(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

