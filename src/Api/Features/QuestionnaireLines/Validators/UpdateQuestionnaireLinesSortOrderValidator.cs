using FluentValidation;

namespace Api.Features.QuestionnaireLines.Validators;

public class UpdateQuestionnaireLinesSortOrderValidator : AbstractValidator<UpdateQuestionnaireLinesSortOrderRequest>
{
    public UpdateQuestionnaireLinesSortOrderValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Id)
                .NotEmpty().WithMessage("ID is required.");
            
            item.RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order must be greater than or equal to 0.");
        });
    }
}
