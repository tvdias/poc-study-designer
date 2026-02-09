using FluentValidation;

namespace Api.Features.Products.Validators;

public class UpdateProductConfigQuestionValidator : AbstractValidator<UpdateProductConfigQuestionRequest>
{
    public UpdateProductConfigQuestionValidator()
    {
        // No specific validation needed for IsActive boolean
    }
}
