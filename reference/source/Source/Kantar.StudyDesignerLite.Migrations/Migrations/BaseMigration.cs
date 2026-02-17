namespace Kantar.StudyDesignerLite.Migrations.Migrations;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kantar.StudyDesignerLite.Migrations.Enums;
using Kantar.StudyDesignerLite.Migrations.Models;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

public abstract class BaseMigration
{
    protected IOrganizationServiceAsync SourceService { get; private set; } = null!;
    protected IOrganizationServiceAsync TargetService { get; private set; } = null!;
    protected ILogger Logger { get; private set; } = null!;
    protected string SourceEnvironment { get; private set; } = string.Empty;
    protected string TargetEnvironment { get; private set; } = string.Empty;

    public string MigrationUniqueId { get; set; } = string.Empty;

    public abstract int ExecutionOrder { get; }
    public abstract string Description { get; }
    public abstract MigrationType Type { get; }
    public virtual bool IsRequired => true;
    public virtual string[] TargetEnvironments => ["DEV", "TEST", "UAT", "PREPROD", "PROD"];

    public void Initialize(
        IOrganizationServiceAsync sourceService,
        IOrganizationServiceAsync targetService,
        ILogger logger,
        string sourceEnvironment,
        string targetEnvironment)
    {
        SourceService = sourceService;
        TargetService = targetService;
        Logger = logger;
        SourceEnvironment = sourceEnvironment;
        TargetEnvironment = targetEnvironment;

        MigrationUniqueId = GetMigrationIdentifier();
    }

    public void Initialize(
        IOrganizationServiceAsync targetService,
        ILogger logger,
        string targetEnvironment)
    {
        TargetService = targetService;
        Logger = logger;
        TargetEnvironment = targetEnvironment;

        MigrationUniqueId = GetMigrationIdentifier();
    }

    public abstract Task<MigrationResult> ExecuteAsync();

    public virtual Task<MigrationResult> ValidateAsync() => Task.FromResult(MigrationResult.Successful("Validation passed"));

    #region Helper methods for common operations

    protected static Entity CreateEntity(string logicalName, Dictionary<string, object> attributes)
    {
        var entity = new Entity(logicalName);
        foreach (var attr in attributes)
        {
            entity[attr.Key] = attr.Value;
        }
        return entity;
    }

    protected async Task<List<Entity>> ReadFromSourceAsync(string entityName, string[] columns, FilterExpression? filter = null)
    {
        var query = new QueryExpression(entityName)
        {
            ColumnSet = new ColumnSet(columns)
        };

        if (filter != null)
        {
            query.Criteria = filter;
        }

        var result = await SourceService.RetrieveMultipleAsync(query);

        return result.Entities
            .ToList();
    }

    protected async Task<List<Entity>> ReadFromTargetAsync(string entityName, string[] columns, FilterExpression? filter = null, LinkEntity? linkEntity = null)
    {
        var query = new QueryExpression(entityName)
        {
            ColumnSet = new ColumnSet(columns)
        };

        if (filter != null)
        {
            query.Criteria = filter;
        }

        if (linkEntity != null)
        {
            query.LinkEntities.Add(linkEntity);
        }

        var result = await TargetService.RetrieveMultipleAsync(query);

        return result.Entities
            .ToList();
    }

    protected void LogStart()
    {
        if (string.IsNullOrEmpty(SourceEnvironment))
        {
            Logger.LogInformation($"{MigrationUniqueId}: Starting migration in {TargetEnvironment}...");
        }
        else
        {
            Logger.LogInformation($"{MigrationUniqueId}: Starting migration from {SourceEnvironment} to {TargetEnvironment}...");
        }
    }

    protected void LogEnd()
    {
        Logger.LogInformation($"{MigrationUniqueId}: Completed migration in {TargetEnvironment}.");
    }

    protected void LogSkip(string reason)
    {
        Logger.LogInformation($"{MigrationUniqueId}: Skipped migration in {TargetEnvironment}: {reason}");
    }

    protected MigrationResult LogException(Exception ex)
    {
        Logger.LogError(ex, $"{MigrationUniqueId}: Failed migration in {TargetEnvironment}");
        return MigrationResult.Failed($"{MigrationUniqueId}: Failed migration in {TargetEnvironment}: {ex.Message}", ex);
    }

    private string GetMigrationIdentifier()
    {
        return $"{ExecutionOrder}_{Type}_{GetType().Name}";
    }

    #endregion
}
