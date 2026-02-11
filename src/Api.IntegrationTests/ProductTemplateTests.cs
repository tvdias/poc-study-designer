using Api.Features.Products;
using Api.Features.ProductTemplates;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

public class ProductTemplateTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task ProductTemplateCrudWorkflow_ExecutesSuccessfully()
    {
        // Arrange
        var httpClient = fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;

        // ===== CHECKPOINT 1: CREATE PRODUCT (dependency) =====
        var productRequest = new CreateProductRequest("Template Workflow Product", "Product for template testing");
        var productResponse = await httpClient.PostAsJsonAsync("/api/products", productRequest, cancellationToken);
        
        productResponse.EnsureSuccessStatusCode();
        var createdProduct = await productResponse.Content.ReadFromJsonAsync<CreateProductResponse>(cancellationToken);
        Assert.NotNull(createdProduct);
        Assert.NotEqual(Guid.Empty, createdProduct.Id);

        var productId = createdProduct.Id;

        // ===== CHECKPOINT 2: CREATE TEMPLATE =====
        var templateRequest = new CreateProductTemplateRequest("Workflow Template v1", 1, productId);
        var templateResponse = await httpClient.PostAsJsonAsync("/api/product-templates", templateRequest, cancellationToken);
        
        templateResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, templateResponse.StatusCode);
        var createdTemplate = await templateResponse.Content.ReadFromJsonAsync<CreateProductTemplateResponse>(cancellationToken);
        Assert.NotNull(createdTemplate);
        Assert.Equal(templateRequest.Name, createdTemplate.Name);
        Assert.Equal(templateRequest.ProductId, createdTemplate.ProductId);
        Assert.NotEqual(Guid.Empty, createdTemplate.Id);

        var templateId = createdTemplate.Id;

        // ===== CHECKPOINT 3: GET BY ID =====
        var getByIdResponse = await httpClient.GetAsync($"/api/product-templates/{templateId}", cancellationToken);
        
        getByIdResponse.EnsureSuccessStatusCode();
        var fetchedTemplate = await getByIdResponse.Content.ReadFromJsonAsync<GetProductTemplateByIdResponse>(cancellationToken);
        Assert.NotNull(fetchedTemplate);
        Assert.Equal(templateId, fetchedTemplate.Id);
        Assert.Equal(templateRequest.Name, fetchedTemplate.Name);
        Assert.Equal(productId, fetchedTemplate.ProductId);

        // ===== CHECKPOINT 4: GET ALL (verify in list) =====
        var getAllResponse = await httpClient.GetAsync("/api/product-templates", cancellationToken);
        
        getAllResponse.EnsureSuccessStatusCode();
        var allTemplates = await getAllResponse.Content.ReadFromJsonAsync<List<GetProductTemplatesResponse>>(cancellationToken);
        Assert.NotNull(allTemplates);
        Assert.Contains(allTemplates, t => t.Id == templateId && t.Name == templateRequest.Name && t.ProductId == productId);

        // ===== CHECKPOINT 5: UPDATE =====
        var updateRequest = new UpdateProductTemplateRequest("Workflow Template v2", 2, productId, false);
        var updateResponse = await httpClient.PutAsJsonAsync($"/api/product-templates/{templateId}", updateRequest, cancellationToken);
        
        updateResponse.EnsureSuccessStatusCode();
        var updatedTemplate = await updateResponse.Content.ReadFromJsonAsync<UpdateProductTemplateResponse>(cancellationToken);
        Assert.NotNull(updatedTemplate);
        Assert.Equal(templateId, updatedTemplate.Id);
        Assert.Equal("Workflow Template v2", updatedTemplate.Name);
        Assert.Equal(2, updatedTemplate.Version);
        Assert.False(updatedTemplate.IsActive);

        // ===== CHECKPOINT 6: VERIFY UPDATE (get by id again) =====
        var verifyUpdateResponse = await httpClient.GetAsync($"/api/product-templates/{templateId}", cancellationToken);
        
        verifyUpdateResponse.EnsureSuccessStatusCode();
        var verifiedTemplate = await verifyUpdateResponse.Content.ReadFromJsonAsync<GetProductTemplateByIdResponse>(cancellationToken);
        Assert.NotNull(verifiedTemplate);
        Assert.Equal("Workflow Template v2", verifiedTemplate.Name);
        Assert.Equal(2, verifiedTemplate.Version);
        Assert.False(verifiedTemplate.IsActive);

        // ===== CHECKPOINT 7: DELETE =====
        var deleteResponse = await httpClient.DeleteAsync($"/api/product-templates/{templateId}", cancellationToken);
        
        deleteResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // ===== CHECKPOINT 8: VERIFY DELETION (should return 404) =====
        var verifyDeleteResponse = await httpClient.GetAsync($"/api/product-templates/{templateId}", cancellationToken);
        
        Assert.Equal(System.Net.HttpStatusCode.NotFound, verifyDeleteResponse.StatusCode);
    }
}
