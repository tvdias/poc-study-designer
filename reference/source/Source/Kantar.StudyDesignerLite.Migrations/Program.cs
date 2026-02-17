namespace Kantar.StudyDesignerLite.Migrations;

using Kantar.StudyDesignerLite.Migrations.Enums;
using Kantar.StudyDesignerLite.Migrations.Helpers;
using Kantar.StudyDesignerLite.Migrations.Models;
using Kantar.StudyDesignerLite.Migrations.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            var options = CommandLineHelper.ParseArguments(args);

            if (options.ShowHelp)
            {
                ShowHelp();
                return 0;
            }

            if (options.MigrationTypes == null)
            {
                Console.WriteLine("Error: --migration-types parameter is required");
                ShowHelp();
                return 1;
            }

            if (string.IsNullOrEmpty(options.TargetEnvironment))
            {
                Console.WriteLine("Error: --target-environment parameter is required");
                ShowHelp();
                return 1;
            }

            if (string.IsNullOrEmpty(options.SourceEnvironment)
                && !options.MigrationTypes.Contains(MigrationType.FixData))
            {
                Console.WriteLine("Error: --source-environment parameter is required");
                ShowHelp();
                return 1;
            }

            // Create host and run migrations
            var host = CreateHost(options);
            using var scope = host.Services.CreateScope();

            var migrationRunner = scope.ServiceProvider.GetRequiredService<MigrationRunner>();
            var success = await migrationRunner.RunMigrationsAsync(options);

            return success ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            if (args.Contains("--debug"))
            {
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            return 1;
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("Kantar StudyDesignerLite Data Migration Tool");
        Console.WriteLine("Fixes & Syncs data from source environment to target environment");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run -- --source-environment <env> --target-environment <env> --migration-types <types> [options]");
        Console.WriteLine();
        Console.WriteLine("Required Parameters:");
        Console.WriteLine("  --source-environment, -s <env>     Source environment (Production, PreProd, Test, Development)");
        Console.WriteLine("  --target-environment, -t <env>     Target environment (Production, PreProd, Test, Development)");
        Console.WriteLine("  --migration-types <types>          Migration types to run (FixData,Security,DataSync)");
        Console.WriteLine();
        Console.WriteLine("Optional Parameters:");
        Console.WriteLine("  --migration, -m <name>             Run specific migration only");
        Console.WriteLine("  --dry-run, -d                      Preview changes without applying");
        Console.WriteLine("  --validate-only, -v                Only validate connections and migrations");
        Console.WriteLine("  --log-level, -l <level>            Logging level (Trace, Debug, Information, Warning, Error)");
        Console.WriteLine("  --debug                            Show detailed error information");
        Console.WriteLine("  --help, -h                         Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotnet run -- --target-environment Test --migration-types FixData");
        Console.WriteLine("  dotnet run -- --source-environment Test --target-environment Development --migration-types DataSync --dry-run");
        Console.WriteLine("  dotnet run -- --source-environment PreProd --target-environment Test --migration-types DataSync,Security");
        Console.WriteLine("  dotnet run -- --source-environment Production --target-environment UAT --migration-types DataSync --validate-only");
        Console.WriteLine();
        Console.WriteLine("Common Use Cases:");
        Console.WriteLine("  Run Fix Data in Test: Run a simple fix data in Test Env, no source needed");
        Console.WriteLine("  Test → Development:        Safe testing with Test data");
        Console.WriteLine("  PreProd → UAT:            Pre-production validation");
        Console.WriteLine("  Production → PreProd:      Production promotion");
    }

    static IHost CreateHost(CommandLineOptions options)
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config
                    .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
                    .AddJsonFile("appsettings.json", optional: true) // for local run
                    .AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;

                // Register source service
                if (!string.IsNullOrEmpty(options.SourceEnvironment))
                {
                    services.AddScoped<IOrganizationServiceAsync>(provider =>
                    {
                        var sourceConnectionString = configuration.GetConnectionString(options.SourceEnvironment);
                        if (string.IsNullOrEmpty(sourceConnectionString))
                        {
                            throw new InvalidOperationException($"Source connection string for '{options.SourceEnvironment}' not found");
                        }

                        var sourceClient = new ServiceClient(sourceConnectionString);
                        if (!sourceClient.IsReady)
                        {
                            throw new InvalidOperationException($"Failed to connect to source environment '{options.SourceEnvironment}': {sourceClient.LastError}");
                        }

                        return sourceClient;
                    });
                }

                // Register target service
                services.AddScoped(provider =>
                {
                    var targetConnectionString = configuration.GetConnectionString(options.TargetEnvironment);
                    if (string.IsNullOrEmpty(targetConnectionString))
                    {
                        throw new InvalidOperationException($"Target connection string for '{options.TargetEnvironment}' not found");
                    }

                    var targetClient = new ServiceClient(targetConnectionString);
                    if (!targetClient.IsReady)
                    {
                        throw new InvalidOperationException($"Failed to connect to target environment '{options.TargetEnvironment}': {targetClient.LastError}");
                    }

                    return new TargetServiceWrapper(targetClient);
                });

                services.AddScoped<MigrationRunner>();
                services.AddSingleton(options);
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();

                var logLevel = options.LogLevel?.ToLower() switch
                {
                    "trace" => LogLevel.Trace,
                    "debug" => LogLevel.Debug,
                    "information" => LogLevel.Information,
                    "warning" => LogLevel.Warning,
                    "error" => LogLevel.Error,
                    _ => LogLevel.Information
                };

                logging.SetMinimumLevel(logLevel);
            })
            .Build();
    }
}

