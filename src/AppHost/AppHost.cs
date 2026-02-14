using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

// Check if running in test mode for Admin E2E tests
var testMode = Environment.GetEnvironmentVariable("ASPIRE_TEST_MODE");
var isAdminE2ETest = testMode == "AdminE2E";

// Check configuration for Azure Functions enablement
var enableAzureFunctions = builder.Configuration.GetValue<bool>("EnableAzureFunctions", false);

// Always add PostgreSQL - required by API
var postgres = builder.AddPostgres("postgres").AddDatabase("studydb");

// Configure API with conditional dependencies
var apiBuilder = builder.AddProject<Projects.Api>("api")
    .WithReference(postgres)
    .WaitFor(postgres);

// Add health check only when not in AdminE2E test mode (for faster startup in tests)
if (!isAdminE2ETest)
{
    apiBuilder = apiBuilder.WithHttpHealthCheck("/health");
}

var api = apiBuilder.WithExternalHttpEndpoints();

// Add Designer app only when not in AdminE2E test mode
if (!isAdminE2ETest)
{
    builder.AddViteApp("app-designer", "../Designer")
        .WithReference(api);
}

// Add Admin app only when not in AdminE2E test mode
// In AdminE2E mode, tests manually start the Vite dev server to avoid slow Aspire Vite integration
if (!isAdminE2ETest)
{
    builder.AddViteApp("app-admin", "../Admin")
        .WithReference(api);
}

// Add Azure Service Bus and Functions based on configuration or test mode
if (!isAdminE2ETest && enableAzureFunctions)
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
