extern alias AppHostAssembly;
using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace Api.IntegrationTests;

[CollectionDefinition("IntegrationTests")]
public class IntegrationTestCollection : ICollectionFixture<BoxedAppHostFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

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
