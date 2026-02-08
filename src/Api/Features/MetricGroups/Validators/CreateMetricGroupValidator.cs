using FluentValidation;

namespace Api.Features.MetricGroups.Validators;

public class CreateMetricGroupValidator : AbstractValidator<CreateMetricGroupRequest>
{
    public CreateMetricGroupValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters.");
    }
}
