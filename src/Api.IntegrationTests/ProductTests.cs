using Api.Features.Products;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

public class ProductTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task ProductWorkflow_CreateAndRetrieve_ExecutesSuccessfully()
    {
        // Arrange
        var httpClient = fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;

        // ===== CHECKPOINT 1: CREATE =====
        var createRequest = new CreateProductRequest("Workflow Test Product", "Comprehensive product description");
        var createResponse = await httpClient.PostAsJsonAsync("/api/products", createRequest, cancellationToken);
        
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<CreateProductResponse>(cancellationToken);
        Assert.NotNull(createdProduct);
        Assert.Equal(createRequest.Name, createdProduct.Name);
        Assert.Equal(createRequest.Description, createdProduct.Description);
        Assert.NotEqual(Guid.Empty, createdProduct.Id);

        var productId = createdProduct.Id;

        // ===== CHECKPOINT 2: GET ALL (verify in list) =====
        var getAllResponse = await httpClient.GetAsync("/api/products", cancellationToken);
        
        getAllResponse.EnsureSuccessStatusCode();
        var allProducts = await getAllResponse.Content.ReadFromJsonAsync<List<GetProductsResponse>>(cancellationToken);
        Assert.NotNull(allProducts);
        Assert.Contains(allProducts, p => p.Id == productId && p.Name == createRequest.Name);
    }
}
