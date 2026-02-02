namespace Api.Models;

public abstract class VersionedEntity : BaseEntity
{
    public int Version { get; set; } = 1;
    public string Status { get; set; } = "Draft"; // Draft, Active, Inactive, Archived
}
