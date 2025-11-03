namespace OIS.Settlement.DTOs;

public record SettlementResponse
{
    public Guid BatchId { get; init; }
    public DateOnly RunDate { get; init; }
    public Guid? IssuanceId { get; init; }
    public decimal TotalAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public int ItemCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

