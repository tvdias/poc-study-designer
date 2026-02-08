using Api.Features.Shared;

namespace Api.Features.Products;

public class ProductTemplate : AuditableEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required int Version { get; set; }
    public required Guid ProductId { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Product? Product { get; set; }
}
