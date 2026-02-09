using FluentValidation;

namespace Api.Features.Modules.Validators;

public class CreateModuleQuestionValidator : AbstractValidator<CreateModuleQuestionRequest>
{
    public CreateModuleQuestionValidator()
    {
        RuleFor(x => x.ModuleId)
            .NotEmpty()
            .WithMessage("Module ID is required");

        RuleFor(x => x.QuestionBankItemId)
            .NotEmpty()
            .WithMessage("Question Bank Item ID is required");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Sort order must be greater than or equal to 0");
    }
}
