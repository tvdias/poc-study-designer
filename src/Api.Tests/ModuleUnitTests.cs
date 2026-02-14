using Api.Features.Modules;
using Api.Features.Modules.Validators;

namespace Api.Tests;

public class ModuleUnitTests
{
    [Fact]
    public void CreateModuleRequest_CreatesInstance()
    {
        var variableName = "AGE - V1";
        var label = "AGE";
        var description = "AGE Module";
        var versionNumber = 1;
        var request = new CreateModuleRequest(variableName, label, description, versionNumber, null, null);
        Assert.Equal(variableName, request.VariableName);
        Assert.Equal(label, request.Label);
        Assert.Equal(description, request.Description);
        Assert.Equal(versionNumber, request.VersionNumber);
    }
}

public class CreateModuleValidatorTests
{
    private readonly CreateModuleValidator _validator = new();

    [Fact]
    public async Task ValidModule_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateModuleRequest("AGE - V1", "AGE", "AGE Module", 1, null, null);

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
        var request = new CreateModuleRequest("", "AGE", "AGE Module", 1, null, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Variable name is required.");
    }

    [Fact]
    public async Task EmptyLabel_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateModuleRequest("AGE - V1", "", "AGE Module", 1, null, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Label is required.");
    }

    [Fact]
    public async Task VersionNumberZero_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateModuleRequest("AGE - V1", "AGE", "AGE Module", 0, null, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Version number must be greater than 0.");
    }

    [Fact]
    public async Task VariableNameExceeding100Characters_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('a', 101);
        var request = new CreateModuleRequest(longName, "AGE", "AGE Module", 1, null, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Variable name must not exceed 100 characters.");
    }

    [Fact]
    public async Task DescriptionExceeding500Characters_ShouldFailValidation()
    {
        // Arrange
        var longDescription = new string('a', 501);
        var request = new CreateModuleRequest("AGE - V1", "AGE", longDescription, 1, null, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Description must not exceed 500 characters.");
    }
}

public class UpdateModuleValidatorTests
{
    private readonly UpdateModuleValidator _validator = new();

    [Fact]
    public async Task ValidModule_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateModuleRequest("AGE - V1", "AGE", "AGE Module", 1, null, null, true);

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
        var request = new UpdateModuleRequest("", "AGE", "AGE Module", 1, null, null, true);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Variable name is required.");
    }
}

public class CreateModuleQuestionValidatorTests
{
    private readonly CreateModuleQuestionValidator _validator = new();

    [Fact]
    public async Task ValidModuleQuestion_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateModuleQuestionRequest(Guid.NewGuid(), 1);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task EmptyQuestionBankItemId_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateModuleQuestionRequest(Guid.Empty, 1);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Question Bank Item is required.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task NegativeDisplayOrder_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateModuleQuestionRequest(Guid.NewGuid(), -1);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Display order must be 0 or greater.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ZeroDisplayOrder_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateModuleQuestionRequest(Guid.NewGuid(), 0);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}

public class UpdateModuleQuestionValidatorTests
{
    private readonly UpdateModuleQuestionValidator _validator = new();

    [Fact]
    public async Task ValidModuleQuestion_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateModuleQuestionRequest(1, true);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task NegativeDisplayOrder_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateModuleQuestionRequest(-1, true);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Display order must be greater than or equal to 0", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ZeroDisplayOrder_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateModuleQuestionRequest(0, false);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
