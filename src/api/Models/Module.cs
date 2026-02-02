namespace Api.Models;

public class Module : VersionedEntity
{
    public string VariableName { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public int? ParentModuleId { get; set; }
    
    // Navigation properties
    public Module? ParentModule { get; set; }
    public ICollection<Module> ChildModules { get; set; } = new List<Module>();
    public ICollection<ModuleQuestion> ModuleQuestions { get; set; } = new List<ModuleQuestion>();
}
