extern alias AppHostAssembly;
using Api.Features.MetricGroups;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

[Collection("IntegrationTests")]
public class MetricGroupTests(BoxedAppHostFixture fixture)
{
    [Fact]
    public async Task CreateAndGetMetricGroups_WorksCorrectly()
    {
        // Arrange
        var appName = "api";
        var client = fixture.App.CreateHttpClient(appName);

        // Act - Create
        var newGroup = new CreateMetricGroupRequest("Integration Test Group");
        var createResponse = await client.PostAsJsonAsync("/api/metric-groups", newGroup, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - Create
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);
        var createdGroup = await createResponse.Content.ReadFromJsonAsync<CreateMetricGroupResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(createdGroup);
        Assert.Equal(newGroup.Name, createdGroup.Name);
        Assert.NotEqual(Guid.Empty, createdGroup.Id);

        // Act - Get
        var getResponse = await client.GetAsync("/api/metric-groups", TestContext.Current.CancellationToken);
        
        // Assert - Get
        getResponse.EnsureSuccessStatusCode();
        var groups = await getResponse.Content.ReadFromJsonAsync<List<GetMetricGroupsResponse>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(groups);
        Assert.Contains(groups, g => g.Id == createdGroup.Id && g.Name == newGroup.Name);
    }
}
