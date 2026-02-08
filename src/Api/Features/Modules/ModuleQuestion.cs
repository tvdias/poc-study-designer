using Api.Features.Shared;

namespace Api.Features.Modules;

public class ModuleQuestion : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public Guid QuestionBankItemId { get; set; }
    public int DisplayOrder { get; set; } = 0;
    
    // Navigation properties
    public Module Module { get; set; } = null!;
    public QuestionBank.QuestionBankItem QuestionBankItem { get; set; } = null!;
}
