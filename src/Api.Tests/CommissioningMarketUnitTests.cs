using Api.Features.CommissioningMarkets;
using Api.Features.CommissioningMarkets.Validators;

namespace Api.Tests;

public class CommissioningMarketUnitTests
{
    [Fact]
    public void CreateCommissioningMarketRequest_CreatesInstance()
    {
        var isoCode = "US";
        var name = "United States";
        var request = new CreateCommissioningMarketRequest(isoCode, name);
        Assert.Equal(isoCode, request.IsoCode);
        Assert.Equal(name, request.Name);
    }
}

public class CreateCommissioningMarketValidatorTests
{
    private readonly CreateCommissioningMarketValidator _validator = new();

    [Fact]
    public async Task ValidMarket_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateCommissioningMarketRequest("US", "United States");

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
        var request = new CreateCommissioningMarketRequest("", "United States");

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
        var request = new CreateCommissioningMarketRequest(null!, "United States");

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
        var request = new CreateCommissioningMarketRequest(longIsoCode, "Test Market");

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
        var request = new CreateCommissioningMarketRequest(maxLengthIsoCode, "Test Market");

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
        var request = new CreateCommissioningMarketRequest("US", "");

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
        var request = new CreateCommissioningMarketRequest("US", null!);

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
        var request = new CreateCommissioningMarketRequest("US", longName);

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
        var request = new CreateCommissioningMarketRequest("US", maxLengthName);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}

public class UpdateCommissioningMarketValidatorTests
{
    private readonly UpdateCommissioningMarketValidator _validator = new();

    [Fact]
    public async Task ValidMarket_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateCommissioningMarketRequest("US", "United States");

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
        var request = new UpdateCommissioningMarketRequest("", "United States");

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
        var request = new UpdateCommissioningMarketRequest("US", "");

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
        var request = new UpdateCommissioningMarketRequest("US", longName);

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
        var request = new UpdateCommissioningMarketRequest(longIsoCode, "Test Market");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "ISO code must not exceed 10 characters.");
    }
}
