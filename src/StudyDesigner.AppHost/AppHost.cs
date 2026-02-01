var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

// API Server
var api = builder.AddProject<Projects.StudyDesigner_API>("api")
    .WithReference(cache)
    .WaitFor(cache)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

// Frontend applications
var designerFrontend = builder.AddViteApp("designer-frontend", "../frontend")
    .WithReference(api)
    .WaitFor(api);

var reviewerFrontend = builder.AddViteApp("reviewer-frontend", "../frontend-reviewer")
    .WithReference(api)
    .WaitFor(api);

// Publish frontend files to API
api.PublishWithContainerFiles(designerFrontend, "wwwroot/designer");
api.PublishWithContainerFiles(reviewerFrontend, "wwwroot/reviewer");

// Azure Functions
builder.AddAzureFunctionsProject<Projects.ServiceBusConsumer>("servicebusconsumer");
builder.AddAzureFunctionsProject<Projects.StudyProcessor>("studyprocessor");

builder.Build().Run();
