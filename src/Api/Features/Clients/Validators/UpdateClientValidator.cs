using FluentValidation;

namespace Api.Features.Clients.Validators;

public class UpdateClientValidator : AbstractValidator<UpdateClientRequest>
{
    public UpdateClientValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Client name is required.")
            .MaximumLength(200).WithMessage("Client name must not exceed 200 characters.");

        RuleFor(x => x.IntegrationMetadata)
            .MaximumLength(1000).WithMessage("Integration metadata must not exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.IntegrationMetadata));

        RuleFor(x => x.ProductsModules)
            .MaximumLength(500).WithMessage("Products/modules must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.ProductsModules));
    }
}
