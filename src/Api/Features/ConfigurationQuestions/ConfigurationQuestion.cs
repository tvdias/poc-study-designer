using Api.Features.Shared;

namespace Api.Features.ConfigurationQuestions;

public class ConfigurationQuestion : AuditableEntity
{
    public Guid Id { get; set; }
    public required string Question { get; set; }
    public string? AiPrompt { get; set; }
    public required RuleType RuleType { get; set; }
    public bool IsActive { get; set; } = true;
    public int Version { get; set; } = 1;
    
    // Navigation properties
    public ICollection<ConfigurationAnswer> Answers { get; set; } = new List<ConfigurationAnswer>();
}

public enum RuleType
{
    SingleCoded,
    MultiCoded
}
