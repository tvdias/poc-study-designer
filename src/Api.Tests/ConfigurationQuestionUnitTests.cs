using Api.Features.ConfigurationQuestions;
using Api.Features.ConfigurationQuestions.Validators;

namespace Api.Tests;

public class ConfigurationQuestionUnitTests
{
    [Fact]
    public void CreateConfigurationQuestionRequest_CreatesInstance()
    {
        var question = "What is your favorite color?";
        var aiPrompt = "Please select the color that best represents your preference.";
        var ruleType = RuleType.SingleCoded;
        
        var request = new CreateConfigurationQuestionRequest(question, aiPrompt, ruleType);
        
        Assert.Equal(question, request.Question);
        Assert.Equal(aiPrompt, request.AiPrompt);
        Assert.Equal(ruleType, request.RuleType);
    }
}

public class CreateConfigurationQuestionValidatorTests
{
    private readonly CreateConfigurationQuestionValidator _validator = new();

    [Fact]
    public async Task ValidQuestion_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateConfigurationQuestionRequest("Valid Question?", null, RuleType.SingleCoded);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task EmptyQuestion_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateConfigurationQuestionRequest("", null, RuleType.SingleCoded);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Question is required.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task NullQuestion_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateConfigurationQuestionRequest(null!, null, RuleType.SingleCoded);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Question is required.");
    }

    [Fact]
    public async Task QuestionExceeding500Characters_ShouldFailValidation()
    {
        // Arrange
        var longQuestion = new string('a', 501);
        var request = new CreateConfigurationQuestionRequest(longQuestion, null, RuleType.SingleCoded);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Question must not exceed 500 characters.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task QuestionExactly500Characters_ShouldPassValidation()
    {
        // Arrange
        var maxLengthQuestion = new string('a', 500);
        var request = new CreateConfigurationQuestionRequest(maxLengthQuestion, null, RuleType.SingleCoded);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task WhitespaceQuestion_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateConfigurationQuestionRequest("   ", null, RuleType.SingleCoded);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Question is required.");
    }
}

public class UpdateConfigurationQuestionValidatorTests
{
    private readonly UpdateConfigurationQuestionValidator _validator = new();

    [Fact]
    public async Task ValidQuestion_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateConfigurationQuestionRequest("Updated Question?", "AI prompt", RuleType.MultiCoded, true);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task EmptyQuestion_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateConfigurationQuestionRequest("", null, RuleType.SingleCoded, true);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Question is required.", result.Errors[0].ErrorMessage);
    }
}

public class CreateConfigurationAnswerValidatorTests
{
    private readonly CreateConfigurationAnswerValidator _validator = new();

    [Fact]
    public async Task ValidAnswer_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateConfigurationAnswerRequest("Answer 1", Guid.NewGuid());

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
        var request = new CreateConfigurationAnswerRequest("", Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Answer name is required.");
    }

    [Fact]
    public async Task NameExceeding200Characters_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('a', 201);
        var request = new CreateConfigurationAnswerRequest(longName, Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Answer name must not exceed 200 characters.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task EmptyGuid_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateConfigurationAnswerRequest("Answer 1", Guid.Empty);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Configuration Question ID is required.");
    }
}

public class CreateDependencyRuleValidatorTests
{
    private readonly CreateDependencyRuleValidator _validator = new();

    [Fact]
    public async Task ValidRule_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateDependencyRuleRequest(
            "Rule 1", 
            Guid.NewGuid(), 
            Guid.NewGuid(),
            "Classification A",
            "Type A",
            "Content Type A",
            "Module A",
            "Question Bank A",
            "Tag A"
        );

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
        var request = new CreateDependencyRuleRequest("", Guid.NewGuid(), null, null, null, null, null, null, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Dependency rule name is required.");
    }

    [Fact]
    public async Task NameExceeding200Characters_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('a', 201);
        var request = new CreateDependencyRuleRequest(longName, Guid.NewGuid(), null, null, null, null, null, null, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Dependency rule name must not exceed 200 characters.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ClassificationExceeding100Characters_ShouldFailValidation()
    {
        // Arrange
        var longClassification = new string('a', 101);
        var request = new CreateDependencyRuleRequest("Rule 1", Guid.NewGuid(), null, longClassification, null, null, null, null, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Classification must not exceed 100 characters.", result.Errors[0].ErrorMessage);
    }
}
