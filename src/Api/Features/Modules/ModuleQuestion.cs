using Api.Features.QuestionBank;
using Api.Features.Shared;

namespace Api.Features.Modules;

public class ModuleQuestion : AuditableEntity
{
    public Guid Id { get; set; }
    public required Guid ModuleId { get; set; }
    public required Guid QuestionBankItemId { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Module? Module { get; set; }
    public QuestionBankItem? QuestionBankItem { get; set; }
}
