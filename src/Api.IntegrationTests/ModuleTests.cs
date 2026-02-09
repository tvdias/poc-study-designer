extern alias AppHostAssembly;
using Api.Features.Modules;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

[Collection("IntegrationTests")]
public class ModuleTests(BoxedAppHostFixture fixture)
{
    [Fact]
    public async Task CreateAndGetModules_WorksCorrectly()
    {
        // Arrange
        var appName = "api";
        var client = fixture.App.CreateHttpClient(appName);

        // Act - Create
        var newModule = new CreateModuleRequest("ModuleVar", "Module Label", "Test Description", 1, null, "Test Instructions");
        var createResponse = await client.PostAsJsonAsync("/api/modules", newModule, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - Create
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);
        var createdModule = await createResponse.Content.ReadFromJsonAsync<CreateModuleResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(createdModule);
        Assert.Equal(newModule.VariableName, createdModule.VariableName);
        Assert.NotEqual(Guid.Empty, createdModule.Id);

        // Act - Get
        var getResponse = await client.GetAsync("/api/modules", TestContext.Current.CancellationToken);
        
        // Assert - Get
        getResponse.EnsureSuccessStatusCode();
        var modules = await getResponse.Content.ReadFromJsonAsync<List<GetModulesResponse>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(modules);
        Assert.Contains(modules, m => m.Id == createdModule.Id && m.VariableName == newModule.VariableName);
    }
}
