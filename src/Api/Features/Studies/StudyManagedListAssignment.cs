using Api.Features.ManagedLists;

namespace Api.Features.Studies;

public class StudyManagedListAssignment
{
    public Guid Id { get; set; }
    public Guid StudyId { get; set; }
    public Study Study { get; set; } = null!;
    public Guid StudyQuestionnaireLineId { get; set; }
    public StudyQuestionnaireLine StudyQuestionnaireLine { get; set; } = null!;
    public Guid ManagedListId { get; set; }
    public ManagedList ManagedList { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
