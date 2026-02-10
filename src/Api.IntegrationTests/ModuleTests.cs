using Api.Features.Modules;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

public class ModuleTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task ModuleWorkflow_CreateAndRetrieve_ExecutesSuccessfully()
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

        // ===== CHECKPOINT 2: GET ALL (verify in list) =====
        var getAllResponse = await httpClient.GetAsync("/api/modules", cancellationToken);
        
        getAllResponse.EnsureSuccessStatusCode();
        var allModules = await getAllResponse.Content.ReadFromJsonAsync<List<GetModulesResponse>>(cancellationToken);
        Assert.NotNull(allModules);
        Assert.Contains(allModules, m => m.Id == moduleId && m.VariableName == createRequest.VariableName);
    }
}
