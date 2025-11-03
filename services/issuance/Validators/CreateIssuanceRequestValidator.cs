using FluentValidation;
using OIS.Issuance.DTOs;

namespace OIS.Issuance.Validators;

public class CreateIssuanceRequestValidator : AbstractValidator<CreateIssuanceRequest>
{
    public CreateIssuanceRequestValidator()
    {
        RuleFor(x => x.AssetId)
            .NotEmpty()
            .WithMessage("AssetId is required");

        RuleFor(x => x.IssuerId)
            .NotEmpty()
            .WithMessage("IssuerId is required");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0)
            .WithMessage("TotalAmount must be greater than 0");

        RuleFor(x => x.Nominal)
            .GreaterThan(0)
            .WithMessage("Nominal must be greater than 0");

        RuleFor(x => x.IssueDate)
            .NotEmpty()
            .WithMessage("IssueDate is required");

        RuleFor(x => x.MaturityDate)
            .NotEmpty()
            .WithMessage("MaturityDate is required")
            .GreaterThan(x => x.IssueDate)
            .WithMessage("MaturityDate must be after IssueDate");
    }
}

