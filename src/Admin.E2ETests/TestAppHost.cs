using Aspire.Hosting;
using System.Runtime.CompilerServices;

namespace Admin.E2ETests;

/// <summary>
/// Test-specific AppHost configuration that only starts services required for Admin E2E tests.
/// This is faster than the full AppHost as it skips unnecessary services like Azure Functions,
/// Service Bus, Designer app, and Redis.
/// 
/// This class follows the same pattern as the main AppHost but with minimal services.
/// It can be used with DistributedApplicationTestingBuilder for proper test infrastructure setup.
/// </summary>
public class TestAppHost
{
    /// <summary>
    /// Gets the source directory of this file to properly resolve project paths.
    /// This ensures paths work correctly regardless of where the test assembly runs from.
    /// </summary>
    private static string GetSourceDirectory([CallerFilePath] string path = "")
        => Path.GetDirectoryName(path)!;

    /// <summary>
    /// Entry point for the test AppHost. This is called by DistributedApplicationTestingBuilder.
    /// </summary>
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

    /// <summary>
    /// Program class required for DistributedApplicationTestingBuilder pattern.
    /// This allows the testing infrastructure to properly set up DCP and dashboard paths.
    /// </summary>
    public class Program
    {
        public static IDistributedApplicationBuilder CreateBuilder(string[] args)
            => TestAppHost.CreateBuilder(args);
    }
}

