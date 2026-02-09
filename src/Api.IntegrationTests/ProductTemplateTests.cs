extern alias AppHostAssembly;
using Api.Features.Products;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

[Collection("IntegrationTests")]
public class ProductTemplateTests(BoxedAppHostFixture fixture)
{
    [Fact]
    public async Task CreateAndGetProductTemplates_WorksCorrectly()
    {
        // Arrange
        var appName = "api";
        var client = fixture.App.CreateHttpClient(appName);

        // 1. Create Product
        var newProduct = new CreateProductRequest("Template Test Product", "Desc");
        var productResponse = await client.PostAsJsonAsync("/api/products", newProduct, cancellationToken: TestContext.Current.CancellationToken);
        productResponse.EnsureSuccessStatusCode();
        var createdProduct = await productResponse.Content.ReadFromJsonAsync<CreateProductResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(createdProduct);

        // Act - Create Template
        var newTemplate = new CreateProductTemplateRequest("Test Template", 1, createdProduct.Id);
        var createResponse = await client.PostAsJsonAsync("/api/product-templates", newTemplate, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - Create
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        var createdTemplate = await createResponse.Content.ReadFromJsonAsync<CreateProductTemplateResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(createdTemplate);
        Assert.Equal(newTemplate.Name, createdTemplate.Name);
        Assert.Equal(newTemplate.ProductId, createdTemplate.ProductId);

        // Act - Get
        var getResponse = await client.GetAsync($"/api/product-templates", TestContext.Current.CancellationToken);
        
        // Assert - Get
        getResponse.EnsureSuccessStatusCode();
        var templates = await getResponse.Content.ReadFromJsonAsync<List<GetProductTemplatesResponse>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(templates);
        Assert.Contains(templates, t => t.Id == createdTemplate.Id && t.Name == newTemplate.Name);
    }
}
