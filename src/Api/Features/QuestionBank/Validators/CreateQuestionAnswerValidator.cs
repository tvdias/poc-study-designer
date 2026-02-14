using FluentValidation;

namespace Api.Features.QuestionBank.Validators;

public class CreateQuestionAnswerValidator : AbstractValidator<CreateQuestionAnswerRequest>
{
    public CreateQuestionAnswerValidator()
    {
        RuleFor(x => x.AnswerText)
            .NotEmpty().WithMessage("Answer Text is required")
            .MaximumLength(500).WithMessage("Answer Text must not exceed 500 characters");

        RuleFor(x => x.AnswerCode)
            .MaximumLength(50).WithMessage("Answer Code must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.AnswerCode));

        RuleFor(x => x.Version)
            .GreaterThan(0).WithMessage("Version must be greater than 0");
    }
}
