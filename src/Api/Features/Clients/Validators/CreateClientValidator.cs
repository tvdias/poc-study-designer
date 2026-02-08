using FluentValidation;

namespace Api.Features.Clients.Validators;

public class CreateClientValidator : AbstractValidator<CreateClientRequest>
{
    public CreateClientValidator()
    {
        RuleFor(x => x.AccountName)
            .NotEmpty().WithMessage("Account name is required.")
            .MaximumLength(200).WithMessage("Account name must not exceed 200 characters.");

        RuleFor(x => x.CompanyNumber)
            .MaximumLength(50).WithMessage("Company number must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.CompanyNumber));

        RuleFor(x => x.CustomerNumber)
            .NotEmpty().WithMessage("Customer number is required.")
            .MaximumLength(50).WithMessage("Customer number must not exceed 50 characters.");

        RuleFor(x => x.CompanyCode)
            .MaximumLength(50).WithMessage("Company code must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.CompanyCode));
    }
}
