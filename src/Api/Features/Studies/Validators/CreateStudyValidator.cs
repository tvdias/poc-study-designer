using FluentValidation;

namespace Api.Features.Studies.Validators;

public class CreateStudyValidator : AbstractValidator<CreateStudyRequest>
{
    public CreateStudyValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .MaximumLength(100).WithMessage("Category cannot exceed 100 characters");

        RuleFor(x => x.MaconomyJobNumber)
            .NotEmpty().WithMessage("MaconomyJobNumber is required")
            .MaximumLength(50).WithMessage("MaconomyJobNumber cannot exceed 50 characters");

        RuleFor(x => x.ProjectOperationsUrl)
            .NotEmpty().WithMessage("ProjectOperationsUrl is required")
            .MaximumLength(500).WithMessage("ProjectOperationsUrl cannot exceed 500 characters")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _)).WithMessage("ProjectOperationsUrl must be a valid URL");

        RuleFor(x => x.FieldworkMarketId)
            .NotEmpty().WithMessage("FieldworkMarketId is required");
    }
}
