using Api.Features.Products;
using Api.Features.ProductTemplates.Validators;

namespace Api.Tests;

public class ProductTemplateUnitTests
{
    [Fact]
    public void CreateProductTemplateRequest_CreatesInstance()
    {
        var name = "Test Template";
        var version = 1;
        var productId = Guid.NewGuid();
        var request = new CreateProductTemplateRequest(name, version, productId);
        Assert.Equal(name, request.Name);
        Assert.Equal(version, request.Version);
        Assert.Equal(productId, request.ProductId);
    }
}

public class CreateProductTemplateValidatorTests
{
    private readonly CreateProductTemplateValidator _validator = new();

    [Fact]
    public async Task ValidProductTemplate_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateProductTemplateRequest("Valid Template", 1, Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task EmptyName_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProductTemplateRequest("", 1, Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Product template name is required");
    }

    [Fact]
    public async Task NullName_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProductTemplateRequest(null!, 1, Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Product template name is required");
    }

    [Fact]
    public async Task NameExceeding200Characters_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('a', 201);
        var request = new CreateProductTemplateRequest(longName, 1, Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Product template name cannot exceed 200 characters");
    }

    [Fact]
    public async Task NameExactly200Characters_ShouldPassValidation()
    {
        // Arrange
        var maxLengthName = new string('a', 200);
        var request = new CreateProductTemplateRequest(maxLengthName, 1, Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task VersionZero_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProductTemplateRequest("Valid Name", 0, Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Version must be greater than 0");
    }

    [Fact]
    public async Task NegativeVersion_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProductTemplateRequest("Valid Name", -1, Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Version must be greater than 0");
    }

    [Fact]
    public async Task EmptyProductId_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProductTemplateRequest("Valid Name", 1, Guid.Empty);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Product ID is required");
    }
}

public class UpdateProductTemplateValidatorTests
{
    private readonly UpdateProductTemplateValidator _validator = new();

    [Fact]
    public async Task ValidProductTemplate_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateProductTemplateRequest("Updated Template", 2, Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task EmptyName_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateProductTemplateRequest("", 1, Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Product template name is required");
    }

    [Fact]
    public async Task NullName_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateProductTemplateRequest(null!, 1, Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Product template name is required");
    }

    [Fact]
    public async Task NameExceeding200Characters_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('a', 201);
        var request = new UpdateProductTemplateRequest(longName, 1, Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Product template name cannot exceed 200 characters");
    }

    [Fact]
    public async Task VersionZero_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateProductTemplateRequest("Valid Name", 0, Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Version must be greater than 0");
    }

    [Fact]
    public async Task EmptyProductId_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateProductTemplateRequest("Valid Name", 1, Guid.Empty);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Product ID is required");
    }
}
