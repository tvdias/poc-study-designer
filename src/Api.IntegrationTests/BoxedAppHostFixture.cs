extern alias AppHostAssembly;
using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace Api.IntegrationTests;

public class BoxedAppHostFixture : IAsyncLifetime
{
    public DistributedApplication App { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
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
    }
}
