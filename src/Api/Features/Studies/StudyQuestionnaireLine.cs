using Api.Features.QuestionBank;
using Api.Features.Shared;

namespace Api.Features.Studies;

public class StudyQuestionnaireLine : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid StudyId { get; set; }
    public Study Study { get; set; } = null!;
    public Guid? QuestionBankItemId { get; set; }
    public QuestionBankItem? QuestionBankItem { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Copied fields from QuestionBankItem (editable in study context)
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
    public bool LockAnswerCode { get; set; } = false;
    public bool EditCustomAnswerCode { get; set; } = false;
    
    // Navigation properties
    public ICollection<StudyManagedListAssignment> ManagedListAssignments { get; set; } = new List<StudyManagedListAssignment>();
    public ICollection<StudyQuestionSubsetLink> SubsetLinks { get; set; } = new List<StudyQuestionSubsetLink>();
}
