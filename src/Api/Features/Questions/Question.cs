using Api.Features.Shared;

namespace Api.Features.Questions;

public class Question : AuditableEntity
{
    public Guid Id { get; set; }
    public required string VariableName { get; set; }
    public required string QuestionType { get; set; }
    public required string QuestionText { get; set; }
    public required string QuestionSource { get; set; } // "Standard" or "Custom"
    public bool IsActive { get; set; } = true;
}
