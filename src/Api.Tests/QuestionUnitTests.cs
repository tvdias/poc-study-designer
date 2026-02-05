using Api.Features.Questions;
using Api.Features.Questions.Validators;

namespace Api.Tests;

public class QuestionUnitTests
{
    [Fact]
    public void CreateQuestionRequest_CreatesInstance()
    {
        var variableName = "EXACT_AGE";
        var questionType = "Numeric input";
        var questionText = "Please type in your age";
        var questionSource = "Standard";
        var request = new CreateQuestionRequest(variableName, questionType, questionText, questionSource);
        Assert.Equal(variableName, request.VariableName);
        Assert.Equal(questionType, request.QuestionType);
        Assert.Equal(questionText, request.QuestionText);
        Assert.Equal(questionSource, request.QuestionSource);
    }
}

public class CreateQuestionValidatorTests
{
    private readonly CreateQuestionValidator _validator = new();

    [Fact]
    public async Task ValidQuestion_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateQuestionRequest("EXACT_AGE", "Numeric input", "Please type in your age", "Standard");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task EmptyVariableName_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateQuestionRequest("", "Numeric input", "Please type in your age", "Standard");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Variable name is required.");
    }

    [Fact]
    public async Task EmptyQuestionType_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateQuestionRequest("EXACT_AGE", "", "Please type in your age", "Standard");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Question type is required.");
    }

    [Fact]
    public async Task EmptyQuestionText_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateQuestionRequest("EXACT_AGE", "Numeric input", "", "Standard");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Question text is required.");
    }

    [Fact]
    public async Task InvalidQuestionSource_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateQuestionRequest("EXACT_AGE", "Numeric input", "Please type in your age", "Invalid");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Question source must be either 'Standard' or 'Custom'.");
    }

    [Fact]
    public async Task CustomQuestionSource_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateQuestionRequest("EXACT_AGE", "Numeric input", "Please type in your age", "Custom");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task QuestionTextExceeding1000Characters_ShouldFailValidation()
    {
        // Arrange
        var longText = new string('a', 1001);
        var request = new CreateQuestionRequest("EXACT_AGE", "Numeric input", longText, "Standard");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Question text must not exceed 1000 characters.");
    }
}
