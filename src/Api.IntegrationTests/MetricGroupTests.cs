using Api.Features.MetricGroups;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

public class MetricGroupTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task MetricGroupWorkflow_CreateAndRetrieve_ExecutesSuccessfully()
    {
        // Arrange
        var httpClient = fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;

        // ===== CHECKPOINT 1: CREATE =====
        var createRequest = new CreateMetricGroupRequest("Workflow Metric Group");
        var createResponse = await httpClient.PostAsJsonAsync("/api/metric-groups", createRequest, cancellationToken);
        
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        var createdGroup = await createResponse.Content.ReadFromJsonAsync<CreateMetricGroupResponse>(cancellationToken);
        Assert.NotNull(createdGroup);
        Assert.Equal(createRequest.Name, createdGroup.Name);
        Assert.NotEqual(Guid.Empty, createdGroup.Id);
        Assert.True(createdGroup.IsActive);

        var groupId = createdGroup.Id;

        // ===== CHECKPOINT 2: GET ALL (verify in list) =====
        var getAllResponse = await httpClient.GetAsync("/api/metric-groups", cancellationToken);
        
        getAllResponse.EnsureSuccessStatusCode();
        var allGroups = await getAllResponse.Content.ReadFromJsonAsync<List<GetMetricGroupsResponse>>(cancellationToken);
        Assert.NotNull(allGroups);
        Assert.Contains(allGroups, g => g.Id == groupId && g.Name == createRequest.Name);
    }
}
