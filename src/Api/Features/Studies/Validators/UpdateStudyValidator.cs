using FluentValidation;

namespace Api.Features.Studies.Validators;

public class UpdateStudyValidator : AbstractValidator<UpdateStudyRequest>
{
    public UpdateStudyValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.StatusReason)
            .MaximumLength(500).WithMessage("StatusReason cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.StatusReason));
    }
}
