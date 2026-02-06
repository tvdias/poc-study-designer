using Api.Features.Shared;

namespace Api.Features.ConfigurationQuestions;

public class DependencyRule : AuditableEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Guid ConfigurationQuestionId { get; set; }
    public Guid? TriggeringAnswerId { get; set; }
    public string? Classification { get; set; }
    public string? Type { get; set; }
    public string? ContentType { get; set; }
    public string? Module { get; set; }
    public string? QuestionBank { get; set; }
    public string? Tag { get; set; }
    public string? StatusReason { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ConfigurationQuestion? ConfigurationQuestion { get; set; }
    public ConfigurationAnswer? TriggeringAnswer { get; set; }
}
