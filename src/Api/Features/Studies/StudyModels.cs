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

public record StudyQuestionnaireLineDto
{
    public required Guid Id { get; init; }
    public required Guid StudyId { get; init; }
    public Guid? QuestionBankItemId { get; init; }
    public required int SortOrder { get; init; }
    public required string VariableName { get; init; }
    public required int Version { get; init; }
    public string? QuestionText { get; init; }
    public string? QuestionTitle { get; init; }
    public string? QuestionType { get; init; }
    public string? Classification { get; init; }
    public string? QuestionRationale { get; init; }
    public string? ScraperNotes { get; init; }
    public string? CustomNotes { get; init; }
    public int? RowSortOrder { get; init; }
    public int? ColumnSortOrder { get; init; }
    public int? AnswerMin { get; init; }
    public int? AnswerMax { get; init; }
    public string? QuestionFormatDetails { get; init; }
    public bool IsDummy { get; init; }
    public bool LockAnswerCode { get; init; }
    public bool EditCustomAnswerCode { get; init; }
}

public record GetStudyQuestionsResponse
{
    public required List<StudyQuestionnaireLineDto> Questions { get; init; }
}
