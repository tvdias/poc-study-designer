using FluentValidation;

namespace Api.Features.Modules.Validators;

public class AddQuestionToModuleValidator : AbstractValidator<AddQuestionToModuleRequest>
{
    public AddQuestionToModuleValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Question ID is required.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThan(0).WithMessage("Display order must be greater than 0.");
    }
}
