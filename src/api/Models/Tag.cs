namespace Api.Models;

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Navigation properties
    public ICollection<QuestionTag> QuestionTags { get; set; } = new List<QuestionTag>();
}
