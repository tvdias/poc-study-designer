using Api.Features.QuestionnaireLines;

namespace Api.Features.ManagedLists;

public class QuestionSubsetLink
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid QuestionnaireLineId { get; set; }
    public QuestionnaireLine QuestionnaireLine { get; set; } = null!;
    public Guid ManagedListId { get; set; }
    public ManagedList ManagedList { get; set; } = null!;
    public Guid? SubsetDefinitionId { get; set; }
    public SubsetDefinition? SubsetDefinition { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }
}
