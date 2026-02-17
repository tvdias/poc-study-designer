using FluentValidation;

namespace Api.Features.ProjectQuestionnaires.Validators;

public class AddProjectQuestionnaireValidator : AbstractValidator<AddProjectQuestionnaireRequest>
{
    public AddProjectQuestionnaireValidator()
    {
        RuleFor(x => x.QuestionBankItemId)
            .NotEmpty().WithMessage("Question Bank Item ID is required.");
    }
}
