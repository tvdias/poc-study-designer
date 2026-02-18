using FluentValidation;

namespace Api.Features.QuestionnaireLines.Validators;

public class AddQuestionnaireLineValidator : AbstractValidator<AddQuestionnaireLineRequest>
{
    public AddQuestionnaireLineValidator()
    {
        // QuestionBankItemId is optional - user can import from bank or add manually
        
        // When adding manually (no QuestionBankItemId), VariableName is required
        RuleFor(x => x.VariableName)
            .NotEmpty()
            .When(x => !x.QuestionBankItemId.HasValue)
            .WithMessage("Variable Name is required when adding a manual question.");
        
        // Version should default to 1 if not provided for manual questions
        RuleFor(x => x.Version)
            .GreaterThan(0)
            .When(x => !x.QuestionBankItemId.HasValue && x.Version.HasValue)
            .WithMessage("Version must be greater than 0.");
    }
}
