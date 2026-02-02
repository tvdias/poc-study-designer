namespace Api.Models;

public class ProductTemplate : VersionedEntity
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TemplateData { get; set; }
    
    // Navigation properties
    public Product Product { get; set; } = null!;
}
