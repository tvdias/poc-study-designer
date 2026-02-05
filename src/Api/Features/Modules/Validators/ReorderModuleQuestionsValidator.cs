using FluentValidation;

namespace Api.Features.Modules.Validators;

public class ReorderModuleQuestionsValidator : AbstractValidator<ReorderModuleQuestionsRequest>
{
    public ReorderModuleQuestionsValidator()
    {
        RuleFor(x => x.Questions)
            .NotEmpty().WithMessage("Questions list cannot be empty.");

        RuleForEach(x => x.Questions)
            .ChildRules(question =>
            {
                question.RuleFor(q => q.QuestionId)
                    .NotEmpty().WithMessage("Question ID is required.");

                question.RuleFor(q => q.DisplayOrder)
                    .GreaterThan(0).WithMessage("Display order must be greater than 0.");
            });
    }
}
