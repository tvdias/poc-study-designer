using Api.Features.QuestionBank;
using Api.Features.Shared;

namespace Api.Features.Modules;

public class ModuleQuestion : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public Guid QuestionBankItemId { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Module Module { get; set; } = null!;
    public QuestionBankItem QuestionBankItem { get; set; } = null!;
}
