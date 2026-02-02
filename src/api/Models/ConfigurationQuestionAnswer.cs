namespace Api.Models;

public class ConfigurationQuestionAnswer : BaseEntity
{
    public int ConfigurationQuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    
    // Navigation properties
    public ConfigurationQuestion ConfigurationQuestion { get; set; } = null!;
}
