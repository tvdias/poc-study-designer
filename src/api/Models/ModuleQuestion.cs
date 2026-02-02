namespace Api.Models;

public class ModuleQuestion
{
    public int ModuleId { get; set; }
    public int QuestionId { get; set; }
    public int DisplayOrder { get; set; }
    
    // Navigation properties
    public Module Module { get; set; } = null!;
    public Question Question { get; set; } = null!;
}
