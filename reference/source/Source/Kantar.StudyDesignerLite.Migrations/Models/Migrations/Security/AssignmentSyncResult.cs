namespace Kantar.StudyDesignerLite.Migrations.Models.Migrations.Security;

public class AssignmentSyncResult
{
    public int Created { get; set; } = 0;
    public int Skipped { get; set; } = 0;
    public int Failed { get; set; } = 0;
    public int Deleted { get; set; } = 0;
}
