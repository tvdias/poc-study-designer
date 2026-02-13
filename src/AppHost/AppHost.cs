using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
var enableAzureFunctions = builder.Configuration.GetValue<bool>("EnableAzureFunctions", false);

var postgres = builder.AddPostgres("postgres").AddDatabase("studydb");

var api = builder.AddProject<Projects.Api>("api")
    .WithReference(postgres)
    .WaitFor(postgres)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

var appDesigner = builder.AddViteApp("app-designer", "../Designer")
    .WithReference(api);

var appAdmin = builder.AddViteApp("app-admin", "../Admin")
    .WithReference(api);

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
