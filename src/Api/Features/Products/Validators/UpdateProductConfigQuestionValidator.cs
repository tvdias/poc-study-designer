using FluentValidation;

namespace Api.Features.Products.Validators;

public class UpdateProductConfigQuestionValidator : AbstractValidator<UpdateProductConfigQuestionRequest>
{
    public UpdateProductConfigQuestionValidator()
    {
        RuleFor(x => x.StatusReason)
            .MaximumLength(200)
            .WithMessage("Status reason cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.StatusReason));
    }
}
