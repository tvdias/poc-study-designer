namespace Api.Models;

public class QuestionAnswer : VersionedEntity
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Location { get; set; } = "Row"; // Row or Column
    public bool IsFixed { get; set; } = false;
    public bool IsExclusive { get; set; } = false;
    public bool IsOpen { get; set; } = false;
    public bool IsTranslatable { get; set; } = true;
    public int DisplayOrder { get; set; }
    public string? Facets { get; set; }
    public string? Restrictions { get; set; }
    public string? RuleMetadata { get; set; }
    
    // Navigation properties
    public Question Question { get; set; } = null!;
}
