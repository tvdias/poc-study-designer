using Api.Features.ConfigurationQuestions;
using Api.Features.Shared;

namespace Api.Features.Products;

public class ProductConfigQuestion : AuditableEntity
{
    public Guid Id { get; set; }
    public required Guid ProductId { get; set; }
    public required Guid ConfigurationQuestionId { get; set; }
    public string? StatusReason { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Product? Product { get; set; }
    public ConfigurationQuestion? ConfigurationQuestion { get; set; }
    public ICollection<ProductConfigQuestionDisplayRule> DisplayRules { get; set; } = new List<ProductConfigQuestionDisplayRule>();
}
