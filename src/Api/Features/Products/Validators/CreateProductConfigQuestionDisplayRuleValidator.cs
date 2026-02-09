using FluentValidation;

namespace Api.Features.Products.Validators;

public class CreateProductConfigQuestionDisplayRuleValidator : AbstractValidator<CreateProductConfigQuestionDisplayRuleRequest>
{
    public CreateProductConfigQuestionDisplayRuleValidator()
    {
        RuleFor(x => x.ProductConfigQuestionId)
            .NotEmpty()
            .WithMessage("Product config question ID is required");

        RuleFor(x => x.TriggeringConfigurationQuestionId)
            .NotEmpty()
            .WithMessage("Triggering configuration question ID is required");

        RuleFor(x => x.DisplayCondition)
            .NotEmpty()
            .WithMessage("Display condition is required")
            .Must(condition => condition == "Show" || condition == "Hide")
            .WithMessage("Display condition must be either 'Show' or 'Hide'");
    }
}
