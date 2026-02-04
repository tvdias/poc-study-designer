using FluentValidation;

namespace Api.Features.Tags.Validators;

public class UpdateTagValidator : AbstractValidator<UpdateTagRequest>
{
    public UpdateTagValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tag name is required.")
            .MaximumLength(100).WithMessage("Tag name must not exceed 100 characters.");
    }
}
