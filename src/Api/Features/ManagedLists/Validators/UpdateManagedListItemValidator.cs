using FluentValidation;

namespace Api.Features.ManagedLists.Validators;

public class UpdateManagedListItemValidator : AbstractValidator<UpdateManagedListItemRequest>
{
    public UpdateManagedListItemValidator()
    {
        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Item value is required.")
            .MaximumLength(100).WithMessage("Item value must not exceed 100 characters.");
        
        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Item label is required.")
            .MaximumLength(200).WithMessage("Item label must not exceed 200 characters.");
        
        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order must be greater than or equal to 0.");
    }
}
