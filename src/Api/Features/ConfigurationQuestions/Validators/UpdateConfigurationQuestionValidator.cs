using FluentValidation;

namespace Api.Features.ConfigurationQuestions.Validators;

public class UpdateConfigurationQuestionValidator : AbstractValidator<UpdateConfigurationQuestionRequest>
{
    public UpdateConfigurationQuestionValidator()
    {
        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("Question is required.")
            .MaximumLength(500).WithMessage("Question must not exceed 500 characters.");
    }
}
