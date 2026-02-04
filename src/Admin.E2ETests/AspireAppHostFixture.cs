extern alias AppHostAssembly;
using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace Admin.E2ETests;

/// <summary>
/// Fixture that starts the entire Aspire application stack for E2E testing.
/// This ensures all services (API, databases, Admin app) are running with correct URLs.
/// </summary>
public class AspireAppHostFixture : IAsyncLifetime
{
    public DistributedApplication App { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<AppHostAssembly::Program>();
        App = await appHost.BuildAsync();
        await App.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await App.DisposeAsync();
    }
}
