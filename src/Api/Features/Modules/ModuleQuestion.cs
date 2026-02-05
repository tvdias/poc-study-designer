using Api.Features.Questions;

namespace Api.Features.Modules;

public class ModuleQuestion
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public Module Module { get; set; } = null!;
    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public DateTime CreatedOn { get; set; }
}
