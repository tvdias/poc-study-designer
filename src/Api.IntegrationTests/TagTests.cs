extern alias AppHostAssembly;
using Api.Features.Tags;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

[Collection("IntegrationTests")]
public class TagTests(BoxedAppHostFixture fixture)
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
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);
        var createdTag = await createResponse.Content.ReadFromJsonAsync<CreateTagResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(createdTag);
        Assert.Equal(newTag.Name, createdTag.Name);
        Assert.NotEqual(Guid.Empty, createdTag.Id);

        // Act - Get
        var getResponse = await client.GetAsync("/api/tags", TestContext.Current.CancellationToken);
        
        // Assert - Get
        getResponse.EnsureSuccessStatusCode();
        var tags = await getResponse.Content.ReadFromJsonAsync<List<GetTagsResponse>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(tags);
        Assert.Contains(tags, t => t.Id == createdTag.Id && t.Name == newTag.Name);
    }

    [Fact]
    public async Task GetTagById_WorksCorrectly()
    {
        // Arrange
        var appName = "api";
        var client = fixture.App.CreateHttpClient(appName);
        var newTag = new CreateTagRequest("GetById Tag");
        var createResponse = await client.PostAsJsonAsync("/api/tags", newTag, cancellationToken: TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);
        var createdTag = await createResponse.Content.ReadFromJsonAsync<CreateTagResponse>(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var getResponse = await client.GetAsync($"/api/tags/{createdTag!.Id}", TestContext.Current.CancellationToken);

        // Assert
        getResponse.EnsureSuccessStatusCode();
        var fetchedTag = await getResponse.Content.ReadFromJsonAsync<GetTagByIdResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(fetchedTag);
        Assert.Equal(createdTag.Id, fetchedTag.Id);
        Assert.Equal(createdTag.Name, fetchedTag.Name);
    }
}


