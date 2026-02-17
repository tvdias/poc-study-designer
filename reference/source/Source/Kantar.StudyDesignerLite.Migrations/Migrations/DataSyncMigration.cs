namespace Kantar.StudyDesignerLite.Migrations.Migrations;

using System;
using System.Linq;
using System.Threading.Tasks;
using Kantar.StudyDesignerLite.Migrations.Enums;
using Kantar.StudyDesignerLite.Migrations.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

public abstract class DataSyncMigration : BaseMigration
{
    public override MigrationType Type => MigrationType.DataSync;

    protected abstract string SourceTableName { get; }
    protected abstract string TargetTableName { get; }
    protected abstract string[] SyncColumns { get; }
    protected abstract string IdentifierColumn { get; }

    public sealed override async Task<MigrationResult> ExecuteAsync()
    {
        try
        {
            LogStart();

            // Step 1 - Read source data
            var sourceRecords = await ReadFromSourceAsync(SourceTableName, SyncColumns);

            if (!sourceRecords.Any())
            {
                return MigrationResult.Skipped($"No records found in {SourceTableName}");
            }

            Logger.LogInformation($"Found {sourceRecords.Count} records in source");

            // Step 2 - Sync to target
            var result = await SyncRecordsToTargetAsync(sourceRecords);

            Logger.LogInformation($"Data sync completed. Created: {result.RecordsCreated}, Updated: {result.RecordsUpdated}, Skipped: {result.RecordsSkipped}");

            LogEnd();

            return result;
        }
        catch (Exception ex)
        {
            return LogException(ex);
        }
    }

    protected virtual async Task<MigrationResult> SyncRecordsToTargetAsync(List<Entity> sourceRecords)
    {
        int created = 0, updated = 0, skipped = 0;

        foreach (var sourceRecord in sourceRecords)
        {
            try
            {
                var identifierValue = sourceRecord.GetAttributeValue<string>(IdentifierColumn);
                if (string.IsNullOrEmpty(identifierValue))
                {
                    skipped++;
                    continue;
                }

                // Check if record exists in target
                var targetFilter = new FilterExpression
                {
                    Conditions = { new ConditionExpression(IdentifierColumn, ConditionOperator.Equal, identifierValue) }
                };

                var targetRecords = await ReadFromTargetAsync(TargetTableName, SyncColumns, targetFilter);

                if (!targetRecords.Any())
                {
                    // Create new record
                    await CreateRecordInTargetAsync(sourceRecord);
                    created++;
                    Logger.LogDebug($"Created record: {identifierValue}");
                }
                else
                {
                    // Update existing record if needed
                    var targetRecord = targetRecords.First();
                    var wasUpdated = await UpdateRecordInTargetIfNeededAsync(sourceRecord, targetRecord);

                    if (wasUpdated)
                    {
                        updated++;
                        Logger.LogDebug($"Updated record: {identifierValue}");
                    }
                    else
                    {
                        skipped++;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Failed to sync record: {sourceRecord.Id}");
            }
        }

        var result = MigrationResult.Successful($"Synced {created + updated} records");
        result.RecordsCreated = created;
        result.RecordsUpdated = updated;
        result.RecordsSkipped = skipped;
        result.RecordsProcessed = sourceRecords.Count;

        return result;
    }

    protected virtual async Task CreateRecordInTargetAsync(Entity sourceRecord)
    {
        var targetRecord = CreateEntity(TargetTableName, []);

        foreach (var column in SyncColumns)
        {
            if (sourceRecord.Contains(column))
            {
                targetRecord[column] = sourceRecord[column];
            }
        }

        await TargetService.CreateAsync(targetRecord);
    }

    protected virtual async Task<bool> UpdateRecordInTargetIfNeededAsync(Entity sourceRecord, Entity targetRecord)
    {
        var updateEntity = new Entity(TargetTableName, targetRecord.Id);
        bool needsUpdate = false;

        var syncColumns = SyncColumns
            .Where(c => !c.Equals($"{TargetTableName}id", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        foreach (var column in syncColumns)
        {
            var sourceValue = sourceRecord.GetAttributeValue<object>(column);
            var targetValue = targetRecord.GetAttributeValue<object>(column);

            if (!Equals(sourceValue, targetValue))
            {
                updateEntity[column] = sourceValue;
                needsUpdate = true;
            }
        }

        if (needsUpdate)
        {
            await TargetService.UpdateAsync(updateEntity);
            return true;
        }

        return false;
    }
}
