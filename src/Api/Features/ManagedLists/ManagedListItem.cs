using Api.Features.Shared;

namespace Api.Features.ManagedLists;

public class ManagedListItem : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ManagedListId { get; set; }
    public ManagedList ManagedList { get; set; } = null!;
    public required string Code { get; set; }
    public required string Label { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Metadata { get; set; }
}
