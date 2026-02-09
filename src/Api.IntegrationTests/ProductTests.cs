extern alias AppHostAssembly;
using Api.Features.Products;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

[Collection("IntegrationTests")]
public class ProductTests(BoxedAppHostFixture fixture)
{
    [Fact]
    public async Task CreateAndGetProducts_WorksCorrectly()
    {
        // Arrange
        var appName = "api";
        var client = fixture.App.CreateHttpClient(appName);

        // Act - Create
        var newProduct = new CreateProductRequest("Integration Test Product", "Description for product");
        var createResponse = await client.PostAsJsonAsync("/api/products", newProduct, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - Create
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<CreateProductResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(createdProduct);
        Assert.Equal(newProduct.Name, createdProduct.Name);
        Assert.NotEqual(Guid.Empty, createdProduct.Id);

        // Act - Get
        var getResponse = await client.GetAsync("/api/products", TestContext.Current.CancellationToken);
        
        // Assert - Get
        getResponse.EnsureSuccessStatusCode();
        var products = await getResponse.Content.ReadFromJsonAsync<List<GetProductsResponse>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(products);
        Assert.Contains(products, p => p.Id == createdProduct.Id && p.Name == newProduct.Name);
    }
}
