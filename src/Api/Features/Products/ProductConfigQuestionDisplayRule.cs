using Api.Features.ConfigurationQuestions;
using Api.Features.Shared;

namespace Api.Features.Products;

public class ProductConfigQuestionDisplayRule : AuditableEntity
{
    public Guid Id { get; set; }
    public required Guid ProductConfigQuestionId { get; set; }
    public required Guid TriggeringConfigurationQuestionId { get; set; }
    public Guid? TriggeringAnswerId { get; set; }
    public required string DisplayCondition { get; set; } // "Show" or "Hide"
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ProductConfigQuestion? ProductConfigQuestion { get; set; }
    public ConfigurationQuestion? TriggeringConfigurationQuestion { get; set; }
    public ConfigurationAnswer? TriggeringAnswer { get; set; }
}
