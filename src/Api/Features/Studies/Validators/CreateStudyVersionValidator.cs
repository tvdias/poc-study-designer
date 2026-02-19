using FluentValidation;

namespace Api.Features.Studies.Validators;

public class CreateStudyVersionValidator : AbstractValidator<CreateStudyVersionRequest>
{
    public CreateStudyVersionValidator()
    {
        RuleFor(x => x.ParentStudyId)
            .NotEmpty().WithMessage("ParentStudyId is required");

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Comment));

        RuleFor(x => x.Reason)
            .MaximumLength(1000).WithMessage("Reason cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}
