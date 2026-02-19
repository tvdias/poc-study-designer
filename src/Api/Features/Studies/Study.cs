using Api.Features.Projects;
using Api.Features.Shared;

namespace Api.Features.Studies;

public class Study : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int VersionNumber { get; set; }
    public StudyStatus Status { get; set; } = StudyStatus.Draft;
    public string? StatusReason { get; set; }
    
    // Lineage tracking
    public Guid? MasterStudyId { get; set; }
    public Study? MasterStudy { get; set; }
    public Guid? ParentStudyId { get; set; }
    public Study? ParentStudy { get; set; }
    
    // Versioning metadata
    public string? VersionComment { get; set; }
    public string? VersionReason { get; set; }
    
    // Navigation properties
    public ICollection<Study> ChildVersions { get; set; } = new List<Study>();
    public ICollection<StudyQuestionnaireLine> QuestionnaireLines { get; set; } = new List<StudyQuestionnaireLine>();
}

public enum StudyStatus
{
    Draft,
    ReadyForScripting,
    Approved,
    Archived
}
