using Api.Features.Products;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

public class ProductTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task ProductCrudWorkflow_ExecutesSuccessfully()
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

        // ===== CHECKPOINT 2: GET BY ID =====
        var getByIdResponse = await httpClient.GetAsync($"/api/products/{productId}", cancellationToken);
        
        getByIdResponse.EnsureSuccessStatusCode();
        var fetchedProduct = await getByIdResponse.Content.ReadFromJsonAsync<GetProductDetailResponse>(cancellationToken);
        Assert.NotNull(fetchedProduct);
        Assert.Equal(productId, fetchedProduct.Id);
        Assert.Equal(createRequest.Name, fetchedProduct.Name);
        Assert.Equal(createRequest.Description, fetchedProduct.Description);

        // ===== CHECKPOINT 3: GET ALL (verify in list) =====
        var getAllResponse = await httpClient.GetAsync("/api/products", cancellationToken);
        
        getAllResponse.EnsureSuccessStatusCode();
        var allProducts = await getAllResponse.Content.ReadFromJsonAsync<List<GetProductsResponse>>(cancellationToken);
        Assert.NotNull(allProducts);
        Assert.Contains(allProducts, p => p.Id == productId && p.Name == createRequest.Name);

        // ===== CHECKPOINT 4: UPDATE =====
        var updateRequest = new UpdateProductRequest("Workflow Test Product (Updated)", "Updated description", false);
        var updateResponse = await httpClient.PutAsJsonAsync($"/api/products/{productId}", updateRequest, cancellationToken);
        
        updateResponse.EnsureSuccessStatusCode();
        var updatedProduct = await updateResponse.Content.ReadFromJsonAsync<UpdateProductResponse>(cancellationToken);
        Assert.NotNull(updatedProduct);
        Assert.Equal(productId, updatedProduct.Id);
        Assert.Equal("Workflow Test Product (Updated)", updatedProduct.Name);
        Assert.False(updatedProduct.IsActive);

        // ===== CHECKPOINT 5: VERIFY UPDATE (get by id again) =====
        var verifyUpdateResponse = await httpClient.GetAsync($"/api/products/{productId}", cancellationToken);
        
        verifyUpdateResponse.EnsureSuccessStatusCode();
        var verifiedProduct = await verifyUpdateResponse.Content.ReadFromJsonAsync<GetProductDetailResponse>(cancellationToken);
        Assert.NotNull(verifiedProduct);
        Assert.Equal("Workflow Test Product (Updated)", verifiedProduct.Name);
        Assert.False(verifiedProduct.IsActive);

        // ===== CHECKPOINT 6: DELETE =====
        var deleteResponse = await httpClient.DeleteAsync($"/api/products/{productId}", cancellationToken);
        
        deleteResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // ===== CHECKPOINT 7: VERIFY DELETION (should return 404) =====
        var verifyDeleteResponse = await httpClient.GetAsync($"/api/products/{productId}", cancellationToken);
        
        Assert.Equal(System.Net.HttpStatusCode.NotFound, verifyDeleteResponse.StatusCode);
    }
}
