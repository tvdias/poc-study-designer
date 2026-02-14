using FluentValidation;

namespace Api.Features.MetricGroups.Validators;

public class UpdateMetricGroupValidator : AbstractValidator<UpdateMetricGroupRequest>
{
    public UpdateMetricGroupValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
    }
}
