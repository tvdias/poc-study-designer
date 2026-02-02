namespace Api.Models;

public class ConfigurationQuestion : VersionedEntity
{
    public string Question { get; set; } = string.Empty;
    public string Rule { get; set; } = string.Empty; // SingleCoded, MultiCoded
    public string? AiPrompt { get; set; }
    public string? DependencyRules { get; set; }
    
    // Navigation properties
    public ICollection<ConfigurationQuestionAnswer> Answers { get; set; } = new List<ConfigurationQuestionAnswer>();
    public ICollection<ProductConfigurationQuestion> ProductConfigurationQuestions { get; set; } = new List<ProductConfigurationQuestion>();
}
