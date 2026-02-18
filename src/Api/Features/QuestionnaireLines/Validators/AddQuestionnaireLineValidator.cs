using FluentValidation;

namespace Api.Features.QuestionnaireLines.Validators;

public class AddQuestionnaireLineValidator : AbstractValidator<AddQuestionnaireLineRequest>
{
    public AddQuestionnaireLineValidator()
    {
        RuleFor(x => x.QuestionBankItemId)
            .NotEmpty().WithMessage("Question Bank Item ID is required.");
    }
}
