using Api.Features.Projects;
using Api.Features.QuestionBank;
using Api.Features.Shared;

namespace Api.Features.ProjectQuestionnaires;

public class ProjectQuestionnaire : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public Guid QuestionBankItemId { get; set; }
    public QuestionBankItem QuestionBankItem { get; set; } = null!;
    public int SortOrder { get; set; }
}
