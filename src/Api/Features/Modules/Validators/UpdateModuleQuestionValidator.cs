using FluentValidation;

namespace Api.Features.Modules.Validators;

public class UpdateModuleQuestionValidator : AbstractValidator<UpdateModuleQuestionRequest>
{
    public UpdateModuleQuestionValidator()
    {
        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Display order must be greater than or equal to 0");
    }
}
