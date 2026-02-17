namespace Kantar.StudyDesignerLite.Migrations.Migrations;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kantar.StudyDesignerLite.Migrations.Enums;
using Kantar.StudyDesignerLite.Migrations.Models;
using Microsoft.Xrm.Sdk;

public abstract class FixDataMigration : BaseMigration
{
    public override MigrationType Type => MigrationType.FixData;

    public sealed override async Task<MigrationResult> ExecuteAsync()
    {
        try
        {
            LogStart();

            // Step 1 - Get records to update
            var recordsToUpdate = await GetRecordsToUpdate();

            if (recordsToUpdate.Count == 0)
            {
                var reasonSkip = "No records found to update.";
                LogSkip(reasonSkip);
                return MigrationResult.Skipped(reasonSkip);
            }

            // Step 2 - Apply updates
            var migrationResult = await UpdateRecords(recordsToUpdate);

            LogEnd();

            return migrationResult;
        }
        catch (Exception ex)
        {
            return LogException(ex);
        }
    }

    protected abstract Task<List<Entity>> GetRecordsToUpdate();

    protected abstract Task<MigrationResult> UpdateRecords(List<Entity> records);
}
