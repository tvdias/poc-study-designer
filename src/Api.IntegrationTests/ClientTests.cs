extern alias AppHostAssembly;
using Api.Features.Clients;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

[Collection("IntegrationTests")]
public class ClientTests(BoxedAppHostFixture fixture)
{
    [Fact]
    public async Task CreateAndGetClients_WorksCorrectly()
    {
        // Arrange
        var appName = "api";
        var client = fixture.App.CreateHttpClient(appName);

        // Act - Create
        var newClient = new CreateClientRequest("Integration Test Client", "12345", "CUST-INT-001", "ITC");
        var createResponse = await client.PostAsJsonAsync("/api/clients", newClient, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - Create
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);
        var createdClient = await createResponse.Content.ReadFromJsonAsync<CreateClientResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(createdClient);
        Assert.Equal(newClient.AccountName, createdClient.AccountName);
        Assert.NotEqual(Guid.Empty, createdClient.Id);

        // Act - Get
        var getResponse = await client.GetAsync("/api/clients", TestContext.Current.CancellationToken);
        
        // Assert - Get
        getResponse.EnsureSuccessStatusCode();
        var clients = await getResponse.Content.ReadFromJsonAsync<List<GetClientsResponse>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(clients);
        Assert.Contains(clients, c => c.Id == createdClient.Id && c.AccountName == newClient.AccountName);
    }

    [Fact]
    public async Task GetClientById_WorksCorrectly()
    {
        // Arrange
        var appName = "api";
        var client = fixture.App.CreateHttpClient(appName);
        var newClient = new CreateClientRequest("GetById Client", "67890", "CUST-INT-002", "GBC");
        var createResponse = await client.PostAsJsonAsync("/api/clients", newClient, cancellationToken: TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();
        var createdClient = await createResponse.Content.ReadFromJsonAsync<CreateClientResponse>(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var getResponse = await client.GetAsync($"/api/clients/{createdClient!.Id}", TestContext.Current.CancellationToken);

        // Assert
        getResponse.EnsureSuccessStatusCode();
        var fetchedClient = await getResponse.Content.ReadFromJsonAsync<GetClientsResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(fetchedClient);
        Assert.Equal(createdClient.Id, fetchedClient.Id);
        Assert.Equal(createdClient.AccountName, fetchedClient.AccountName);
    }

    [Fact]
    public async Task UpdateClient_WorksCorrectly()
    {
        // Arrange
        var appName = "api";
        var client = fixture.App.CreateHttpClient(appName);
        var newClient = new CreateClientRequest("Update Client", "11111", "CUST-INT-003", "UPC");
        var createResponse = await client.PostAsJsonAsync("/api/clients", newClient, cancellationToken: TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();
        var createdClient = await createResponse.Content.ReadFromJsonAsync<CreateClientResponse>(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var updateRequest = new UpdateClientRequest("Update Client (Updated)", "22222", "CUST-INT-003", "UPC-UPD", false);
        var updateResponse = await client.PutAsJsonAsync($"/api/clients/{createdClient!.Id}", updateRequest, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        updateResponse.EnsureSuccessStatusCode();
        var updatedClient = await updateResponse.Content.ReadFromJsonAsync<UpdateClientResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(updatedClient);
        Assert.Equal(createdClient.Id, updatedClient.Id);
        Assert.Equal("Update Client (Updated)", updatedClient.AccountName);
        Assert.False(updatedClient.IsActive);
    }

    [Fact]
    public async Task DeleteClient_WorksCorrectly()
    {
        // Arrange
        var appName = "api";
        var client = fixture.App.CreateHttpClient(appName);
        var newClient = new CreateClientRequest("Delete Client", "33333", "CUST-INT-004", "DLC");
        var createResponse = await client.PostAsJsonAsync("/api/clients", newClient, cancellationToken: TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();
        var createdClient = await createResponse.Content.ReadFromJsonAsync<CreateClientResponse>(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var deleteResponse = await client.DeleteAsync($"/api/clients/{createdClient!.Id}", TestContext.Current.CancellationToken);

        // Assert
        deleteResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
}
