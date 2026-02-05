var builder = DistributedApplication.CreateBuilder(args);

// Check if running in test mode for Admin E2E tests
var testMode = Environment.GetEnvironmentVariable("ASPIRE_TEST_MODE");
var isAdminE2ETest = testMode == "AdminE2E";

// Always add PostgreSQL - required by API
var postgres = builder.AddPostgres("postgres").AddDatabase("studydb");

// Configure API with conditional dependencies
var apiBuilder = builder.AddProject<Projects.Api>("api")
    .WithReference(postgres)
    .WaitFor(postgres);

// Add Redis and configure API to use it only when not in AdminE2E test mode
if (!isAdminE2ETest)
{
    var cache = builder.AddRedis("cache");
    apiBuilder = apiBuilder
        .WithReference(cache)
        .WaitFor(cache)
        .WithHttpHealthCheck("/health");
}

var api = apiBuilder.WithExternalHttpEndpoints();

// Add Designer app only when not in AdminE2E test mode
if (!isAdminE2ETest)
{
    builder.AddViteApp("app-designer", "../Designer")
        .WithReference(api)
        .WaitFor(api);
}

// Always add Admin app
var appAdmin = builder.AddViteApp("app-admin", "../Admin")
    .WithReference(api);

if (!isAdminE2ETest)
{
    appAdmin = appAdmin.WaitFor(api);
}

// Add Azure Service Bus and Functions only when not in AdminE2E test mode
if (!isAdminE2ETest)
{
    var serviceBus = builder.AddAzureServiceBus("servicebus")
        .AddTopic("questions")
        .AddTopic("projects");

    builder.AddAzureFunctionsProject<Projects.CluedinProcessor>("func-cluedin-processor")
        .WithReference(serviceBus);

    builder.AddAzureFunctionsProject<Projects.ProjectsProcessor>("func-projects-processor")
        .WithReference(serviceBus);
}

builder.Build().Run();

public partial class Program { }
