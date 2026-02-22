using Api.Features.Projects;
using Api.Features.Shared;
using Api.Features.FieldworkMarkets;

namespace Api.Features.Studies;

public class Study : AuditableEntity
{
    public Guid Id { get; set; }
    public int Version { get; set; }
    public StudyStatus Status { get; set; } = StudyStatus.Draft;
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public required string Name { get; set; }
    public required string Category { get; set; }
    public Guid FieldworkMarketId { get; set; }
    public FieldworkMarket FieldworkMarket { get; set; } = null!;
    public required string MaconomyJobNumber { get; set; }
    public required string ProjectOperationsUrl { get; set; }
    public string? ScripterNotes { get; set; }
    
    public Guid MasterStudyId { get; set; }
    public Study MasterStudy { get; set; } = null!;
    public Guid? ParentStudyId { get; set; }
    public Study? ParentStudy { get; set; }
    
    public ICollection<Study> ChildVersions { get; set; } = new List<Study>();
    public ICollection<StudyQuestionnaireLine> QuestionnaireLines { get; set; } = new List<StudyQuestionnaireLine>();
}

public enum StudyStatus
{
    Draft,
    ReadyForScripting,
    Approved,
    Completed,
    Abandoned
}
