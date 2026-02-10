using Api.Features.CommissioningMarkets;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

public class CommissioningMarketTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task CreateAndGetCommissioningMarkets_WorksCorrectly()
    {
        // Arrange
        var client = fixture.HttpClient;

        // Act - Create
        var newMarket = new CreateCommissioningMarketRequest("US", "United States");
        var createResponse = await client.PostAsJsonAsync("/api/commissioning-markets", newMarket, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - Create
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);
        var createdMarket = await createResponse.Content.ReadFromJsonAsync<CreateCommissioningMarketResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(createdMarket);
        Assert.Equal(newMarket.IsoCode, createdMarket.IsoCode);
        Assert.Equal(newMarket.Name, createdMarket.Name);
        Assert.NotEqual(Guid.Empty, createdMarket.Id);

        // Act - Get
        var getResponse = await client.GetAsync("/api/commissioning-markets", TestContext.Current.CancellationToken);
        
        // Assert - Get
        getResponse.EnsureSuccessStatusCode();
        var markets = await getResponse.Content.ReadFromJsonAsync<List<GetCommissioningMarketsResponse>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(markets);
        Assert.Contains(markets, m => m.Id == createdMarket.Id && m.IsoCode == newMarket.IsoCode && m.Name == newMarket.Name);
    }

    [Fact]
    public async Task GetCommissioningMarketById_WorksCorrectly()
    {
        // Arrange
        var client = fixture.HttpClient;
        var newMarket = new CreateCommissioningMarketRequest("CA", "Canada");
        var createResponse = await client.PostAsJsonAsync("/api/commissioning-markets", newMarket, cancellationToken: TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);
        var createdMarket = await createResponse.Content.ReadFromJsonAsync<CreateCommissioningMarketResponse>(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var getResponse = await client.GetAsync($"/api/commissioning-markets/{createdMarket!.Id}", TestContext.Current.CancellationToken);

        // Assert
        getResponse.EnsureSuccessStatusCode();
        var fetchedMarket = await getResponse.Content.ReadFromJsonAsync<GetCommissioningMarketByIdResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(fetchedMarket);
        Assert.Equal(createdMarket.Id, fetchedMarket.Id);
        Assert.Equal(createdMarket.IsoCode, fetchedMarket.IsoCode);
        Assert.Equal(createdMarket.Name, fetchedMarket.Name);
    }

    [Fact]
    public async Task UpdateCommissioningMarket_WorksCorrectly()
    {
        // Arrange
        var client = fixture.HttpClient;
        var newMarket = new CreateCommissioningMarketRequest("MX", "Mexico");
        var createResponse = await client.PostAsJsonAsync("/api/commissioning-markets", newMarket, cancellationToken: TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();
        var createdMarket = await createResponse.Content.ReadFromJsonAsync<CreateCommissioningMarketResponse>(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var updateRequest = new UpdateCommissioningMarketRequest("MX", "Mexico (Updated)", false);
        var updateResponse = await client.PutAsJsonAsync($"/api/commissioning-markets/{createdMarket!.Id}", updateRequest, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        updateResponse.EnsureSuccessStatusCode();
        var updatedMarket = await updateResponse.Content.ReadFromJsonAsync<UpdateCommissioningMarketResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(updatedMarket);
        Assert.Equal(createdMarket.Id, updatedMarket.Id);
        Assert.Equal("Mexico (Updated)", updatedMarket.Name);
        Assert.False(updatedMarket.IsActive);
    }

    [Fact]
    public async Task DeleteCommissioningMarket_WorksCorrectly()
    {
        // Arrange
        var client = fixture.HttpClient;
        var newMarket = new CreateCommissioningMarketRequest("BR", "Brazil");
        var createResponse = await client.PostAsJsonAsync("/api/commissioning-markets", newMarket, cancellationToken: TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();
        var createdMarket = await createResponse.Content.ReadFromJsonAsync<CreateCommissioningMarketResponse>(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var deleteResponse = await client.DeleteAsync($"/api/commissioning-markets/{createdMarket!.Id}", TestContext.Current.CancellationToken);

        // Assert
        deleteResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task SearchCommissioningMarkets_WorksCorrectly()
    {
        // Arrange
        var client = fixture.HttpClient;
        var market1 = new CreateCommissioningMarketRequest("FR", "France");
        var market2 = new CreateCommissioningMarketRequest("DE", "Germany");
        await client.PostAsJsonAsync("/api/commissioning-markets", market1, cancellationToken: TestContext.Current.CancellationToken);
        await client.PostAsJsonAsync("/api/commissioning-markets", market2, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var searchResponse = await client.GetAsync("/api/commissioning-markets?query=France", TestContext.Current.CancellationToken);

        // Assert
        searchResponse.EnsureSuccessStatusCode();
        var markets = await searchResponse.Content.ReadFromJsonAsync<List<GetCommissioningMarketsResponse>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(markets);
        Assert.Contains(markets, m => m.Name == "France");
    }
}
