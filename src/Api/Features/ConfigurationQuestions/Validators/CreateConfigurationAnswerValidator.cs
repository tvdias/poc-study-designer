using FluentValidation;

namespace Api.Features.ConfigurationQuestions.Validators;

public class CreateConfigurationAnswerValidator : AbstractValidator<CreateConfigurationAnswerRequest>
{
    public CreateConfigurationAnswerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Answer name is required.")
            .MaximumLength(200).WithMessage("Answer name must not exceed 200 characters.");
        
        RuleFor(x => x.ConfigurationQuestionId)
            .NotEmpty().WithMessage("Configuration Question ID is required.");
    }
}
