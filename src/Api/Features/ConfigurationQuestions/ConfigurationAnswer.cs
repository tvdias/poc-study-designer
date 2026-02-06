using Api.Features.Shared;

namespace Api.Features.ConfigurationQuestions;

public class ConfigurationAnswer : AuditableEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Guid ConfigurationQuestionId { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ConfigurationQuestion? ConfigurationQuestion { get; set; }
}
