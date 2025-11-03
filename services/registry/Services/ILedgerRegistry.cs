using Microsoft.Extensions.Configuration;

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
    private readonly bool _useMock;
    private readonly string? _chaincodeEndpoint;

    public LedgerRegistryAdapter(
        ILogger<LedgerRegistryAdapter> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _chaincodeEndpoint = _configuration["Ledger:ChaincodeEndpoint"];
        _useMock = string.IsNullOrEmpty(_chaincodeEndpoint) || 
                   _configuration.GetValue<bool>("Ledger:UseMock", true);

        if (_useMock)
        {
            _logger.LogWarning("Ledger registry adapter running in MOCK mode");
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
        // TODO: Implement
        throw new NotImplementedException("Real HLF integration not implemented");
    }

    private async Task<string> RealRedeemAsync(string holderId, Guid issuanceId, decimal amount, CancellationToken ct)
    {
        // TODO: Implement
        throw new NotImplementedException("Real HLF integration not implemented");
    }

    private static string GenerateMockTxHash()
    {
        var random = new Random();
        var bytes = new byte[32];
        random.NextBytes(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

