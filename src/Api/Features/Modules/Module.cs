using Api.Features.Shared;

namespace Api.Features.Modules;

public class Module : AuditableEntity
{
    public Guid Id { get; set; }
    public required string VariableName { get; set; }
    public required string Label { get; set; }
    public string? Description { get; set; }
    public int VersionNumber { get; set; } = 1;
    public Guid? ParentModuleId { get; set; }
    public Module? ParentModule { get; set; }
    public string? Instructions { get; set; }
    public required string Status { get; set; } = "Active";
    public string? StatusReason { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ICollection<Module> ChildModules { get; set; } = new List<Module>();
    public ICollection<ModuleVersion> Versions { get; set; } = new List<ModuleVersion>();
}
