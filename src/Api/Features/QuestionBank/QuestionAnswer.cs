using Api.Features.Shared;

namespace Api.Features.QuestionBank;

public class QuestionAnswer : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid QuestionBankItemId { get; set; }
    public required string AnswerText { get; set; }
    public string? AnswerCode { get; set; }
    public string? AnswerLocation { get; set; }
    public bool IsOpen { get; set; } = false;
    public bool IsFixed { get; set; } = false;
    public bool IsExclusive { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public string? CustomProperty { get; set; }
    public string? Facets { get; set; }
    public int Version { get; set; } = 1;
    public int? DisplayOrder { get; set; }
    
    // Navigation properties
    public QuestionBankItem QuestionBankItem { get; set; } = null!;
}
