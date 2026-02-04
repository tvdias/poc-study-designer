using FluentValidation;

namespace Api.Features.CommissioningMarkets.Validators;

public class UpdateCommissioningMarketValidator : AbstractValidator<UpdateCommissioningMarketRequest>
{
    public UpdateCommissioningMarketValidator()
    {
        RuleFor(x => x.IsoCode)
            .NotEmpty().WithMessage("ISO code is required.")
            .MaximumLength(10).WithMessage("ISO code must not exceed 10 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Market name is required.")
            .MaximumLength(100).WithMessage("Market name must not exceed 100 characters.");
    }
}
