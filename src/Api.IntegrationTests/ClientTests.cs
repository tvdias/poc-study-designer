using Api.Features.Clients;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

public class ClientTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task ClientCrudWorkflow_ExecutesSuccessfully()
    {
        // Arrange
        var httpClient = fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;

        // ===== CHECKPOINT 1: CREATE =====
        var createRequest = new CreateClientRequest("CRUD Workflow Client", "12345", "CUST-CRUD-001", "CWC");
        var createResponse = await httpClient.PostAsJsonAsync("/api/clients", createRequest, cancellationToken);
        
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        var createdClient = await createResponse.Content.ReadFromJsonAsync<CreateClientResponse>(cancellationToken);
        Assert.NotNull(createdClient);
        Assert.Equal(createRequest.AccountName, createdClient.AccountName);
        Assert.Equal(createRequest.CustomerNumber, createdClient.CustomerNumber);
        Assert.NotEqual(Guid.Empty, createdClient.Id);
        Assert.True(createdClient.IsActive);

        var clientId = createdClient.Id;

        // ===== CHECKPOINT 2: GET BY ID =====
        var getByIdResponse = await httpClient.GetAsync($"/api/clients/{clientId}", cancellationToken);
        
        getByIdResponse.EnsureSuccessStatusCode();
        var fetchedClient = await getByIdResponse.Content.ReadFromJsonAsync<GetClientsResponse>(cancellationToken);
        Assert.NotNull(fetchedClient);
        Assert.Equal(clientId, fetchedClient.Id);
        Assert.Equal(createRequest.AccountName, fetchedClient.AccountName);
        Assert.True(fetchedClient.IsActive);

        // ===== CHECKPOINT 3: GET ALL (verify in list) =====
        var getAllResponse = await httpClient.GetAsync("/api/clients", cancellationToken);
        
        getAllResponse.EnsureSuccessStatusCode();
        var allClients = await getAllResponse.Content.ReadFromJsonAsync<List<GetClientsResponse>>(cancellationToken);
        Assert.NotNull(allClients);
        Assert.Contains(allClients, c => c.Id == clientId && c.AccountName == createRequest.AccountName);

        // ===== CHECKPOINT 4: UPDATE =====
        var updateRequest = new UpdateClientRequest("CRUD Workflow Client (Updated)", "99999", "CUST-CRUD-001-UPD", "CWC-UPD", false);
        var updateResponse = await httpClient.PutAsJsonAsync($"/api/clients/{clientId}", updateRequest, cancellationToken);
        
        updateResponse.EnsureSuccessStatusCode();
        var updatedClient = await updateResponse.Content.ReadFromJsonAsync<UpdateClientResponse>(cancellationToken);
        Assert.NotNull(updatedClient);
        Assert.Equal(clientId, updatedClient.Id);
        Assert.Equal("CRUD Workflow Client (Updated)", updatedClient.AccountName);
        Assert.Equal("99999", updatedClient.CompanyNumber);
        Assert.False(updatedClient.IsActive);

        // ===== CHECKPOINT 5: VERIFY UPDATE (get by id again) =====
        var verifyUpdateResponse = await httpClient.GetAsync($"/api/clients/{clientId}", cancellationToken);
        
        verifyUpdateResponse.EnsureSuccessStatusCode();
        var verifiedClient = await verifyUpdateResponse.Content.ReadFromJsonAsync<GetClientsResponse>(cancellationToken);
        Assert.NotNull(verifiedClient);
        Assert.Equal("CRUD Workflow Client (Updated)", verifiedClient.AccountName);
        Assert.False(verifiedClient.IsActive);

        // ===== CHECKPOINT 6: DELETE =====
        var deleteResponse = await httpClient.DeleteAsync($"/api/clients/{clientId}", cancellationToken);
        
        deleteResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // ===== CHECKPOINT 7: VERIFY DELETION (should return 404) =====
        var verifyDeleteResponse = await httpClient.GetAsync($"/api/clients/{clientId}", cancellationToken);
        
        Assert.Equal(System.Net.HttpStatusCode.NotFound, verifyDeleteResponse.StatusCode);
    }
}
