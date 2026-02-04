using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace Admin.E2ETests;

/// <summary>
/// Fixture that starts only the services needed for Admin E2E testing.
/// Uses optimized TestAppHost configuration that starts: PostgreSQL, API, and Admin app.
/// This is shared across all tests in the collection for faster execution.
/// </summary>
public class AspireAppHostFixture : IAsyncLifetime
{
    public DistributedApplication App { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        // Create the test-specific AppHost builder
        var builder = TestAppHost.CreateBuilder(Array.Empty<string>());
        
        // Build and start the application
        App = builder.Build();
        await App.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (App != null)
        {
            await App.DisposeAsync();
        }
    }
}
