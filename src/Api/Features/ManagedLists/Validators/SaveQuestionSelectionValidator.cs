using FluentValidation;

namespace Api.Features.ManagedLists.Validators;

public class SaveQuestionSelectionValidator : AbstractValidator<SaveQuestionSelectionRequest>
{
    public SaveQuestionSelectionValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required.");

        RuleFor(x => x.QuestionnaireLineId)
            .NotEmpty()
            .WithMessage("Questionnaire line ID is required.");

        RuleFor(x => x.ManagedListId)
            .NotEmpty()
            .WithMessage("Managed list ID is required.");

        RuleFor(x => x.SelectedManagedListItemIds)
            .NotNull()
            .WithMessage("Selected items list is required.")
            .Must(list => list != null && list.Count > 0)
            .WithMessage("At least one managed list item must be selected.");
    }
}
