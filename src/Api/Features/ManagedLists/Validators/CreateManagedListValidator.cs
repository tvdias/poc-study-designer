using FluentValidation;

namespace Api.Features.ManagedLists.Validators;

public class CreateManagedListValidator : AbstractValidator<CreateManagedListRequest>
{
    public CreateManagedListValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Managed list name is required.")
            .MaximumLength(200).WithMessage("Managed list name must not exceed 200 characters.");
        
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
