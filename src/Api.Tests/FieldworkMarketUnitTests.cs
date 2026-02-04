using Api.Features.FieldworkMarkets;
using Api.Features.FieldworkMarkets.Validators;

namespace Api.Tests;

public class FieldworkMarketUnitTests
{
    [Fact]
    public void CreateFieldworkMarketRequest_CreatesInstance()
    {
        var isoCode = "US";
        var name = "United States";
        var request = new CreateFieldworkMarketRequest(isoCode, name);
        Assert.Equal(isoCode, request.IsoCode);
        Assert.Equal(name, request.Name);
    }
}

public class CreateFieldworkMarketValidatorTests
{
    private readonly CreateFieldworkMarketValidator _validator = new();

    [Fact]
    public async Task ValidMarket_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateFieldworkMarketRequest("US", "United States");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task EmptyIsoCode_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateFieldworkMarketRequest("", "United States");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "ISO code is required.");
    }

    [Fact]
    public async Task NullIsoCode_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateFieldworkMarketRequest(null!, "United States");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "ISO code is required.");
    }

    [Fact]
    public async Task IsoCodeExceeding10Characters_ShouldFailValidation()
    {
        // Arrange
        var longIsoCode = new string('A', 11);
        var request = new CreateFieldworkMarketRequest(longIsoCode, "Test Market");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "ISO code must not exceed 10 characters.");
    }

    [Fact]
    public async Task IsoCodeExactly10Characters_ShouldPassValidation()
    {
        // Arrange
        var maxLengthIsoCode = new string('A', 10);
        var request = new CreateFieldworkMarketRequest(maxLengthIsoCode, "Test Market");

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
        var request = new CreateFieldworkMarketRequest("US", "");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Market name is required.");
    }

    [Fact]
    public async Task NullName_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateFieldworkMarketRequest("US", null!);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Market name is required.");
    }

    [Fact]
    public async Task NameExceeding100Characters_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('a', 101);
        var request = new CreateFieldworkMarketRequest("US", longName);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Market name must not exceed 100 characters.");
    }

    [Fact]
    public async Task NameExactly100Characters_ShouldPassValidation()
    {
        // Arrange
        var maxLengthName = new string('a', 100);
        var request = new CreateFieldworkMarketRequest("US", maxLengthName);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}

public class UpdateFieldworkMarketValidatorTests
{
    private readonly UpdateFieldworkMarketValidator _validator = new();

    [Fact]
    public async Task ValidMarket_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateFieldworkMarketRequest("US", "United States", true);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task EmptyIsoCode_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateFieldworkMarketRequest("", "United States", true);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "ISO code is required.");
    }

    [Fact]
    public async Task EmptyName_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateFieldworkMarketRequest("US", "", false);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Market name is required.");
    }

    [Fact]
    public async Task NameExceeding100Characters_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('a', 101);
        var request = new UpdateFieldworkMarketRequest("US", longName, true);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Market name must not exceed 100 characters.");
    }

    [Fact]
    public async Task IsoCodeExceeding10Characters_ShouldFailValidation()
    {
        // Arrange
        var longIsoCode = new string('A', 11);
        var request = new UpdateFieldworkMarketRequest(longIsoCode, "Test Market", false);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "ISO code must not exceed 10 characters.");
    }
}
