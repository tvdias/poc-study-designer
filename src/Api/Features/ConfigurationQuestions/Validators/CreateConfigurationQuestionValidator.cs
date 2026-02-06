using FluentValidation;

namespace Api.Features.ConfigurationQuestions.Validators;

public class CreateConfigurationQuestionValidator : AbstractValidator<CreateConfigurationQuestionRequest>
{
    public CreateConfigurationQuestionValidator()
    {
        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("Question is required.")
            .MaximumLength(500).WithMessage("Question must not exceed 500 characters.");
    }
}
