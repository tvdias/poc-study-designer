using FluentValidation;

namespace Api.Features.ConfigurationQuestions.Validators;

public class UpdateConfigurationAnswerValidator : AbstractValidator<UpdateConfigurationAnswerRequest>
{
    public UpdateConfigurationAnswerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Answer name is required.")
            .MaximumLength(200).WithMessage("Answer name must not exceed 200 characters.");
    }
}
