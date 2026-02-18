using Api.Features.QuestionnaireLines;

namespace Api.Features.ManagedLists;

public class QuestionManagedList
{
    public Guid Id { get; set; }
    public Guid QuestionnaireLineId { get; set; }
    public QuestionnaireLine QuestionnaireLine { get; set; } = null!;
    public Guid ManagedListId { get; set; }
    public ManagedList ManagedList { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
