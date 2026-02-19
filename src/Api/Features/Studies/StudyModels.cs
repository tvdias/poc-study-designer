namespace Api.Features.Studies;

// Request DTOs
public record CreateStudyRequest
{
    public required Guid ProjectId { get; init; }
    public required string Name { get; init; }
    public required string Category { get; init; }
    public required Guid FieldworkMarketId { get; init; }
    public required string MaconomyJobNumber { get; init; }
    public required string ProjectOperationsUrl { get; init; }
    public string? ScripterNotes { get; init; }
}

public record UpdateStudyRequest
{
    public required string Name { get; init; }
    public StudyStatus? Status { get; init; }
    public required string Category { get; init; }
    public required Guid FieldworkMarketId { get; init; }
    public required string MaconomyJobNumber { get; init; }
    public required string ProjectOperationsUrl { get; init; }
    public string? ScripterNotes { get; init; }
}

// Response DTOs
public record CreateStudyResponse
{
    public required Guid StudyId { get; init; }
    public required string Name { get; init; }
    public required int Version { get; init; }
    public required StudyStatus Status { get; init; }
    public required int QuestionCount { get; init; }
}

public record CreateStudyVersionResponse
{
    public required Guid StudyId { get; init; }
    public required Guid ParentStudyId { get; init; }
    public required string Name { get; init; }
    public required int Version { get; init; }
    public required StudyStatus Status { get; init; }
    public required int QuestionCount { get; init; }
}

public record GetStudiesResponse
{
    public required List<StudySummary> Studies { get; init; }
}

public record StudySummary
{
    public required Guid StudyId { get; init; }
    public required string Name { get; init; }
    public required int Version { get; init; }
    public required StudyStatus Status { get; init; }
    public required string Category { get; init; }
    public required string FieldworkMarketName { get; init; }
    public required DateTime CreatedOn { get; init; }
    public string? CreatedBy { get; init; }
    public required int QuestionCount { get; init; }
}

public record GetStudyDetailsResponse
{
    public required Guid StudyId { get; init; }
    public required Guid ProjectId { get; init; }
    public required string ProjectName { get; init; }
    public required string Name { get; init; }
    public required int Version { get; init; }
    public required StudyStatus Status { get; init; }
    public Guid? MasterStudyId { get; init; }
    public Guid? ParentStudyId { get; init; }
    public required DateTime CreatedOn { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? ModifiedOn { get; init; }
    public string? ModifiedBy { get; init; }
    public required int QuestionCount { get; init; }
    public string? Category { get; init; }
    public string? MaconomyJobNumber { get; init; }
    public string? ProjectOperationsUrl { get; init; }
    public string? ScripterNotes { get; init; }
    public Guid? FieldworkMarketId { get; init; }
    public string? FieldworkMarketName { get; init; }
}
