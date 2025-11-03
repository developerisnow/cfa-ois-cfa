namespace OIS.Registry.DTOs;

public record WalletResponse
{
    public Guid InvestorId { get; init; }
    public decimal Balance { get; init; }
    public decimal Blocked { get; init; }
    public IReadOnlyList<HoldingDto> Holdings { get; init; } = Array.Empty<HoldingDto>();
}

public record HoldingDto
{
    public Guid IssuanceId { get; init; }
    public decimal Quantity { get; init; }
    public DateTime UpdatedAt { get; init; }
}

