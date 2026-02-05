namespace Api.Features.Modules;

public class ModuleVersion
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public Module Module { get; set; } = null!;
    public int VersionNumber { get; set; }
    public string? ChangeDescription { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
}
