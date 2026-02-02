namespace Api.Models;

public class ProductConfigurationQuestion
{
    public int ProductId { get; set; }
    public int ConfigurationQuestionId { get; set; }
    public int DisplayOrder { get; set; }
    
    // Navigation properties
    public Product Product { get; set; } = null!;
    public ConfigurationQuestion ConfigurationQuestion { get; set; } = null!;
}
