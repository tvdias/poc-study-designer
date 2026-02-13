using Api.Features.Products;
using Api.Features.Products.Validators;

namespace Api.Tests;

public class ProductUnitTests
{
    [Fact]
    public void CreateProductRequest_CreatesInstance()
    {
        var name = "Test Product";
        var description = "Test Description";
        var request = new CreateProductRequest(name, description);
        Assert.Equal(name, request.Name);
        Assert.Equal(description, request.Description);
    }
}

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator = new();

    [Fact]
    public async Task ValidProduct_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateProductRequest("Valid Product", "Valid Description");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidProductWithoutDescription_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateProductRequest("Valid Product", null);

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
        var request = new CreateProductRequest("", null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Product name is required");
    }

    [Fact]
    public async Task NullName_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProductRequest(null!, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Product name is required");
    }

    [Fact]
    public async Task NameExceeding200Characters_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('a', 201);
        var request = new CreateProductRequest(longName, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Product name cannot exceed 200 characters");
    }

    [Fact]
    public async Task NameExactly200Characters_ShouldPassValidation()
    {
        // Arrange
        var maxLengthName = new string('a', 200);
        var request = new CreateProductRequest(maxLengthName, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task DescriptionExceeding2000Characters_ShouldFailValidation()
    {
        // Arrange
        var longDescription = new string('a', 2001);
        var request = new CreateProductRequest("Valid Name", longDescription);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Product description cannot exceed 2000 characters");
    }

    [Fact]
    public async Task DescriptionExactly2000Characters_ShouldPassValidation()
    {
        // Arrange
        var maxLengthDescription = new string('a', 2000);
        var request = new CreateProductRequest("Valid Name", maxLengthDescription);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task WhitespaceName_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProductRequest("   ", null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Product name is required");
    }
}

public class UpdateProductValidatorTests
{
    private readonly UpdateProductValidator _validator = new();

    [Fact]
    public async Task ValidProduct_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateProductRequest("Updated Product", "Updated Description");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidProductWithoutDescription_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateProductRequest("Valid Product", null);

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
        var request = new UpdateProductRequest("", null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Product name is required");
    }

    [Fact]
    public async Task NullName_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateProductRequest(null!, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Product name is required");
    }

    [Fact]
    public async Task NameExceeding200Characters_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('a', 201);
        var request = new UpdateProductRequest(longName, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Product name cannot exceed 200 characters");
    }

    [Fact]
    public async Task DescriptionExceeding2000Characters_ShouldFailValidation()
    {
        // Arrange
        var longDescription = new string('a', 2001);
        var request = new UpdateProductRequest("Valid Name", longDescription);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Product description cannot exceed 2000 characters");
    }

    [Fact]
    public async Task DescriptionExactly2000Characters_ShouldPassValidation()
    {
        // Arrange
        var maxLengthDescription = new string('a', 2000);
        var request = new UpdateProductRequest("Valid Name", maxLengthDescription);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
