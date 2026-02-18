using FluentValidation;

namespace Api.Features.QuestionnaireLines.Validators;

public class UpdateQuestionnaireLineValidator : AbstractValidator<UpdateQuestionnaireLineRequest>
{
    public UpdateQuestionnaireLineValidator()
    {
        RuleFor(x => x.QuestionText)
            .MaximumLength(2000).WithMessage("Question text must not exceed 2000 characters.");

        RuleFor(x => x.QuestionTitle)
            .MaximumLength(500).WithMessage("Question title must not exceed 500 characters.");

        RuleFor(x => x.QuestionRationale)
            .MaximumLength(2000).WithMessage("Question rationale must not exceed 2000 characters.");

        RuleFor(x => x.ScraperNotes)
            .MaximumLength(2000).WithMessage("Scraper notes must not exceed 2000 characters.");

        RuleFor(x => x.CustomNotes)
            .MaximumLength(2000).WithMessage("Custom notes must not exceed 2000 characters.");

        RuleFor(x => x.QuestionFormatDetails)
            .MaximumLength(1000).WithMessage("Question format details must not exceed 1000 characters.");

        RuleFor(x => x.AnswerMin)
            .GreaterThanOrEqualTo(0).When(x => x.AnswerMin.HasValue)
            .WithMessage("Answer min must be greater than or equal to 0.");

        RuleFor(x => x.AnswerMax)
            .GreaterThanOrEqualTo(0).When(x => x.AnswerMax.HasValue)
            .WithMessage("Answer max must be greater than or equal to 0.");

        RuleFor(x => x.AnswerMax)
            .GreaterThanOrEqualTo(x => x.AnswerMin).When(x => x.AnswerMin.HasValue && x.AnswerMax.HasValue)
            .WithMessage("Answer max must be greater than or equal to answer min.");
    }
}
