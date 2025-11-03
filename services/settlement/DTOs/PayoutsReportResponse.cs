namespace OIS.Settlement.DTOs;

public record PayoutsReportResponse
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public int TotalBatches { get; init; }
    public decimal TotalAmount { get; init; }
    public int TotalItems { get; init; }
    public int CompletedItems { get; init; }
    public int FailedItems { get; init; }
    public IReadOnlyList<PayoutBatchDto> Batches { get; init; } = Array.Empty<PayoutBatchDto>();
}

public record PayoutBatchDto
{
    public Guid Id { get; init; }
    public DateOnly RunDate { get; init; }
    public Guid? IssuanceId { get; init; }
    public decimal TotalAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public int ItemCount { get; init; }
    public int CompletedCount { get; init; }
    public int FailedCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

