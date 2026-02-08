using FluentValidation;

namespace Api.Features.ProductTemplates.Validators;

public class UpdateProductTemplateValidator : AbstractValidator<Api.Features.Products.UpdateProductTemplateRequest>
{
    public UpdateProductTemplateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product template name is required")
            .MaximumLength(200)
            .WithMessage("Product template name cannot exceed 200 characters");
        
        RuleFor(x => x.Version)
            .GreaterThan(0)
            .WithMessage("Version must be greater than 0");
        
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");
    }
}
