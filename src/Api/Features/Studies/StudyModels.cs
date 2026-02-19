namespace Api.Features.Studies;

// Request DTOs
public record CreateStudyRequest
{
    public required Guid ProjectId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? Comment { get; init; }
}

public record CreateStudyVersionRequest
{
    public required Guid ParentStudyId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Comment { get; init; }
    public string? Reason { get; init; }
}

public record UpdateStudyRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public StudyStatus? Status { get; init; }
    public string? StatusReason { get; init; }
}

// Response DTOs
public record CreateStudyResponse
{
    public required Guid StudyId { get; init; }
    public required string Name { get; init; }
    public required int VersionNumber { get; init; }
    public required StudyStatus Status { get; init; }
    public required int QuestionCount { get; init; }
}

public record CreateStudyVersionResponse
{
    public required Guid StudyId { get; init; }
    public required string Name { get; init; }
    public required int VersionNumber { get; init; }
    public required StudyStatus Status { get; init; }
    public required Guid ParentStudyId { get; init; }
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
    public required int VersionNumber { get; init; }
    public required StudyStatus Status { get; init; }
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
    public string? Description { get; init; }
    public required int VersionNumber { get; init; }
    public required StudyStatus Status { get; init; }
    public Guid? MasterStudyId { get; init; }
    public Guid? ParentStudyId { get; init; }
    public string? VersionComment { get; init; }
    public required DateTime CreatedOn { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? ModifiedOn { get; init; }
    public string? ModifiedBy { get; init; }
    public required int QuestionCount { get; init; }
}
