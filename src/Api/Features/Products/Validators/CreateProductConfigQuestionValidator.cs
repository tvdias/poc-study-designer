using FluentValidation;

namespace Api.Features.Products.Validators;

public class CreateProductConfigQuestionValidator : AbstractValidator<CreateProductConfigQuestionRequest>
{
    public CreateProductConfigQuestionValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");
        
        RuleFor(x => x.ConfigurationQuestionId)
            .NotEmpty()
            .WithMessage("Configuration Question ID is required");
        
        RuleFor(x => x.StatusReason)
            .MaximumLength(200)
            .WithMessage("Status reason cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.StatusReason));
    }
}
