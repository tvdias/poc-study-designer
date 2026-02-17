namespace Kantar.StudyDesignerLite.Migrations.Migrations.FixData;

using System.Collections.Generic;
using System.Threading.Tasks;
using Kantar.StudyDesignerLite.Migrations.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

public class MLEntitiesSetDefaultValueDisplayOrder : FixDataMigration
{
    public override int ExecutionOrder => 14;
    public override string Description => "MLEntities Set Default Value for DisplayOrder";

    private string _entityLogicalName = "ktr_managedlistentity";
    private string _sortOrderFieldName = "ktr_displayorder";

    protected override async Task<List<Entity>> GetRecordsToUpdate()
    {
        var filter = new FilterExpression(LogicalOperator.Or)
        {
            Conditions =
            {
                new ConditionExpression("ktr_displayorder", ConditionOperator.Null),
                new ConditionExpression("ktr_displayorder", ConditionOperator.Equal, 999)
            }
        };

        string[] columns = ["ktr_managedlistentityid", "ktr_managedlist", "ktr_displayorder", "createdon"];

        var result = await ReadFromTargetAsync(
            "ktr_managedlistentity",
            columns,
            filter
        ) ?? [];

        return result;
    }

    #region Reorder Helper
    public async Task<bool> ReorderEntities(
            IEnumerable<Guid> ids)
    {
        if (ids == null || !ids.Any())
        {
            return false;
        }

        var orderedRows = ToSequentialOrder(ids);

        var success = await UpdateBulkEntity(orderedRows);

        return success;
    }

    public static IDictionary<Guid, int> ToSequentialOrder(IEnumerable<Guid> rows)
    {
        if (rows == null || rows.Count() == 0)
        {
            return null;
        }

        var sortOrder = 0;
        var result = new Dictionary<Guid, int>() { };
        foreach (var rowId in rows)
        {
            result.Add(rowId, sortOrder++);
        }

        return result;
    }
    #endregion

    #region Queries to Dataverse 
    private async Task<bool> UpdateBulkEntity(
       IDictionary<Guid, int> orderedRows)
    {
        if (orderedRows == null || orderedRows.Count == 0)
        {
            return false;
        }

        var updateRequests = new OrganizationRequestCollection();

        foreach (var row in orderedRows)
        {
            var entity = new Entity(_entityLogicalName, row.Key)
            {
                [_sortOrderFieldName] = row.Value,
            };

            updateRequests.Add(new UpdateRequest { Target = entity });
        }

        var executeMultiple = new ExecuteMultipleRequest
        {
            Requests = updateRequests,
            Settings = new ExecuteMultipleSettings
            {
                ContinueOnError = false,
                ReturnResponses = true
            }
        };

        var response = (ExecuteMultipleResponse)await TargetService.ExecuteAsync(executeMultiple);

        if (response.IsFaulted)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private async Task<List<Entity>> GetMLEntities(List<Guid> managedListIds)
    {
        if (managedListIds == null || managedListIds.Count == 0)
        {
            return new List<Entity>();
        }

        var filter = new FilterExpression()
        {
            Conditions =
            {
                new ConditionExpression("ktr_managedlist", ConditionOperator.In, managedListIds.Cast<object>().ToArray()),
                new ConditionExpression("statuscode", ConditionOperator.Equal, 1), //Active 
            }
        };

        string[] columns = ["ktr_managedlistentityid", "ktr_managedlist", "ktr_displayorder", "createdon"];

        var result = await ReadFromTargetAsync(
            "ktr_managedlistentity",
            columns,
            filter
        ) ?? [];

        return result;
    }
    #endregion

    protected override async Task<MigrationResult> UpdateRecords(List<Entity> recordsToUpdate)
    {
        var updateCount = 0;

        var managedLists = recordsToUpdate
            .Where(e => e.Contains("ktr_managedlist"))
            .Select(e => e.GetAttributeValue<EntityReference>("ktr_managedlist")!.Id)
            .Distinct()
            .ToList();

        var allManagedListEntities = await GetMLEntities(managedLists);

        if (allManagedListEntities == null || allManagedListEntities.Count == 0)
        {
            return MigrationResult.Skipped("No managed list entities found to fix data.");
        }

        foreach (var managedListId in managedLists)
        {
            var managedListEntities = allManagedListEntities
                .Where(e => e.GetAttributeValue<EntityReference>("ktr_managedlist")!.Id == managedListId)
                .ToList();

            if (managedListEntities == null || managedListEntities.Count == 0)
            {
                return MigrationResult.Skipped("No managed list entities found to fix data for this ML.");
            }

            var ids = managedListEntities
                .OrderBy(ql => ql.GetAttributeValue<int>("ktr_displayorder"))
                .ThenBy(ql => ql.GetAttributeValue<DateTime>("createdon"))
                .Select(ql => ql.Id)
                .ToList();

            var success = await ReorderEntities(ids);

            if (success)
            {
                updateCount++;
            }
        }

        var result = MigrationResult.Successful($"Completed: {updateCount} rows processed");
        result.RecordsUpdated = updateCount;
        result.RecordsProcessed = updateCount;

        return result;
    }
}
