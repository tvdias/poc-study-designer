var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");
var postgres = builder.AddPostgres("postgres").AddDatabase("studydb");
var serviceBus = builder.AddAzureServiceBus("servicebus")
    .AddTopic("questions")
    .AddTopic("projects");

var api = builder.AddProject<Projects.Api>("api")
    .WithReference(cache)
    .WithReference(postgres)
    .WaitFor(cache)
    .WaitFor(postgres)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

var appDesigner = builder.AddViteApp("app-designer", "../Designer")
    .WithReference(api)
    .WaitFor(api);

var appAdmin = builder.AddViteApp("app-admin", "../Admin")
    .WithReference(api)
    .WaitFor(api);

builder.AddAzureFunctionsProject<Projects.CluedinProcessor>("func-cluedin-processor")
    .WithReference(serviceBus);

builder.AddAzureFunctionsProject<Projects.ProjectsProcessor>("func-projects-processor")
    .WithReference(serviceBus);

builder.Build().Run();

public partial class Program { }
