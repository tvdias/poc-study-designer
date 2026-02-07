using FluentValidation;

namespace Api.Features.ConfigurationQuestions.Validators;

public class CreateDependencyRuleValidator : AbstractValidator<CreateDependencyRuleRequest>
{
    public CreateDependencyRuleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Dependency rule name is required.")
            .MaximumLength(200).WithMessage("Dependency rule name must not exceed 200 characters.");
        
        RuleFor(x => x.ConfigurationQuestionId)
            .NotEmpty().WithMessage("Configuration Question ID is required.");
        
        RuleFor(x => x.Classification)
            .MaximumLength(100).WithMessage("Classification must not exceed 100 characters.");
        
        RuleFor(x => x.Type)
            .MaximumLength(100).WithMessage("Type must not exceed 100 characters.");
        
        RuleFor(x => x.ContentType)
            .MaximumLength(100).WithMessage("Content type must not exceed 100 characters.");
        
        RuleFor(x => x.Module)
            .MaximumLength(100).WithMessage("Module must not exceed 100 characters.");
        
        RuleFor(x => x.QuestionBank)
            .MaximumLength(100).WithMessage("Question bank must not exceed 100 characters.");
        
        RuleFor(x => x.Tag)
            .MaximumLength(100).WithMessage("Tag must not exceed 100 characters.");
    }
}
