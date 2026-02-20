using FluentValidation;
using System.Text.RegularExpressions;

namespace Api.Features.ManagedLists.Validators;

public class UpdateManagedListItemValidator : AbstractValidator<UpdateManagedListItemRequest>
{
    public UpdateManagedListItemValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Item code is required.")
            .MaximumLength(100).WithMessage("Item code must not exceed 100 characters.")
            .Must(BeValidCodeFormat).WithMessage("Item code must start with a letter and contain only alphanumeric characters and underscores.");
        
        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Item label (name) is required.")
            .MaximumLength(200).WithMessage("Item label (name) must not exceed 200 characters.");
        
        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order must be greater than or equal to 0.");
    }
    
    private static bool BeValidCodeFormat(string? code)
    {
        if (string.IsNullOrEmpty(code))
            return false;
            
        // Code must start with a letter and contain only alphanumeric characters and underscores
        return Regex.IsMatch(code, @"^[a-zA-Z][a-zA-Z0-9_]*$");
    }
}
