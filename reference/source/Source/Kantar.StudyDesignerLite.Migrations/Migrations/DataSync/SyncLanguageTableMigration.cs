namespace Kantar.StudyDesignerLite.Migrations.Migrations.DataSync;

public class SyncLanguageTableMigration : DataSyncMigration
{
    public override int ExecutionOrder => 1;
    public override string Description => "Reference data Table-to-table synchronization";

    protected override string SourceTableName => "ktr_language";

    protected override string TargetTableName => "ktr_language";

    protected override string[] SyncColumns => ["ktr_languageid", "ktr_name", "ktr_country", "ktr_countrycode", "ktr_direction"];

    protected override string IdentifierColumn => "ktr_name";
}
