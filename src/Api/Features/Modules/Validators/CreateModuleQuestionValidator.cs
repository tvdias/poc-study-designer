using FluentValidation;

namespace Api.Features.Modules.Validators;

public class CreateModuleQuestionValidator : AbstractValidator<CreateModuleQuestionRequest>
{
    public CreateModuleQuestionValidator()
    {
        RuleFor(x => x.QuestionBankItemId)
            .NotEmpty()
            .WithMessage("Question Bank Item is required.");
        
        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Display order must be 0 or greater.");
    }
}
