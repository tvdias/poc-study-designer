using Api.Features.MetricGroups;
using Api.Features.MetricGroups.Validators;

namespace Api.Tests;

public class MetricGroupUnitTests
{
    [Fact]
    public void CreateMetricGroupRequest_CreatesInstance()
    {
        var name = "Test Metric Group";
        var request = new CreateMetricGroupRequest(name);
        Assert.Equal(name, request.Name);
    }
}

public class CreateMetricGroupValidatorTests
{
    private readonly CreateMetricGroupValidator _validator = new();

    [Fact]
    public async Task ValidMetricGroup_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateMetricGroupRequest("Valid Metric Group");

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
        var request = new CreateMetricGroupRequest("");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name is required.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task NullName_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateMetricGroupRequest(null!);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Name is required.");
    }

    [Fact]
    public async Task NameExceeding100Characters_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('a', 101);
        var request = new CreateMetricGroupRequest(longName);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name must not exceed 100 characters.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task NameExactly100Characters_ShouldPassValidation()
    {
        // Arrange
        var maxLengthName = new string('a', 100);
        var request = new CreateMetricGroupRequest(maxLengthName);

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
        var request = new CreateMetricGroupRequest("   ");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Name is required.");
    }
}

public class UpdateMetricGroupValidatorTests
{
    private readonly UpdateMetricGroupValidator _validator = new();

    [Fact]
    public async Task ValidUpdateMetricGroup_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateMetricGroupRequest("Valid Update", true);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task EmptyName_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateMetricGroupRequest("", true);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Name is required.");
    }

    [Fact]
    public async Task NameExceeding100Characters_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('a', 101);
        var request = new UpdateMetricGroupRequest(longName, true);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Name must not exceed 100 characters.");
    }
}
