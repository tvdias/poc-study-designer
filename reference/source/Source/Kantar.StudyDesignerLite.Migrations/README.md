# Kantar StudyDesignerLite Data Migration Tool

Enterprise-grade migrations for Dataverse environments, designed for CI/CD pipeline integration.

## Overview

This SDK-based migration application synchronizes supporting data from a source environment to target environments (Development, Test, UAT, PreProd, Prod) post-deployment.
It also addresses the core requirement to Fix Data that cannot be included in managed solutions.

## Migration Types Supported

1. **Fix Data** - Fix data in tables in the higher envs
2. **Field Security Profile Assignment** - Field Security Profile Assignment Sync Template from source to target
3. **User Personalization Settings** - User preferences sync from source to target
4. **Environment Settings** - Environment-specific configurations and parameters
5. **Data Sync** - Table-to-table data migration with validation

## Architecture Benefits

**SDK-Native**: Direct Dataverse API operations, no SQL translation overhead
**Source-to-Target**: Pulls current data from Source (Maybe Production), applies to targets
**Configuration-Driven**: Environment-specific settings via JSON configuration
**CI/CD Ready**: Direct pipeline integration with proper error handling
**Focused**: Addresses exactly 5 core migration scenarios

## Project Structure

```
Kantar.StudyDesignerLite.Migrations/
├── Migrations/
│   └── BaseMigration.cs          # Base migration class
├── Migrations/
│   ├── FixData/
│   │   └── 001_SomeFixDataInTable.cs
│   ├── Security/
│       └── ColumnLevelSecurityMigration.cs # Role assignments
├── Program.cs                              # Main application
├── appsettings.json                        # Base configuration
└── Kantar.StudyDesignerLite.Migrations.csproj
```

## Quick Start

### 1. Configuration Setup

Update `appsettings.json` with environment connection strings:

```json
{
  "ConnectionStrings": {
    "Production": "AuthType=ClientSecret;Url=https://prod.crm.dynamics.com;ClientId=...;ClientSecret=...;RequireNewInstance=true",
    "Development": "AuthType=ClientSecret;Url=https://dev.crm.dynamics.com;ClientId=...;ClientSecret=...;RequireNewInstance=true",
    "Test": "AuthType=ClientSecret;Url=https://test.crm.dynamics.com;ClientId=...;ClientSecret=...;RequireNewInstance=true"
  }
}
```

### 2. Build & Test

```bash
# Build the application
dotnet build

# Test connections
dotnet run -- --target-environment Test --migration-types FixData --validate-only

# Preview changes (dry run)
dotnet run -- --target-environment Test --migration-types FixData --dry-run

# Execute migrations
dotnet run -- --target-environment Test --migration-types FixData
```

## Usage Examples

```bash
# Fix data in an environment
dotnet run -- --target-environment Test --migration-types FixData

# Run specific Fix data scropts in an environment
dotnet run -- --target-environment Test --migration-types FixData --migration DependencyRuleMultiCodedTriggeringAnswer

# Sync data from Production to Development
dotnet run -- --source-environment Production --target-environment Development --migration-types DataSync

# Sync specific migration types
dotnet run -- --source-environment Production --target-environment Test --migration-types DataSync,Security

# Run specific migration
dotnet run -- --source-environment Production --target-environment UAT --migration-types DataSync --migration SyncConfigurationQuestions

# Dry run with debug logging
dotnet run -- --source-environment Production --target-environment PreProd --migration-types DataSync --dry-run --log-level Debug

# Safe development testing (Test as source)
dotnet run -- --source-environment Test --target-environment Development --migration-types DataSync --validate-only
```

## CI/CD Pipeline Integration

### Azure DevOps Pipeline

```yaml
stages:
- stage: DeployToTest
  jobs:
  - deployment: DeployApplication
    environment: Test
    strategy:
      runOnce:
        deploy:
          steps:
          # Deploy managed solution first
          - task: PowerPlatformPublishCustomizations@2
            inputs:
              authenticationType: 'PowerPlatformSPN'
              PowerPlatformSPN: $(PowerPlatformSPN)
              
          # Run data migrations after solution deployment
          - task: DotNetCoreCLI@2
            displayName: 'Run Data Migrations'
            inputs:
              command: 'run'
              projects: 'Kantar.StudyDesignerLite.Migrations/*.csproj'
              arguments: '-- --target-environment Test --migration-types FixData'
            env:
              ConnectionStrings__Production: $(PROD_DATAVERSE_CONNECTION)
              ConnectionStrings__Test: $(TEST_DATAVERSE_CONNECTION)
```

## Migration Examples

### Fix Data (SQL Script Data)

Reads the 12 configuration questions with AI prompts from Production and applies them to target environment:

- "Which gender question would you like to use?"
- "What type of category is this survey for?"
- "How would you like unaided awareness to be coded?"
- [And 9 more from original SQL script]

## Key Features

**Dual Environment Connections**
- Connects to both source and target environments simultaneously
- Validates connections before executing migrations
- Handles connection failures gracefully

**Smart Synchronization**
- Only updates records that have actually changed
- Creates missing records, updates existing ones
- Skips unchanged data to improve performance

**Comprehensive Logging**
- Detailed execution logs with timing
- Success/failure tracking per migration
- Integration with Application Insights (optional)

**Validation & Safety**
- Dry-run mode for previewing changes
- Connection validation before execution  
- Environment-specific safety controls

## Extending the Framework

### Adding New Migrations

1. Create new migration class inheriting from `BaseMigration`
2. Implement the required abstract methods
3. Place in appropriate folder (FixData/Security/DataSync)
4. The framework auto-discovers and executes it

```csharp
public class SyncCustomData : BaseMigration
{
    public override int ExecutionOrder => 5;
    public override string Description => "Sync custom data from source to target";
    public override MigrationType Type => MigrationType.DataSync;

    public override async Task<MigrationResult> ExecuteAsync()
    {
        // Read from source environment
        var sourceData = await ReadFromSourceAsync("ktr_customentity", 
            new[] { "ktr_name", "ktr_value" });
        
        // Apply to target environment
        // ... implementation
        
        return MigrationResult.Successful($"Synced {sourceData.Count} records");
    }
}
```

## Troubleshooting

**Migration Failures:**
- Use `--dry-run` to preview changes
- Check logs for specific error details
- Validate source data exists before syncing

## Summary

This framework provides the robust, SDK-native solution you need for post-deployment data synchronization. It handles specific configuration question updates (from the SQL script) while providing a scalable foundation for future migration needs.

**Running this locally:**
- appsettings.json and add the below settings that includes the connections strings - See example above

{
  "ConnectionStrings": {
    "Production": "",
    "Development": "",
    "Test": "",
    "UAT": "",
    "PreProd": ""
  },
  "Environments": {
    "Production": {
      "BusinessUnitName": "UC1 Production",
      "AllowDestructiveOperations": false
    },
    "PreProd": {
      "BusinessUnitName": "UC1 PreProd",
      "AllowDestructiveOperations": false
    },
    "UAT": {
      "BusinessUnitName": "UC1 UAT",
      "AllowDestructiveOperations": true
    },
    "Test": {
      "BusinessUnitName": "UC1 Test",
      "AllowDestructiveOperations": true
    },
    "Development": {
      "BusinessUnitName": "UC1 Development",
      "AllowDestructiveOperations": true
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Kantar.StudyDesignerLite.Migrations": "Information",
      "Microsoft": "Warning",
      "System": "Warning"
    }
  }
}