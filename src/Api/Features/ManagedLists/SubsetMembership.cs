namespace Api.Features.ManagedLists;

public class SubsetMembership
{
    public Guid Id { get; set; }
    public Guid SubsetDefinitionId { get; set; }
    public SubsetDefinition SubsetDefinition { get; set; } = null!;
    public Guid ManagedListItemId { get; set; }
    public ManagedListItem ManagedListItem { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
}
