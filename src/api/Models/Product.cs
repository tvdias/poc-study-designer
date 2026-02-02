namespace Api.Models;

public class Product : VersionedEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Rules { get; set; }
    
    // Navigation properties
    public ICollection<ProductTemplate> ProductTemplates { get; set; } = new List<ProductTemplate>();
    public ICollection<ProductConfigurationQuestion> ProductConfigurationQuestions { get; set; } = new List<ProductConfigurationQuestion>();
}
