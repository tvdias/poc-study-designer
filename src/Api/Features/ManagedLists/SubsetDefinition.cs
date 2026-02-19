using Api.Features.Projects;
using Api.Features.Shared;

namespace Api.Features.ManagedLists;

public class SubsetDefinition : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public Guid ManagedListId { get; set; }
    public ManagedList ManagedList { get; set; } = null!;
    public required string Name { get; set; }
    public required string SignatureHash { get; set; }
    public SubsetStatus Status { get; set; } = SubsetStatus.Active;
    
    // Navigation properties
    public ICollection<SubsetMembership> Memberships { get; set; } = new List<SubsetMembership>();
    public ICollection<QuestionSubsetLink> QuestionLinks { get; set; } = new List<QuestionSubsetLink>();
}

public enum SubsetStatus
{
    Active,
    Inactive
}
