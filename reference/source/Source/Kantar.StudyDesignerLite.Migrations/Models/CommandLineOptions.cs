namespace Kantar.StudyDesignerLite.Migrations.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kantar.StudyDesignerLite.Migrations.Enums;

public class CommandLineOptions
{
    public string? SourceEnvironment { get; set; }

    public string TargetEnvironment { get; set; } = string.Empty;

    public string SpecificMigration { get; set; } = string.Empty;

    public bool DryRun { get; set; }

    public bool ValidateOnly { get; set; }

    public List<MigrationType>? MigrationTypes { get; set; }

    public string LogLevel { get; set; } = "Information";

    public bool ShowHelp { get; set; }

    public bool Debug { get; set; }
}
