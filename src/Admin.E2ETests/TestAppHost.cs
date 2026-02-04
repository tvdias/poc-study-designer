using Aspire.Hosting;
using System.Runtime.CompilerServices;

namespace Admin.E2ETests;

/// <summary>
/// Test-specific AppHost configuration that only starts services required for Admin E2E tests.
/// This is faster than the full AppHost as it skips unnecessary services like Azure Functions,
/// Service Bus, Designer app, and Redis.
/// </summary>
public static class TestAppHost
{
    /// <summary>
    /// Gets the source directory of this file to properly resolve project paths.
    /// This ensures paths work correctly regardless of where the test assembly runs from.
    /// </summary>
    private static string GetSourceDirectory([CallerFilePath] string path = "")
        => Path.GetDirectoryName(path)!;

    public static IDistributedApplicationBuilder CreateBuilder(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);
        
        // Get the source directory and navigate to the src folder
        var sourceDir = GetSourceDirectory();
        var srcDir = Path.GetFullPath(Path.Combine(sourceDir, ".."));

        // Only add PostgreSQL - required by API
        var postgres = builder.AddPostgres("postgres").AddDatabase("studydb");

        // Add API with only PostgreSQL dependency (skip Redis/Service Bus)
        // Using absolute path resolved from the source directory
        var apiPath = Path.Combine(srcDir, "Api", "Api.csproj");
        var api = builder.AddProject("api", apiPath)
            .WithReference(postgres)
            .WaitFor(postgres)
            .WithHttpHealthCheck("/health")
            .WithExternalHttpEndpoints();

        // Add Admin app without any dependencies or wait conditions
        // The Admin app will connect to API via environment variables set by Aspire
        var adminPath = Path.Combine(srcDir, "Admin");
        builder.AddViteApp("app-admin", adminPath)
            .WithReference(api);

        return builder;
    }

    public static DistributedApplication Build(string[] args)
    {
        return CreateBuilder(args).Build();
    }
}

