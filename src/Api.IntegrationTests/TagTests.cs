extern alias AppHostAssembly;
using Api.Features.Tags;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

public class TagTests(BoxedAppHostFixture fixture) : IClassFixture<BoxedAppHostFixture>
{
    [Fact]
    public async Task CreateAndGetTags_WorksCorrectly()
    {
        // Arrange
        var appName = "api";
        var client = fixture.App.CreateHttpClient(appName);

        // Act - Create
        var newTag = new CreateTagRequest("Integration Test Tag");
        var createResponse = await client.PostAsJsonAsync("/api/tags", newTag, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - Create
        createResponse.EnsureSuccessStatusCode();
        var createdTag = await createResponse.Content.ReadFromJsonAsync<CreateTagResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(createdTag);
        Assert.Equal(newTag.Name, createdTag.Name);
        Assert.NotEqual(Guid.Empty, createdTag.Id);

        // Act - Get
        var getResponse = await client.GetAsync("/api/tags");
        
        // Assert - Get
        getResponse.EnsureSuccessStatusCode();
        var tags = await getResponse.Content.ReadFromJsonAsync<List<GetTagsResponse>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(tags);
        Assert.Contains(tags, t => t.Id == createdTag.Id && t.Name == newTag.Name);
    }
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
