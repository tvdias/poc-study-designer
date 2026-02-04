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
        // Use the test-specific AppHost that builds and returns ready application
        App = TestAppHost.Build(Array.Empty<string>());
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
