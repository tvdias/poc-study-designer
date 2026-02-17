using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

// Check configuration for Azure Functions enablement
var enableAzureFunctions = builder.Configuration.GetValue<bool>("EnableAzureFunctions", false);

// Always add PostgreSQL - required by API
var postgres = builder.AddPostgres("postgres").AddDatabase("studydb");

// Configure API with health check
var api = builder.AddProject<Projects.Api>("api")
    .WithReference(postgres)
    .WaitFor(postgres)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithHttpCommand("/api/seed", "Seed Database", commandName: "seed-db",
        commandOptions: new() { Method = HttpMethod.Post, IconName = "DatabaseLightning" });

// Add Admin and Designer Vite apps
builder.AddViteApp("app-admin", "../Admin")
    .WithReference(api);

builder.AddViteApp("app-designer", "../Designer")
    .WithReference(api);

// Add Azure Service Bus and Functions based on configuration
if (enableAzureFunctions)
{
    var serviceBus = builder.AddAzureServiceBus("servicebus");
    
    serviceBus.AddServiceBusTopic("questions");
    serviceBus.AddServiceBusTopic("projects");

    builder.AddAzureFunctionsProject<Projects.CluedinProcessor>("func-cluedin-processor")
        .WithReference(serviceBus);

    builder.AddAzureFunctionsProject<Projects.ProjectsProcessor>("func-projects-processor")
        .WithReference(serviceBus);
}

builder.Build().Run();

public partial class Program { }
