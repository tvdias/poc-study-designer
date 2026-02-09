using Api.Features.Shared;
using Api.Features.Modules;

namespace Api.Features.QuestionBank;

public class QuestionBankItem : AuditableEntity
{
    public Guid Id { get; set; }
    public required string VariableName { get; set; }
    public int Version { get; set; } = 1;
    public string? QuestionType { get; set; }
    public string? QuestionText { get; set; }
    public string? Classification { get; set; } // Standard or Custom
    public bool IsDummy { get; set; } = false;
    public string? QuestionTitle { get; set; }
    public string? Status { get; set; } // Active/Inactive
    public string? Methodology { get; set; }
    public string? DataQualityTag { get; set; }
    public int? RowSortOrder { get; set; }
    public int? ColumnSortOrder { get; set; }
    public int? AnswerMin { get; set; }
    public int? AnswerMax { get; set; }
    public string? QuestionFormatDetails { get; set; }
    public string? ScraperNotes { get; set; }
    public string? CustomNotes { get; set; }
    public Guid? MetricGroupId { get; set; }
    public MetricGroups.MetricGroup? MetricGroup { get; set; }
    public string? TableTitle { get; set; }
    public string? QuestionRationale { get; set; }
    public string? SingleOrMulticode { get; set; }
    public string? ManagedListReferences { get; set; }
    
    // Admin fields
    public bool IsTranslatable { get; set; } = false;
    public bool IsHidden { get; set; } = false;
    public bool IsQuestionActive { get; set; } = true;
    public bool IsQuestionOutOfUse { get; set; } = false;
    public int? AnswerRestrictionMin { get; set; }
    public int? AnswerRestrictionMax { get; set; }
    public string? RestrictionDataType { get; set; }
    public string? RestrictedToClient { get; set; }
    
    // Answer type details
    public string? AnswerTypeCode { get; set; }
    public bool IsAnswerRequired { get; set; } = false;
    public string? ScalePoint { get; set; }
    public string? ScaleType { get; set; }
    public string? DisplayType { get; set; }
    public string? InstructionText { get; set; }
    public Guid? ParentQuestionId { get; set; }
    public string? QuestionFacet { get; set; }
    
    // Navigation properties
    public QuestionBankItem? ParentQuestion { get; set; }
    public ICollection<ModuleQuestion> ModuleQuestions { get; set; } = new List<ModuleQuestion>();
    public ICollection<QuestionAnswer> Answers { get; set; } = new List<QuestionAnswer>();
}
