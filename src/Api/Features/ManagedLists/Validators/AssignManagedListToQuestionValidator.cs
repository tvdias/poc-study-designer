using FluentValidation;

namespace Api.Features.ManagedLists.Validators;

public class AssignManagedListToQuestionValidator : AbstractValidator<AssignManagedListToQuestionRequest>
{
    public AssignManagedListToQuestionValidator()
    {
        RuleFor(x => x.QuestionnaireLineId)
            .NotEmpty().WithMessage("Questionnaire line ID is required.");
        
        RuleFor(x => x.ManagedListId)
            .NotEmpty().WithMessage("Managed list ID is required.");
    }
}
