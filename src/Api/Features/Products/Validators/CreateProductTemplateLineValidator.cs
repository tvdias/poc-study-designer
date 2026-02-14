using FluentValidation;

namespace Api.Features.Products.Validators;

public class CreateProductTemplateLineValidator : AbstractValidator<CreateProductTemplateLineRequest>
{
    public CreateProductTemplateLineValidator()
    {
        RuleFor(x => x.ProductTemplateId)
            .NotEmpty()
            .WithMessage("Product template ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(200)
            .WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Type is required")
            .Must(type => type == "Module" || type == "Question")
            .WithMessage("Type must be either 'Module' or 'Question'");

        RuleFor(x => x.ModuleId)
            .NotEmpty()
            .When(x => x.Type == "Module")
            .WithMessage("Module ID is required when Type is 'Module'");

        RuleFor(x => x.QuestionBankItemId)
            .NotEmpty()
            .When(x => x.Type == "Question")
            .WithMessage("Question Bank Item ID is required when Type is 'Question'");

        RuleFor(x => x)
            .Must(x => (x.Type == "Module" && x.ModuleId.HasValue && !x.QuestionBankItemId.HasValue) ||
                       (x.Type == "Question" && x.QuestionBankItemId.HasValue && !x.ModuleId.HasValue))
            .WithMessage("Either Module ID or Question Bank Item ID must be provided, not both");
    }
}
