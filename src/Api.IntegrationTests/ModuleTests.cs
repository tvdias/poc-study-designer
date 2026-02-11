using Api.Features.Modules;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

public class ModuleTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task ModuleCrudWorkflow_ExecutesSuccessfully()
    {
        // Arrange
        var httpClient = fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;

        // ===== CHECKPOINT 1: CREATE =====
        var createRequest = new CreateModuleRequest("WorkflowModuleVar", "Workflow Module Label", "Comprehensive module description", 1, null, "Detailed instructions");
        var createResponse = await httpClient.PostAsJsonAsync("/api/modules", createRequest, cancellationToken);
        
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        var createdModule = await createResponse.Content.ReadFromJsonAsync<CreateModuleResponse>(cancellationToken);
        Assert.NotNull(createdModule);
        Assert.Equal(createRequest.VariableName, createdModule.VariableName);
        Assert.Equal(createRequest.Label, createdModule.Label);
        Assert.NotEqual(Guid.Empty, createdModule.Id);

        var moduleId = createdModule.Id;

        // ===== CHECKPOINT 2: GET BY ID =====
        var getByIdResponse = await httpClient.GetAsync($"/api/modules/{moduleId}", cancellationToken);
        
        getByIdResponse.EnsureSuccessStatusCode();
        var fetchedModule = await getByIdResponse.Content.ReadFromJsonAsync<GetModuleByIdResponse>(cancellationToken);
        Assert.NotNull(fetchedModule);
        Assert.Equal(moduleId, fetchedModule.Id);
        Assert.Equal(createRequest.VariableName, fetchedModule.VariableName);

        // ===== CHECKPOINT 3: GET ALL (verify in list) =====
        var getAllResponse = await httpClient.GetAsync("/api/modules", cancellationToken);
        
        getAllResponse.EnsureSuccessStatusCode();
        var allModules = await getAllResponse.Content.ReadFromJsonAsync<List<GetModulesResponse>>(cancellationToken);
        Assert.NotNull(allModules);
        Assert.Contains(allModules, m => m.Id == moduleId && m.VariableName == createRequest.VariableName);

        // ===== CHECKPOINT 4: UPDATE =====
        var updateRequest = new UpdateModuleRequest("WorkflowModuleVar (Updated)", "Workflow Module Label (Updated)", "Updated description", 2, null, "Updated instructions", false);
        var updateResponse = await httpClient.PutAsJsonAsync($"/api/modules/{moduleId}", updateRequest, cancellationToken);
        
        updateResponse.EnsureSuccessStatusCode();
        var updatedModule = await updateResponse.Content.ReadFromJsonAsync<UpdateModuleResponse>(cancellationToken);
        Assert.NotNull(updatedModule);
        Assert.Equal(moduleId, updatedModule.Id);
        Assert.Equal("WorkflowModuleVar (Updated)", updatedModule.VariableName);
        Assert.False(updatedModule.IsActive);

        // ===== CHECKPOINT 5: VERIFY UPDATE (get by id again) =====
        var verifyUpdateResponse = await httpClient.GetAsync($"/api/modules/{moduleId}", cancellationToken);
        
        verifyUpdateResponse.EnsureSuccessStatusCode();
        var verifiedModule = await verifyUpdateResponse.Content.ReadFromJsonAsync<GetModuleByIdResponse>(cancellationToken);
        Assert.NotNull(verifiedModule);
        Assert.Equal("WorkflowModuleVar (Updated)", verifiedModule.VariableName);
        Assert.False(verifiedModule.IsActive);

        // ===== CHECKPOINT 6: DELETE =====
        var deleteResponse = await httpClient.DeleteAsync($"/api/modules/{moduleId}", cancellationToken);
        
        deleteResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // ===== CHECKPOINT 7: VERIFY DELETION (should return 404) =====
        var verifyDeleteResponse = await httpClient.GetAsync($"/api/modules/{moduleId}", cancellationToken);
        
        Assert.Equal(System.Net.HttpStatusCode.NotFound, verifyDeleteResponse.StatusCode);
    }
}
