using FluentValidation;
using OIS.Registry.DTOs;

namespace OIS.Registry.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.InvestorId)
            .NotEmpty()
            .WithMessage("InvestorId is required");

        RuleFor(x => x.IssuanceId)
            .NotEmpty()
            .WithMessage("IssuanceId is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0");
    }
}

