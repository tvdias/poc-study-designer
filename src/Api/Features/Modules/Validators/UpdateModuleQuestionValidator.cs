using FluentValidation;

namespace Api.Features.Modules.Validators;

public class UpdateModuleQuestionValidator : AbstractValidator<UpdateModuleQuestionRequest>
{
    public UpdateModuleQuestionValidator()
    {
        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Sort order must be greater than or equal to 0");
    }
}
