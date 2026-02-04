using FluentValidation;

namespace Api.Features.Tags.Validators;

public class CreateTagValidator : AbstractValidator<CreateTagRequest>
{
    public CreateTagValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tag name is required.")
            .MaximumLength(100).WithMessage("Tag name must not exceed 100 characters.");
    }
}
