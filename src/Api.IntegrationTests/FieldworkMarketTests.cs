extern alias AppHostAssembly;
using Api.Features.FieldworkMarkets;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

public class FieldworkMarketTests(BoxedAppHostFixture fixture) : IClassFixture<BoxedAppHostFixture>
{
    [Fact]
    public async Task CreateAndGetFieldworkMarkets_WorksCorrectly()
    {
        // Arrange
        var appName = "api";
        var client = fixture.App.CreateHttpClient(appName);

        // Act - Create
        var newMarket = new CreateFieldworkMarketRequest("GB", "United Kingdom");
        var createResponse = await client.PostAsJsonAsync("/api/fieldwork-markets", newMarket, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - Create
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);
        var createdMarket = await createResponse.Content.ReadFromJsonAsync<CreateFieldworkMarketResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(createdMarket);
        Assert.Equal(newMarket.IsoCode, createdMarket.IsoCode);
        Assert.Equal(newMarket.Name, createdMarket.Name);
        Assert.NotEqual(Guid.Empty, createdMarket.Id);

        // Act - Get
        var getResponse = await client.GetAsync("/api/fieldwork-markets", TestContext.Current.CancellationToken);
        
        // Assert - Get
        getResponse.EnsureSuccessStatusCode();
        var markets = await getResponse.Content.ReadFromJsonAsync<List<GetFieldworkMarketsResponse>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(markets);
        Assert.Contains(markets, m => m.Id == createdMarket.Id && m.IsoCode == newMarket.IsoCode && m.Name == newMarket.Name);
    }

    [Fact]
    public async Task GetFieldworkMarketById_WorksCorrectly()
    {
        // Arrange
        var appName = "api";
        var client = fixture.App.CreateHttpClient(appName);
        var newMarket = new CreateFieldworkMarketRequest("AU", "Australia");
        var createResponse = await client.PostAsJsonAsync("/api/fieldwork-markets", newMarket, cancellationToken: TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);
        var createdMarket = await createResponse.Content.ReadFromJsonAsync<CreateFieldworkMarketResponse>(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var getResponse = await client.GetAsync($"/api/fieldwork-markets/{createdMarket!.Id}", TestContext.Current.CancellationToken);

        // Assert
        getResponse.EnsureSuccessStatusCode();
        var fetchedMarket = await getResponse.Content.ReadFromJsonAsync<GetFieldworkMarketByIdResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(fetchedMarket);
        Assert.Equal(createdMarket.Id, fetchedMarket.Id);
        Assert.Equal(createdMarket.IsoCode, fetchedMarket.IsoCode);
        Assert.Equal(createdMarket.Name, fetchedMarket.Name);
    }

    [Fact]
    public async Task UpdateFieldworkMarket_WorksCorrectly()
    {
        // Arrange
        var appName = "api";
        var client = fixture.App.CreateHttpClient(appName);
        var newMarket = new CreateFieldworkMarketRequest("NZ", "New Zealand");
        var createResponse = await client.PostAsJsonAsync("/api/fieldwork-markets", newMarket, cancellationToken: TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();
        var createdMarket = await createResponse.Content.ReadFromJsonAsync<CreateFieldworkMarketResponse>(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var updateRequest = new UpdateFieldworkMarketRequest("NZ", "New Zealand (Updated)", false);
        var updateResponse = await client.PutAsJsonAsync($"/api/fieldwork-markets/{createdMarket!.Id}", updateRequest, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        updateResponse.EnsureSuccessStatusCode();
        var updatedMarket = await updateResponse.Content.ReadFromJsonAsync<UpdateFieldworkMarketResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(updatedMarket);
        Assert.Equal(createdMarket.Id, updatedMarket.Id);
        Assert.Equal("New Zealand (Updated)", updatedMarket.Name);
        Assert.False(updatedMarket.IsActive);
    }

    [Fact]
    public async Task DeleteFieldworkMarket_WorksCorrectly()
    {
        // Arrange
        var appName = "api";
        var client = fixture.App.CreateHttpClient(appName);
        var newMarket = new CreateFieldworkMarketRequest("JP", "Japan");
        var createResponse = await client.PostAsJsonAsync("/api/fieldwork-markets", newMarket, cancellationToken: TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();
        var createdMarket = await createResponse.Content.ReadFromJsonAsync<CreateFieldworkMarketResponse>(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var deleteResponse = await client.DeleteAsync($"/api/fieldwork-markets/{createdMarket!.Id}", TestContext.Current.CancellationToken);

        // Assert
        deleteResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task SearchFieldworkMarkets_WorksCorrectly()
    {
        // Arrange
        var appName = "api";
        var client = fixture.App.CreateHttpClient(appName);
        var market1 = new CreateFieldworkMarketRequest("IT", "Italy");
        var market2 = new CreateFieldworkMarketRequest("ES", "Spain");
        await client.PostAsJsonAsync("/api/fieldwork-markets", market1, cancellationToken: TestContext.Current.CancellationToken);
        await client.PostAsJsonAsync("/api/fieldwork-markets", market2, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var searchResponse = await client.GetAsync("/api/fieldwork-markets?query=Italy", TestContext.Current.CancellationToken);

        // Assert
        searchResponse.EnsureSuccessStatusCode();
        var markets = await searchResponse.Content.ReadFromJsonAsync<List<GetFieldworkMarketsResponse>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(markets);
        Assert.Contains(markets, m => m.Name == "Italy");
    }
}
