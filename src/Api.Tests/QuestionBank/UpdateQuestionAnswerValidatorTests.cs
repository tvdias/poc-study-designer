using Api.Features.QuestionBank;
using Api.Features.QuestionBank.Validators;

namespace Api.Tests.QuestionBank;

public class UpdateQuestionAnswerValidatorTests
{
    private readonly UpdateQuestionAnswerValidator _validator = new();

    [Fact]
    public async Task ValidQuestionAnswer_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateQuestionAnswerRequest(
            "Updated Answer Text",
            "CODE2",
            "Location",
            true,
            false,
            true,
            false,
            "Custom",
            "Facets",
            2,
            1
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task EmptyAnswerText_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateQuestionAnswerRequest(
            "",
            null,
            null,
            false,
            false,
            false,
            true,
            null,
            null,
            1,
            null
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Answer Text is required");
    }

    [Fact]
    public async Task NullAnswerText_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateQuestionAnswerRequest(
            null!,
            null,
            null,
            false,
            false,
            false,
            true,
            null,
            null,
            1,
            null
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Answer Text is required");
    }

    [Fact]
    public async Task AnswerTextExceeding500Characters_ShouldFailValidation()
    {
        // Arrange
        var longText = new string('a', 501);
        var request = new UpdateQuestionAnswerRequest(
            longText,
            null,
            null,
            false,
            false,
            false,
            true,
            null,
            null,
            1,
            null
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Answer Text must not exceed 500 characters", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task AnswerTextExactly500Characters_ShouldPassValidation()
    {
        // Arrange
        var maxText = new string('a', 500);
        var request = new UpdateQuestionAnswerRequest(
            maxText,
            "CODE",
            null,
            false,
            false,
            false,
            true,
            null,
            null,
            1,
            null
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task AnswerCodeExceeding50Characters_ShouldFailValidation()
    {
        // Arrange
        var longCode = new string('a', 51);
        var request = new UpdateQuestionAnswerRequest(
            "Answer Text",
            longCode,
            null,
            false,
            false,
            false,
            true,
            null,
            null,
            1,
            null
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Answer Code must not exceed 50 characters", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task AnswerCodeExactly50Characters_ShouldPassValidation()
    {
        // Arrange
        var maxCode = new string('a', 50);
        var request = new UpdateQuestionAnswerRequest(
            "Answer Text",
            maxCode,
            null,
            false,
            false,
            false,
            true,
            null,
            null,
            1,
            null
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task EmptyAnswerCode_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateQuestionAnswerRequest(
            "Answer Text",
            "",
            null,
            false,
            false,
            false,
            true,
            null,
            null,
            1,
            null
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ZeroVersion_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateQuestionAnswerRequest(
            "Answer Text",
            null,
            null,
            false,
            false,
            false,
            true,
            null,
            null,
            0,
            null
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Version must be greater than 0", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task NegativeVersion_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateQuestionAnswerRequest(
            "Answer Text",
            "CODE",
            null,
            false,
            false,
            false,
            true,
            null,
            null,
            -1,
            null
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Version must be greater than 0", result.Errors[0].ErrorMessage);
    }
}
