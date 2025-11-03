using System.ComponentModel.DataAnnotations;

namespace OIS.Issuance.DTOs;

public record CreateIssuanceRequest
{
    [Required]
    public Guid AssetId { get; init; }

    [Required]
    public Guid IssuerId { get; init; }

    [Required]
    [Range(0.00000001, double.MaxValue)]
    public decimal TotalAmount { get; init; }

    [Required]
    [Range(0.00000001, double.MaxValue)]
    public decimal Nominal { get; init; }

    [Required]
    public DateOnly IssueDate { get; init; }

    [Required]
    public DateOnly MaturityDate { get; init; }

    public Dictionary<string, object>? ScheduleJson { get; init; }
}

