namespace Kantar.StudyDesignerLite.Migrations.Helpers;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kantar.StudyDesignerLite.Migrations.Enums;
using Kantar.StudyDesignerLite.Migrations.Models;

[ExcludeFromCodeCoverage]
public static class CommandLineHelper
{
    public static CommandLineOptions ParseArguments(string[] args)
    {
        var options = new CommandLineOptions();

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--help":
                case "-h":
                    options.ShowHelp = true;
                    break;
                case "--source-environment":
                case "-s":
                    if (i + 1 < args.Length)
                    {
                        options.SourceEnvironment = args[++i];
                    }
                    break;
                case "--target-environment":
                case "-t":
                    if (i + 1 < args.Length)
                    {
                        options.TargetEnvironment = args[++i];
                    }
                    break;
                case "--migration":
                case "-m":
                    if (i + 1 < args.Length)
                    {
                        options.SpecificMigration = args[++i];
                    }
                    break;
                case "--dry-run":
                case "-d":
                    options.DryRun = true;
                    break;
                case "--validate-only":
                case "-v":
                    options.ValidateOnly = true;
                    break;
                case "--migration-types":
                    if (i + 1 < args.Length)
                    {
                        var types = args[++i].Split(',', StringSplitOptions.RemoveEmptyEntries);
                        options.MigrationTypes = types
                            .Select(t => Enum.Parse<MigrationType>(t, true))
                            .ToList();
                    }
                    break;
                case "--log-level":
                case "-l":
                    if (i + 1 < args.Length)
                    {
                        options.LogLevel = args[++i];
                    }
                    break;
                case "--debug":
                    options.Debug = true;
                    break;
            }
        }

        return options;
    }

}
