using FluentValidation;

namespace Api.Features.Questions.Validators;

public class CreateQuestionValidator : AbstractValidator<CreateQuestionRequest>
{
    public CreateQuestionValidator()
    {
        RuleFor(x => x.VariableName)
            .NotEmpty().WithMessage("Variable name is required.")
            .MaximumLength(100).WithMessage("Variable name must not exceed 100 characters.");

        RuleFor(x => x.QuestionType)
            .NotEmpty().WithMessage("Question type is required.")
            .MaximumLength(50).WithMessage("Question type must not exceed 50 characters.");

        RuleFor(x => x.QuestionText)
            .NotEmpty().WithMessage("Question text is required.")
            .MaximumLength(1000).WithMessage("Question text must not exceed 1000 characters.");

        RuleFor(x => x.QuestionSource)
            .NotEmpty().WithMessage("Question source is required.")
            .Must(x => x == "Standard" || x == "Custom")
            .WithMessage("Question source must be either 'Standard' or 'Custom'.");
    }
}
