namespace OIS.Registry.DTOs;

public record OrderResponse
{
    public Guid Id { get; init; }
    public Guid InvestorId { get; init; }
    public Guid IssuanceId { get; init; }
    public decimal Amount { get; init; }
    public string Status { get; init; } = string.Empty;
    public Guid? WalletId { get; init; }
    public string? DltTxHash { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime? ConfirmedAt { get; init; }
    public string? FailureReason { get; init; }
}

