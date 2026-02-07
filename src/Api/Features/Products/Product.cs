using Api.Features.Shared;

namespace Api.Features.Products;

public class Product : AuditableEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<ProductTemplate> ProductTemplates { get; set; } = new List<ProductTemplate>();
    public ICollection<ProductConfigQuestion> ProductConfigQuestions { get; set; } = new List<ProductConfigQuestion>();
}
