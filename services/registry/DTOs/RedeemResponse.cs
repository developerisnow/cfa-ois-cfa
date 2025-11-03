namespace OIS.Registry.DTOs;

public record RedeemResponse
{
    public Guid IssuanceId { get; init; }
    public decimal RedeemedAmount { get; init; }
    public string DltTxHash { get; init; } = string.Empty;
    public DateTime RedeemedAt { get; init; }
}

