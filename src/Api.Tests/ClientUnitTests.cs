using Api.Features.Clients;
using Api.Features.Clients.Validators;

namespace Api.Tests;

public class ClientUnitTests
{
    [Fact]
    public void CreateClientRequest_CreatesInstance()
    {
        var name = "Test Client";
        var metadata = "Test Metadata";
        var products = "Product1, Product2";
        var request = new CreateClientRequest(name, metadata, products);
        Assert.Equal(name, request.Name);
        Assert.Equal(metadata, request.IntegrationMetadata);
        Assert.Equal(products, request.ProductsModules);
    }
}

public class CreateClientValidatorTests
{
    private readonly CreateClientValidator _validator = new();

    [Fact]
    public async Task ValidClient_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateClientRequest("Valid Client Name", "metadata", "products");

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
        var request = new CreateClientRequest("", null, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Client name is required.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task NullName_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateClientRequest(null!, null, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Client name is required.");
    }

    [Fact]
    public async Task NameExceeding200Characters_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('a', 201);
        var request = new CreateClientRequest(longName, null, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Client name must not exceed 200 characters.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task NameExactly200Characters_ShouldPassValidation()
    {
        // Arrange
        var maxLengthName = new string('a', 200);
        var request = new CreateClientRequest(maxLengthName, null, null);

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
        var request = new CreateClientRequest("   ", null, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Client name is required.");
    }

    [Fact]
    public async Task IntegrationMetadataExceeding1000Characters_ShouldFailValidation()
    {
        // Arrange
        var longMetadata = new string('a', 1001);
        var request = new CreateClientRequest("Valid Name", longMetadata, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Integration metadata must not exceed 1000 characters.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ProductsModulesExceeding500Characters_ShouldFailValidation()
    {
        // Arrange
        var longProducts = new string('a', 501);
        var request = new CreateClientRequest("Valid Name", null, longProducts);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Products/modules must not exceed 500 characters.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task NullOptionalFields_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateClientRequest("Valid Client Name", null, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}

public class UpdateClientValidatorTests
{
    private readonly UpdateClientValidator _validator = new();

    [Fact]
    public async Task ValidClient_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateClientRequest("Updated Client Name", "metadata", "products", true);

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
        var request = new UpdateClientRequest("", null, null, true);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Client name is required.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task NullName_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateClientRequest(null!, null, null, false);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Client name is required.");
    }

    [Fact]
    public async Task NameExceeding200Characters_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('a', 201);
        var request = new UpdateClientRequest(longName, null, null, true);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Client name must not exceed 200 characters.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task NameExactly200Characters_ShouldPassValidation()
    {
        // Arrange
        var maxLengthName = new string('a', 200);
        var request = new UpdateClientRequest(maxLengthName, null, null, false);

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
        var request = new UpdateClientRequest("   ", null, null, true);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Client name is required.");
    }

    [Fact]
    public async Task IntegrationMetadataExceeding1000Characters_ShouldFailValidation()
    {
        // Arrange
        var longMetadata = new string('a', 1001);
        var request = new UpdateClientRequest("Valid Name", longMetadata, null, true);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Integration metadata must not exceed 1000 characters.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ProductsModulesExceeding500Characters_ShouldFailValidation()
    {
        // Arrange
        var longProducts = new string('a', 501);
        var request = new UpdateClientRequest("Valid Name", null, longProducts, false);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Products/modules must not exceed 500 characters.", result.Errors[0].ErrorMessage);
    }
}
