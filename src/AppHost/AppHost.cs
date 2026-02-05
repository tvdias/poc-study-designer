var builder = DistributedApplication.CreateBuilder(args);

// Check if running in test mode for Admin E2E tests
var testMode = Environment.GetEnvironmentVariable("ASPIRE_TEST_MODE");
var isAdminE2ETest = testMode == "AdminE2E";

// Always add PostgreSQL - required by API
var postgres = builder.AddPostgres("postgres").AddDatabase("studydb");

// Add Redis and Service Bus only when not in AdminE2E test mode
var cache = isAdminE2ETest ? null : builder.AddRedis("cache");
var serviceBus = isAdminE2ETest ? null : builder.AddAzureServiceBus("servicebus")
    .AddTopic("questions")
    .AddTopic("projects");

// Configure API with conditional dependencies
var apiBuilder = builder.AddProject<Projects.Api>("api")
    .WithReference(postgres)
    .WaitFor(postgres);

if (cache != null)
{
    apiBuilder = apiBuilder
        .WithReference(cache)
        .WaitFor(cache);
}

if (!isAdminE2ETest)
{
    apiBuilder = apiBuilder.WithHttpHealthCheck("/health");
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

// Add Azure Functions only when not in AdminE2E test mode
if (!isAdminE2ETest)
{
    builder.AddAzureFunctionsProject<Projects.CluedinProcessor>("func-cluedin-processor")
        .WithReference(serviceBus!);

    builder.AddAzureFunctionsProject<Projects.ProjectsProcessor>("func-projects-processor")
        .WithReference(serviceBus!);
}

builder.Build().Run();

public partial class Program { }
