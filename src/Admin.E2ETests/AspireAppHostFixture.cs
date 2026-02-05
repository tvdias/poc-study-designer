extern alias AppHostAssembly;
using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace Admin.E2ETests;

/// <summary>
/// Fixture that starts the main AppHost in AdminE2E test mode.
/// The AppHost checks ASPIRE_TEST_MODE environment variable and skips unnecessary services
/// (Redis, Service Bus, Designer app, Azure Functions) when set to "AdminE2E".
/// Only starts: PostgreSQL, API, and Admin app.
/// This is shared across all tests in the collection for faster execution.
/// </summary>
public class AspireAppHostFixture : IAsyncLifetime
{
    public DistributedApplication App { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        // Set environment variable to signal AppHost to run in AdminE2E test mode
        Environment.SetEnvironmentVariable("ASPIRE_TEST_MODE", "AdminE2E");

        // Use the main AppHost with AdminE2E mode which skips unnecessary services
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<AppHostAssembly::Program>();
        App = await appHost.BuildAsync();
        await App.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (App != null)
        {
            await App.DisposeAsync();
        }
        
        // Clean up environment variable
        Environment.SetEnvironmentVariable("ASPIRE_TEST_MODE", null);
    }
}
