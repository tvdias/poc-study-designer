using Api.Features.Tags;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

public class TagTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task TagCrudWorkflow_ExecutesSuccessfully()
    {
        // Arrange
        var httpClient = fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;

        // ===== CHECKPOINT 1: CREATE =====
        var createRequest = new CreateTagRequest("CRUD Workflow Tag");
        var createResponse = await httpClient.PostAsJsonAsync("/api/tags", createRequest, cancellationToken);
        
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        var createdTag = await createResponse.Content.ReadFromJsonAsync<CreateTagResponse>(cancellationToken);
        Assert.NotNull(createdTag);
        Assert.Equal(createRequest.Name, createdTag.Name);
        Assert.NotEqual(Guid.Empty, createdTag.Id);

        var tagId = createdTag.Id;

        // ===== CHECKPOINT 2: GET BY ID =====
        var getByIdResponse = await httpClient.GetAsync($"/api/tags/{tagId}", cancellationToken);
        
        getByIdResponse.EnsureSuccessStatusCode();
        var fetchedTag = await getByIdResponse.Content.ReadFromJsonAsync<GetTagByIdResponse>(cancellationToken);
        Assert.NotNull(fetchedTag);
        Assert.Equal(tagId, fetchedTag.Id);
        Assert.Equal(createRequest.Name, fetchedTag.Name);

        // ===== CHECKPOINT 3: GET ALL (verify in list) =====
        var getAllResponse = await httpClient.GetAsync("/api/tags", cancellationToken);
        
        getAllResponse.EnsureSuccessStatusCode();
        var allTags = await getAllResponse.Content.ReadFromJsonAsync<List<GetTagsResponse>>(cancellationToken);
        Assert.NotNull(allTags);
        Assert.Contains(allTags, t => t.Id == tagId && t.Name == createRequest.Name);

        // ===== CHECKPOINT 4: UPDATE =====
        var updateRequest = new UpdateTagRequest("CRUD Workflow Tag (Updated)");
        var updateResponse = await httpClient.PutAsJsonAsync($"/api/tags/{tagId}", updateRequest, cancellationToken);
        
        updateResponse.EnsureSuccessStatusCode();
        var updatedTag = await updateResponse.Content.ReadFromJsonAsync<UpdateTagResponse>(cancellationToken);
        Assert.NotNull(updatedTag);
        Assert.Equal(tagId, updatedTag.Id);
        Assert.Equal("CRUD Workflow Tag (Updated)", updatedTag.Name);

        // ===== CHECKPOINT 5: VERIFY UPDATE (get by id again) =====
        var verifyUpdateResponse = await httpClient.GetAsync($"/api/tags/{tagId}", cancellationToken);
        
        verifyUpdateResponse.EnsureSuccessStatusCode();
        var verifiedTag = await verifyUpdateResponse.Content.ReadFromJsonAsync<GetTagByIdResponse>(cancellationToken);
        Assert.NotNull(verifiedTag);
        Assert.Equal("CRUD Workflow Tag (Updated)", verifiedTag.Name);

        // ===== CHECKPOINT 6: DELETE =====
        var deleteResponse = await httpClient.DeleteAsync($"/api/tags/{tagId}", cancellationToken);
        
        deleteResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // ===== CHECKPOINT 7: VERIFY DELETION (should return 404) =====
        var verifyDeleteResponse = await httpClient.GetAsync($"/api/tags/{tagId}", cancellationToken);
        
        Assert.Equal(System.Net.HttpStatusCode.NotFound, verifyDeleteResponse.StatusCode);
    }
}


