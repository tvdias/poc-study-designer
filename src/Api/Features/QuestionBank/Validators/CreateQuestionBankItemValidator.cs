using FluentValidation;

namespace Api.Features.QuestionBank.Validators;

public class CreateQuestionBankItemValidator : AbstractValidator<CreateQuestionBankItemRequest>
{
    public CreateQuestionBankItemValidator()
    {
        RuleFor(x => x.VariableName)
            .NotEmpty().WithMessage("Variable Name is required")
            .MaximumLength(200).WithMessage("Variable Name must not exceed 200 characters");

        RuleFor(x => x.Version)
            .GreaterThan(0).WithMessage("Version must be greater than 0");

        RuleFor(x => x.QuestionType)
            .MaximumLength(100).WithMessage("Question Type must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.QuestionType));

        RuleFor(x => x.Classification)
            .MaximumLength(50).WithMessage("Classification must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Classification));

        RuleFor(x => x.Status)
            .MaximumLength(50).WithMessage("Status must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.Methodology)
            .MaximumLength(100).WithMessage("Methodology must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Methodology));
    }
}
