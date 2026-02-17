namespace Kantar.StudyDesignerLite.Migrations.Models;

using System;

public class MigrationResult
{
    public bool Success { get; private set; }
    public string Message { get; private set; }
    public Exception? Exception { get; private set; }
    public TimeSpan ExecutionTime { get; set; }
    public int RecordsProcessed { get; set; }
    public int RecordsCreated { get; set; }
    public int RecordsUpdated { get; set; }
    public int RecordsSkipped { get; set; }
    public int RecordsDeleted { get; set; }

    private MigrationResult(bool success, string message, Exception? exception = null)
    {
        Success = success;
        Message = message;
        Exception = exception;
    }

    public static MigrationResult Successful(string message = "Migration completed successfully")
        => new(true, message);

    public static MigrationResult Failed(string message, Exception? exception = null)
        => new(false, message, exception);

    public static MigrationResult Skipped(string reason)
        => new(true, $"Skipped: {reason}");
}
