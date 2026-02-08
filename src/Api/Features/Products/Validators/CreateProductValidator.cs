using FluentValidation;

namespace Api.Features.Products.Validators;

public class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(200)
            .WithMessage("Product name cannot exceed 200 characters");
        
        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Product description cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
