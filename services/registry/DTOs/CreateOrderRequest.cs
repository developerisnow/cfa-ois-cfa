using System.ComponentModel.DataAnnotations;

namespace OIS.Registry.DTOs;

public record CreateOrderRequest
{
    [Required]
    public Guid InvestorId { get; init; }

    [Required]
    public Guid IssuanceId { get; init; }

    [Required]
    [Range(0.00000001, double.MaxValue)]
    public decimal Amount { get; init; }
}

