using System.ComponentModel.DataAnnotations;

namespace OIS.Registry.DTOs;

public record RedeemRequest
{
    [Required]
    [Range(0.00000001, double.MaxValue)]
    public decimal Amount { get; init; }
}

