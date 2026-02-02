namespace Api.Models;

public class Question : VersionedEntity
{
    public string VariableName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Single, Multi, Open, Grid, etc.
    public string? Methodology { get; set; }
    public bool IsStandard { get; set; } = false;
    public bool IsDummy { get; set; } = false;
    public string? ScriptNotes { get; set; }
    
    // Table ties
    public string? MetricGroup { get; set; }
    public string? DataQualityTags { get; set; }
    public string? TableNotes { get; set; }
    
    // Admin attributes
    public string? Scale { get; set; }
    public string? DisplayType { get; set; }
    public string? Restrictions { get; set; }
    public string? Facets { get; set; }
    public int? ParentQuestionId { get; set; }
    public bool IsTranslatable { get; set; } = true;
    public bool IsHidden { get; set; } = false;
    
    // Navigation properties
    public Question? ParentQuestion { get; set; }
    public ICollection<Question> ChildQuestions { get; set; } = new List<Question>();
    public ICollection<QuestionAnswer> Answers { get; set; } = new List<QuestionAnswer>();
    public ICollection<QuestionTag> QuestionTags { get; set; } = new List<QuestionTag>();
    public ICollection<ModuleQuestion> ModuleQuestions { get; set; } = new List<ModuleQuestion>();
}
