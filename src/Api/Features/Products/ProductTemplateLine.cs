using Api.Features.Modules;
using Api.Features.QuestionBank;
using Api.Features.Shared;

namespace Api.Features.Products;

public class ProductTemplateLine : AuditableEntity
{
    public Guid Id { get; set; }
    public required Guid ProductTemplateId { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; } // "Module" or "Question"
    public bool IncludeByDefault { get; set; } = true;
    public int SortOrder { get; set; } = 0;
    public Guid? ModuleId { get; set; }
    public Guid? QuestionBankItemId { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ProductTemplate? ProductTemplate { get; set; }
    public Module? Module { get; set; }
    public QuestionBankItem? QuestionBankItem { get; set; }
}
