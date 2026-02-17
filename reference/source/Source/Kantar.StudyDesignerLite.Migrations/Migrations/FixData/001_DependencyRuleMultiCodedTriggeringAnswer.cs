namespace Kantar.StudyDesignerLite.Migrations.Migrations.FixData;

using System.Collections.Generic;
using System.Threading.Tasks;
using Kantar.StudyDesignerLite.Migrations.Enums;
using Kantar.StudyDesignerLite.Migrations.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

/// <summary>
/// 
/// UPDATE dr
/// SET dr.ktr_triggeringanswer = NULL
/// FROM ktr_dependencyrule dr
///     INNER JOIN ktr_configurationquestion cq ON cq.ktr_configurationquestionid = dr.ktr_configurationquestion
/// WHERE cq.ktr_rule = @Rule_MultiCoded AND dr.ktr_triggeringanswer IS NOT NULL;
///
/// </summary>
public class DependencyRuleMultiCodedTriggeringAnswer : FixDataMigration
{
    public override int ExecutionOrder => 1;
    public override string Description => "Clear triggering answer for dependency rules with MultiCoded questions";
    private readonly int _ruleMultiCoded = 847610001;

    protected override async Task<List<Entity>> GetRecordsToUpdate()
    {
        var filter = new FilterExpression
        {
            Conditions =
            {
                new ConditionExpression("ktr_triggeringanswer", ConditionOperator.NotNull)
            }
        };

        var linkEntity = new LinkEntity
        {
            LinkFromEntityName = "ktr_dependencyrule",
            LinkFromAttributeName = "ktr_configurationquestion",
            LinkToEntityName = "ktr_configurationquestion",
            LinkToAttributeName = "ktr_configurationquestionid",
            JoinOperator = JoinOperator.Inner,
            Columns = new ColumnSet(false),
            LinkCriteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("ktr_rule", ConditionOperator.Equal, _ruleMultiCoded)
                }
            }
        };

        var result = await ReadFromTargetAsync(
            "ktr_dependencyrule",
            ["ktr_dependencyruleid", "ktr_triggeringanswer"],
            filter,
            linkEntity
        );

        return result;
    }

    protected override async Task<MigrationResult> UpdateRecords(List<Entity> recordsToUpdate)
    {
        var fieldToUpdate = "ktr_triggeringanswer";
        string? newValue = null;
        var updateCount = 0;

        foreach (var record in recordsToUpdate)
        {
            record[fieldToUpdate] = newValue;
            await TargetService.UpdateAsync(record);
            updateCount++;
        }
        var result = MigrationResult.Successful($"Completed: {updateCount} rows processed");
        result.RecordsUpdated = updateCount;
        result.RecordsProcessed = updateCount;

        return result;
    }
}
