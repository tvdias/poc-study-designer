using Kantar.StudyDesignerLite.Migrations.Enums;
using Kantar.StudyDesignerLite.Migrations.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Migrations.Migrations.FixData;

public class RemoveSequentialFromRowAndColumnSortOrderQuestionBank : FixDataMigration
{
    public override int ExecutionOrder => 12;
    public override string Description => "RemoveSequentialFromRowAndColumnSortOrderQuestionBank";
    private readonly int _sequentialValue = 847610005; // Sequential

    protected override async Task<List<Entity>> GetRecordsToUpdate()
    {
        var filter = new FilterExpression(LogicalOperator.Or)
        {
            Conditions =
            {
                new ConditionExpression("ktr_rowsortorder", ConditionOperator.Equal, _sequentialValue),
                new ConditionExpression("ktr_columnsortorder", ConditionOperator.Equal, _sequentialValue)
            }
        };

        string[] columns = ["ktr_rowsortorder", "ktr_columnsortorder"];

        List<Entity> result = await ReadFromTargetAsync(
            "kt_questionbank",
            columns,
            filter
        ) ?? new List<Entity>();

        return result;
    }

    protected override async Task<MigrationResult> UpdateRecords(List<Entity> recordsToUpdate)
    {
        var rowFieldToUpdate = "ktr_rowsortorder";
        var columnFieldToUpdate = "ktr_columnsortorder";
        int sortOrderNormal = 847610003;// Normal 
        var updateCount = 0;

        foreach (var record in recordsToUpdate)
        {
            record[rowFieldToUpdate] = new OptionSetValue(sortOrderNormal);
            record[columnFieldToUpdate] = new OptionSetValue(sortOrderNormal);
            await TargetService.UpdateAsync(record);
            updateCount++;
        }
        var result = MigrationResult.Successful($"Completed: {updateCount} rows processed");
        result.RecordsUpdated = updateCount;
        result.RecordsProcessed = updateCount;

        return result;
    }
}
