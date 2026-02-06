using FluentValidation;

namespace Api.Features.Modules.Validators;

public class UpdateModuleValidator : AbstractValidator<UpdateModuleRequest>
{
    public UpdateModuleValidator()
    {
        RuleFor(x => x.VariableName)
            .NotEmpty().WithMessage("Variable name is required.")
            .MaximumLength(100).WithMessage("Variable name must not exceed 100 characters.");

        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Label is required.")
            .MaximumLength(100).WithMessage("Label must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Instructions)
            .MaximumLength(2000).WithMessage("Instructions must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Instructions));

        RuleFor(x => x.VersionNumber)
            .GreaterThan(0).WithMessage("Version number must be greater than 0.");
    }
}
