using Api.Features.Products;
using Api.Features.ProductTemplates;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

public class ProductTemplateTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task ProductTemplateWorkflow_CreateWithProductDependency_ExecutesSuccessfully()
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

        // ===== CHECKPOINT 3: GET ALL TEMPLATES (verify in list) =====
        var getAllResponse = await httpClient.GetAsync("/api/product-templates", cancellationToken);
        
        getAllResponse.EnsureSuccessStatusCode();
        var allTemplates = await getAllResponse.Content.ReadFromJsonAsync<List<GetProductTemplatesResponse>>(cancellationToken);
        Assert.NotNull(allTemplates);
        Assert.Contains(allTemplates, t => t.Id == templateId && t.Name == templateRequest.Name && t.ProductId == productId);
    }
}
