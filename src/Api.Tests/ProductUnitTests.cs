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

public class CreateProductConfigQuestionValidatorTests
{
    private readonly CreateProductConfigQuestionValidator _validator = new();

    [Fact]
    public async Task ValidProductConfigQuestion_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateProductConfigQuestionRequest(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task EmptyProductId_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProductConfigQuestionRequest(Guid.Empty, Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Product ID is required", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task EmptyConfigurationQuestionId_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProductConfigQuestionRequest(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Configuration Question ID is required", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task BothIdsEmpty_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProductConfigQuestionRequest(Guid.Empty, Guid.Empty);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }
}

public class UpdateProductConfigQuestionValidatorTests
{
    private readonly UpdateProductConfigQuestionValidator _validator = new();

    [Fact]
    public async Task ValidProductConfigQuestion_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateProductConfigQuestionRequest(true);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task IsActiveFalse_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateProductConfigQuestionRequest(false);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}

public class CreateProductConfigQuestionDisplayRuleValidatorTests
{
    private readonly CreateProductConfigQuestionDisplayRuleValidator _validator = new();

    [Fact]
    public async Task ValidDisplayRuleWithShow_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateProductConfigQuestionDisplayRuleRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "Show"
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidDisplayRuleWithHide_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateProductConfigQuestionDisplayRuleRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Hide"
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task EmptyProductConfigQuestionId_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProductConfigQuestionDisplayRuleRequest(
            Guid.Empty,
            Guid.NewGuid(),
            null,
            "Show"
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Product config question ID is required", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task EmptyTriggeringConfigurationQuestionId_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProductConfigQuestionDisplayRuleRequest(
            Guid.NewGuid(),
            Guid.Empty,
            null,
            "Show"
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Triggering configuration question ID is required", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task EmptyDisplayCondition_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProductConfigQuestionDisplayRuleRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            ""
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Display condition is required");
    }

    [Fact]
    public async Task InvalidDisplayCondition_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProductConfigQuestionDisplayRuleRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "Invalid"
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Display condition must be either 'Show' or 'Hide'");
    }
}

public class UpdateProductConfigQuestionDisplayRuleValidatorTests
{
    private readonly UpdateProductConfigQuestionDisplayRuleValidator _validator = new();

    [Fact]
    public async Task ValidDisplayRule_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateProductConfigQuestionDisplayRuleRequest(
            Guid.NewGuid(),
            null,
            "Show",
            true
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task EmptyTriggeringConfigurationQuestionId_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateProductConfigQuestionDisplayRuleRequest(
            Guid.Empty,
            null,
            "Hide",
            true
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Triggering configuration question ID is required", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task InvalidDisplayCondition_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateProductConfigQuestionDisplayRuleRequest(
            Guid.NewGuid(),
            null,
            "Toggle",
            false
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Display condition must be either 'Show' or 'Hide'");
    }
}

public class CreateProductTemplateLineValidatorTests
{
    private readonly CreateProductTemplateLineValidator _validator = new();

    [Fact]
    public async Task ValidTemplateLineWithModule_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateProductTemplateLineRequest(
            Guid.NewGuid(),
            "Module Line",
            "Module",
            true,
            1,
            Guid.NewGuid(),
            null
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidTemplateLineWithQuestion_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateProductTemplateLineRequest(
            Guid.NewGuid(),
            "Question Line",
            "Question",
            false,
            2,
            null,
            Guid.NewGuid()
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task EmptyProductTemplateId_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProductTemplateLineRequest(
            Guid.Empty,
            "Line",
            "Module",
            true,
            1,
            Guid.NewGuid(),
            null
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Product template ID is required");
    }

    [Fact]
    public async Task EmptyName_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProductTemplateLineRequest(
            Guid.NewGuid(),
            "",
            "Module",
            true,
            1,
            Guid.NewGuid(),
            null
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Name is required");
    }

    [Fact]
    public async Task NameExceeding200Characters_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('a', 201);
        var request = new CreateProductTemplateLineRequest(
            Guid.NewGuid(),
            longName,
            "Module",
            true,
            1,
            Guid.NewGuid(),
            null
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Name must not exceed 200 characters");
    }

    [Fact]
    public async Task InvalidType_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProductTemplateLineRequest(
            Guid.NewGuid(),
            "Line",
            "Invalid",
            true,
            1,
            null,
            null
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Type must be either 'Module' or 'Question'");
    }

    [Fact]
    public async Task ModuleTypeWithoutModuleId_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProductTemplateLineRequest(
            Guid.NewGuid(),
            "Module Line",
            "Module",
            true,
            1,
            null,
            null
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Module ID is required when Type is 'Module'");
    }

    [Fact]
    public async Task QuestionTypeWithoutQuestionBankItemId_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProductTemplateLineRequest(
            Guid.NewGuid(),
            "Question Line",
            "Question",
            true,
            1,
            null,
            null
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Question Bank Item ID is required when Type is 'Question'");
    }

    [Fact]
    public async Task BothModuleIdAndQuestionBankItemId_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProductTemplateLineRequest(
            Guid.NewGuid(),
            "Line",
            "Module",
            true,
            1,
            Guid.NewGuid(),
            Guid.NewGuid()
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Either Module ID or Question Bank Item ID must be provided, not both");
    }
}

public class UpdateProductTemplateLineValidatorTests
{
    private readonly UpdateProductTemplateLineValidator _validator = new();

    [Fact]
    public async Task ValidTemplateLineWithModule_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateProductTemplateLineRequest(
            "Updated Module Line",
            "Module",
            true,
            1,
            Guid.NewGuid(),
            null,
            true
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidTemplateLineWithQuestion_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateProductTemplateLineRequest(
            "Updated Question Line",
            "Question",
            false,
            2,
            null,
            Guid.NewGuid(),
            false
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
        var request = new UpdateProductTemplateLineRequest(
            "",
            "Module",
            true,
            1,
            Guid.NewGuid(),
            null,
            true
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Name is required");
    }

    [Fact]
    public async Task NameExceeding200Characters_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('a', 201);
        var request = new UpdateProductTemplateLineRequest(
            longName,
            "Module",
            true,
            1,
            Guid.NewGuid(),
            null,
            true
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Name must not exceed 200 characters");
    }

    [Fact]
    public async Task InvalidType_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateProductTemplateLineRequest(
            "Line",
            "Section",
            true,
            1,
            null,
            null,
            true
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Type must be either 'Module' or 'Question'");
    }

    [Fact]
    public async Task ModuleTypeWithoutModuleId_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateProductTemplateLineRequest(
            "Module Line",
            "Module",
            true,
            1,
            null,
            null,
            true
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Module ID is required when Type is 'Module'");
    }

    [Fact]
    public async Task QuestionTypeWithoutQuestionBankItemId_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateProductTemplateLineRequest(
            "Question Line",
            "Question",
            true,
            1,
            null,
            null,
            true
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Question Bank Item ID is required when Type is 'Question'");
    }

    [Fact]
    public async Task BothModuleIdAndQuestionBankItemId_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateProductTemplateLineRequest(
            "Line",
            "Question",
            true,
            1,
            Guid.NewGuid(),
            Guid.NewGuid(),
            true
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Either Module ID or Question Bank Item ID must be provided, not both");
    }
}
