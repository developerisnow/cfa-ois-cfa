namespace OIS.Issuance.Services;

/// <summary>
/// Interface for ledger operations (Hyperledger Fabric chaincode)
/// </summary>
public interface ILedgerIssuance
{
    /// <summary>
    /// Issue an issuance on the ledger
    /// </summary>
    Task<string> IssueAsync(
        Guid id,
        Guid assetId,
        Guid issuerId,
        decimal totalAmount,
        decimal nominal,
        DateOnly issueDate,
        DateOnly maturityDate,
        string? scheduleJson,
        CancellationToken ct);

    /// <summary>
    /// Close an issuance on the ledger
    /// </summary>
    Task<string> CloseAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Get issuance from ledger
    /// </summary>
    Task<LedgerIssuanceInfo?> GetAsync(Guid id, CancellationToken ct);
}

public record LedgerIssuanceInfo
{
    public string Status { get; init; } = string.Empty;
    public int Version { get; init; }
    public string? TransactionHash { get; init; }
}

