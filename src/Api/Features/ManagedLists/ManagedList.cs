using Api.Features.Projects;
using Api.Features.Shared;

namespace Api.Features.ManagedLists;

public class ManagedList : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ManagedListStatus Status { get; set; } = ManagedListStatus.Active;
    
    // Navigation properties
    public ICollection<ManagedListItem> Items { get; set; } = new List<ManagedListItem>();
    public ICollection<QuestionManagedList> QuestionAssignments { get; set; } = new List<QuestionManagedList>();
}

public enum ManagedListStatus
{
    Active,
    Inactive
}
