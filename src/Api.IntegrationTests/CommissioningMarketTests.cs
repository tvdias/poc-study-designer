using Api.Features.CommissioningMarkets;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

public class CommissioningMarketTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task CommissioningMarketCrudWorkflow_ExecutesSuccessfully()
    {
        // Arrange
        var httpClient = fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;

        // ===== CHECKPOINT 1: CREATE =====
        var createRequest = new CreateCommissioningMarketRequest("US-TEST", "Workflow Market");
        var createResponse = await httpClient.PostAsJsonAsync("/api/commissioning-markets", createRequest, cancellationToken);
        
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        var createdMarket = await createResponse.Content.ReadFromJsonAsync<CreateCommissioningMarketResponse>(cancellationToken);
        Assert.NotNull(createdMarket);
        Assert.Equal(createRequest.IsoCode, createdMarket.IsoCode);
        Assert.Equal(createRequest.Name, createdMarket.Name);
        Assert.NotEqual(Guid.Empty, createdMarket.Id);

        var marketId = createdMarket.Id;

        // ===== CHECKPOINT 2: GET BY ID =====
        var getByIdResponse = await httpClient.GetAsync($"/api/commissioning-markets/{marketId}", cancellationToken);
        
        getByIdResponse.EnsureSuccessStatusCode();
        var fetchedMarket = await getByIdResponse.Content.ReadFromJsonAsync<GetCommissioningMarketByIdResponse>(cancellationToken);
        Assert.NotNull(fetchedMarket);
        Assert.Equal(marketId, fetchedMarket.Id);
        Assert.Equal(createRequest.IsoCode, fetchedMarket.IsoCode);
        Assert.Equal(createRequest.Name, fetchedMarket.Name);

        // ===== CHECKPOINT 3: GET ALL (verify in list) =====
        var getAllResponse = await httpClient.GetAsync("/api/commissioning-markets", cancellationToken);
        
        getAllResponse.EnsureSuccessStatusCode();
        var allMarkets = await getAllResponse.Content.ReadFromJsonAsync<List<GetCommissioningMarketsResponse>>(cancellationToken);
        Assert.NotNull(allMarkets);
        Assert.Contains(allMarkets, m => m.Id == marketId && m.IsoCode == createRequest.IsoCode && m.Name == createRequest.Name);

        // ===== CHECKPOINT 4: UPDATE =====
        var updateRequest = new UpdateCommissioningMarketRequest("US-TEST", "Workflow Market (Updated)");
        var updateResponse = await httpClient.PutAsJsonAsync($"/api/commissioning-markets/{marketId}", updateRequest, cancellationToken);
        
        updateResponse.EnsureSuccessStatusCode();
        var updatedMarket = await updateResponse.Content.ReadFromJsonAsync<UpdateCommissioningMarketResponse>(cancellationToken);
        Assert.NotNull(updatedMarket);
        Assert.Equal(marketId, updatedMarket.Id);
        Assert.Equal("Workflow Market (Updated)", updatedMarket.Name);

        // ===== CHECKPOINT 5: VERIFY UPDATE (get by id again) =====
        var verifyUpdateResponse = await httpClient.GetAsync($"/api/commissioning-markets/{marketId}", cancellationToken);
        
        verifyUpdateResponse.EnsureSuccessStatusCode();
        var verifiedMarket = await verifyUpdateResponse.Content.ReadFromJsonAsync<GetCommissioningMarketByIdResponse>(cancellationToken);
        Assert.NotNull(verifiedMarket);
        Assert.Equal("Workflow Market (Updated)", verifiedMarket.Name);

        // ===== CHECKPOINT 6: SEARCH =====
        var searchResponse = await httpClient.GetAsync("/api/commissioning-markets?query=Workflow", cancellationToken);
        
        searchResponse.EnsureSuccessStatusCode();
        var searchedMarkets = await searchResponse.Content.ReadFromJsonAsync<List<GetCommissioningMarketsResponse>>(cancellationToken);
        Assert.NotNull(searchedMarkets);
        Assert.Contains(searchedMarkets, m => m.Id == marketId);

        // ===== CHECKPOINT 7: DELETE =====
        var deleteResponse = await httpClient.DeleteAsync($"/api/commissioning-markets/{marketId}", cancellationToken);
        
        deleteResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // ===== CHECKPOINT 8: VERIFY DELETION (should return 404) =====
        var verifyDeleteResponse = await httpClient.GetAsync($"/api/commissioning-markets/{marketId}", cancellationToken);
        
        Assert.Equal(System.Net.HttpStatusCode.NotFound, verifyDeleteResponse.StatusCode);
    }
}
