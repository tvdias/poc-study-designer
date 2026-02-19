using Api.Features.ManagedLists;

namespace Api.Features.Studies;

public class StudyQuestionSubsetLink
{
    public Guid Id { get; set; }
    public Guid StudyId { get; set; }
    public Study Study { get; set; } = null!;
    public Guid StudyQuestionnaireLineId { get; set; }
    public StudyQuestionnaireLine StudyQuestionnaireLine { get; set; } = null!;
    public Guid ManagedListId { get; set; }
    public ManagedList ManagedList { get; set; } = null!;
    public Guid? SubsetDefinitionId { get; set; }
    public SubsetDefinition? SubsetDefinition { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }
}
