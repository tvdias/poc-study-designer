namespace Kantar.StudyDesignerLite.Migrations.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Kantar.StudyDesignerLite.Migrations.Migrations;
using Kantar.StudyDesignerLite.Migrations.Models;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;

public class MigrationRunner
{
    private readonly IOrganizationServiceAsync? _sourceService;
    private readonly TargetServiceWrapper _targetService;
    private readonly ILogger<MigrationRunner> _logger;
    private readonly CommandLineOptions _options;

    public MigrationRunner(
        TargetServiceWrapper targetService,
        ILogger<MigrationRunner> logger,
        CommandLineOptions options,
        IOrganizationServiceAsync? sourceService = null)
    {
        if (sourceService != null)
        {
            _sourceService = sourceService;
        }
        _targetService = targetService;
        _logger = logger;
        _options = options;
    }

    public async Task<bool> RunMigrationsAsync(CommandLineOptions options)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Starting Kantar StudyDesignerLite Data Migration");
            if (!string.IsNullOrWhiteSpace(options.SourceEnvironment))
            {
                _logger.LogInformation($"Source Environment: {options.SourceEnvironment}");
            }
            _logger.LogInformation($"Target Environment: {options.TargetEnvironment}");
            _logger.LogInformation($"Dry Run: {options.DryRun}");
            _logger.LogInformation($"Validate Only: {options.ValidateOnly}");

            // Discover migrations
            var migrations = DiscoverMigrations();

            // Filter migrations
            migrations = FilterMigrations(migrations, options);

            if (!migrations.Any())
            {
                _logger.LogWarning("No migrations found to execute");
                return true;
            }

            _logger.LogInformation($"Found {migrations.Count} migrations to execute");

            // Validate if requested
            if (options.ValidateOnly)
            {
                return await ValidateMigrationsAsync(migrations);
            }

            // Execute migrations
            return await ExecuteMigrationsAsync(migrations, options.DryRun);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Migration execution failed");
            return false;
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation($"Total execution time: {stopwatch.Elapsed:mm\\:ss\\.fff}");
        }
    }

    private List<BaseMigration> DiscoverMigrations()
    {
        var migrations = new List<BaseMigration>();

        var migrationTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseMigration)))
            .ToList();

        foreach (var type in migrationTypes)
        {
            try
            {
                var migration = (BaseMigration)Activator.CreateInstance(type)!;
                if (_sourceService == null)
                {
                    migration.Initialize(_targetService.Service, _logger, _options.TargetEnvironment!);
                }
                else
                {
                    migration.Initialize(_sourceService, _targetService.Service, _logger, _options.SourceEnvironment!, _options.TargetEnvironment!);
                }
                migrations.Add(migration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create migration instance: {type.Name}");
            }
        }

        return migrations.OrderBy(m => m.ExecutionOrder).ToList();
    }

    private List<BaseMigration> FilterMigrations(List<BaseMigration> migrations, CommandLineOptions options)
    {
        var filtered = migrations.AsEnumerable();

        // Filter by specific migration name
        if (!string.IsNullOrEmpty(options.SpecificMigration))
        {
            filtered = filtered.Where(m => m.Description.Contains(options.SpecificMigration, StringComparison.OrdinalIgnoreCase));
        }

        // Filter by migration types
        if (options.MigrationTypes?.Any() == true)
        {
            filtered = filtered.Where(m => options.MigrationTypes.Contains(m.Type));
        }

        // Filter by target environment
        filtered = filtered.Where(m => m.TargetEnvironments.Contains(options.TargetEnvironment, StringComparer.OrdinalIgnoreCase));

        return filtered.ToList();
    }

    private async Task<bool> ValidateMigrationsAsync(List<BaseMigration> migrations)
    {
        _logger.LogInformation("Validating migrations...");
        bool allValid = true;

        foreach (var migration in migrations)
        {
            try
            {
                _logger.LogInformation($"Validating: {migration.MigrationUniqueId}");
                var result = await migration.ValidateAsync();

                if (result.Success)
                {
                    _logger.LogInformation($"✓ {migration.MigrationUniqueId} - Valid");
                }
                else
                {
                    _logger.LogError($"✗ {migration.MigrationUniqueId} - Invalid: {result.Message}");
                    allValid = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"✗ {migration.MigrationUniqueId} - Validation failed");
                allValid = false;
            }
        }

        _logger.LogInformation($"Validation completed. Result: {(allValid ? "All Valid" : "Validation Failures")}");
        return allValid;
    }

    private async Task<bool> ExecuteMigrationsAsync(List<BaseMigration> migrations, bool dryRun)
    {
        bool allSuccessful = true;
        int successful = 0, failed = 0, skipped = 0;

        foreach (var migration in migrations)
        {
            var migrationStopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation($"Executing: {migration.MigrationUniqueId}");

                if (dryRun)
                {
                    _logger.LogInformation("DRY RUN MODE - No changes will be made");
                }

                var result = await migration.ExecuteAsync();
                migrationStopwatch.Stop();

                if (result.Success)
                {
                    if (result.Message.StartsWith("Skipped"))
                    {
                        _logger.LogInformation($"⊝ {migration.MigrationUniqueId} - {result.Message} ({migrationStopwatch.Elapsed.TotalSeconds:F1}s)");
                        skipped++;
                    }
                    else
                    {
                        _logger.LogInformation($"✓ {migration.MigrationUniqueId} - {result.Message} ({migrationStopwatch.Elapsed.TotalSeconds:F1}s)");
                        successful++;
                    }
                }
                else
                {
                    _logger.LogError($"✗ {migration.MigrationUniqueId} - {result.Message} ({migrationStopwatch.Elapsed.TotalSeconds:F1}s)");
                    failed++;
                    allSuccessful = false;

                    if (migration.IsRequired)
                    {
                        _logger.LogError("Required migration failed. Stopping execution.");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                migrationStopwatch.Stop();
                _logger.LogError(ex, $"✗ {migration.MigrationUniqueId} - Exception occurred ({migrationStopwatch.Elapsed.TotalSeconds:F1}s)");
                failed++;
                allSuccessful = false;

                if (migration.IsRequired)
                {
                    _logger.LogError("Required migration failed. Stopping execution.");
                    break;
                }
            }
        }

        _logger.LogInformation($"Migration execution completed. Successful: {successful}, Failed: {failed}, Skipped: {skipped}");
        return allSuccessful;
    }
}
