using Api.Features.Tags;
using Api.Features.Tags.Validators;
using Xunit;

namespace Api.Tests;

public class TagUnitTests
{
    [Fact]
    public void CreateTagRequest_CreatesInstance()
    {
        var name = "Unit Test Tag";
        var request = new CreateTagRequest(name);
        Assert.Equal(name, request.Name);
    }
}

public class CreateTagValidatorTests
{
    private readonly CreateTagValidator _validator = new();

    [Fact]
    public async Task ValidTag_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateTagRequest("Valid Tag Name");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task EmptyName_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateTagRequest("");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Tag name is required.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task NullName_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateTagRequest(null!);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Tag name is required.");
    }

    [Fact]
    public async Task NameExceeding100Characters_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('a', 101);
        var request = new CreateTagRequest(longName);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Tag name must not exceed 100 characters.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task NameExactly100Characters_ShouldPassValidation()
    {
        // Arrange
        var maxLengthName = new string('a', 100);
        var request = new CreateTagRequest(maxLengthName);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task WhitespaceName_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateTagRequest("   ");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Tag name is required.");
    }
}

public class UpdateTagValidatorTests
{
    private readonly UpdateTagValidator _validator = new();

    [Fact]
    public async Task ValidTag_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateTagRequest("Updated Tag Name", true);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task EmptyName_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateTagRequest("", true);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Tag name is required.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task NullName_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateTagRequest(null!, false);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Tag name is required.");
    }

    [Fact]
    public async Task NameExceeding100Characters_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('a', 101);
        var request = new UpdateTagRequest(longName, true);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Tag name must not exceed 100 characters.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task NameExactly100Characters_ShouldPassValidation()
    {
        // Arrange
        var maxLengthName = new string('a', 100);
        var request = new UpdateTagRequest(maxLengthName, false);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task WhitespaceName_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateTagRequest("   ", true);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Tag name is required.");
    }
}
