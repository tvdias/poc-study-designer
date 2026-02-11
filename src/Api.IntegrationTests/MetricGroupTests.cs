using Api.Features.MetricGroups;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

public class MetricGroupTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task MetricGroupCrudWorkflow_ExecutesSuccessfully()
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

        var groupId = createdGroup.Id;

        // ===== CHECKPOINT 2: GET BY ID =====
        var getByIdResponse = await httpClient.GetAsync($"/api/metric-groups/{groupId}", cancellationToken);
        
        getByIdResponse.EnsureSuccessStatusCode();
        var fetchedGroup = await getByIdResponse.Content.ReadFromJsonAsync<GetMetricGroupByIdResponse>(cancellationToken);
        Assert.NotNull(fetchedGroup);
        Assert.Equal(groupId, fetchedGroup.Id);
        Assert.Equal(createRequest.Name, fetchedGroup.Name);

        // ===== CHECKPOINT 3: GET ALL (verify in list) =====
        var getAllResponse = await httpClient.GetAsync("/api/metric-groups", cancellationToken);
        
        getAllResponse.EnsureSuccessStatusCode();
        var allGroups = await getAllResponse.Content.ReadFromJsonAsync<List<GetMetricGroupsResponse>>(cancellationToken);
        Assert.NotNull(allGroups);
        Assert.Contains(allGroups, g => g.Id == groupId && g.Name == createRequest.Name);

        // ===== CHECKPOINT 4: UPDATE =====
        var updateRequest = new UpdateMetricGroupRequest("Workflow Metric Group (Updated)");
        var updateResponse = await httpClient.PutAsJsonAsync($"/api/metric-groups/{groupId}", updateRequest, cancellationToken);
        
        updateResponse.EnsureSuccessStatusCode();
        var updatedGroup = await updateResponse.Content.ReadFromJsonAsync<UpdateMetricGroupResponse>(cancellationToken);
        Assert.NotNull(updatedGroup);
        Assert.Equal(groupId, updatedGroup.Id);
        Assert.Equal("Workflow Metric Group (Updated)", updatedGroup.Name);

        // ===== CHECKPOINT 5: VERIFY UPDATE (get by id again) =====
        var verifyUpdateResponse = await httpClient.GetAsync($"/api/metric-groups/{groupId}", cancellationToken);
        
        verifyUpdateResponse.EnsureSuccessStatusCode();
        var verifiedGroup = await verifyUpdateResponse.Content.ReadFromJsonAsync<GetMetricGroupByIdResponse>(cancellationToken);
        Assert.NotNull(verifiedGroup);
        Assert.Equal("Workflow Metric Group (Updated)", verifiedGroup.Name);

        // ===== CHECKPOINT 6: DELETE =====
        var deleteResponse = await httpClient.DeleteAsync($"/api/metric-groups/{groupId}", cancellationToken);
        
        deleteResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // ===== CHECKPOINT 7: VERIFY DELETION (should return 404) =====
        var verifyDeleteResponse = await httpClient.GetAsync($"/api/metric-groups/{groupId}", cancellationToken);
        
        Assert.Equal(System.Net.HttpStatusCode.NotFound, verifyDeleteResponse.StatusCode);
    }
}
