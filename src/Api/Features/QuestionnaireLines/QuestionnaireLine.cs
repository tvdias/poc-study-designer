using Api.Features.Projects;
using Api.Features.QuestionBank;
using Api.Features.Shared;

namespace Api.Features.QuestionnaireLines;

public class QuestionnaireLine : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public Guid? QuestionBankItemId { get; set; }
    public QuestionBankItem? QuestionBankItem { get; set; }
    public int SortOrder { get; set; }
    
    // Editable fields copied from QuestionBankItem
    public required string VariableName { get; set; }
    public int Version { get; set; }
    public string? QuestionText { get; set; }
    public string? QuestionTitle { get; set; }
    public string? QuestionType { get; set; }
    public string? Classification { get; set; }
    public string? QuestionRationale { get; set; }
    public string? ScraperNotes { get; set; }
    public string? CustomNotes { get; set; }
    public int? RowSortOrder { get; set; }
    public int? ColumnSortOrder { get; set; }
    public int? AnswerMin { get; set; }
    public int? AnswerMax { get; set; }
    public string? QuestionFormatDetails { get; set; }
    public bool IsDummy { get; set; }
}
