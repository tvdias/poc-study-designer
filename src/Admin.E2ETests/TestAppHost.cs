using Aspire.Hosting;

namespace Admin.E2ETests;

/// <summary>
/// Test-specific AppHost configuration that only starts services required for Admin E2E tests.
/// This is faster than the full AppHost as it skips unnecessary services like Azure Functions,
/// Service Bus, Designer app, and Redis.
/// </summary>
public static class TestAppHost
{
    public static IDistributedApplicationBuilder CreateBuilder(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        // Only add PostgreSQL - required by API
        var postgres = builder.AddPostgres("postgres").AddDatabase("studydb");

        // Add API with only PostgreSQL dependency (skip Redis/Service Bus)
        // Using the path relative to the test project
        var api = builder.AddProject("api", "../Api/Api.csproj")
            .WithReference(postgres)
            .WaitFor(postgres)
            .WithHttpHealthCheck("/health")
            .WithExternalHttpEndpoints();

        // Add Admin app without any dependencies or wait conditions
        // The Admin app will connect to API via environment variables set by Aspire
        builder.AddViteApp("app-admin", "../Admin")
            .WithReference(api);

        return builder;
    }

    public static DistributedApplication Build(string[] args)
    {
        return CreateBuilder(args).Build();
    }
}

