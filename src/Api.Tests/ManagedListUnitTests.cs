using Api.Features.ManagedLists;
using Api.Features.ManagedLists.Validators;

namespace Api.Tests;

public class ManagedListUnitTests
{
    [Fact]
    public void CreateManagedListRequest_CreatesInstance()
    {
        var projectId = Guid.NewGuid();
        var name = "Test Managed List";
        var description = "Test description";
        var request = new CreateManagedListRequest(projectId, name, description);
        
        Assert.Equal(projectId, request.ProjectId);
        Assert.Equal(name, request.Name);
        Assert.Equal(description, request.Description);
    }
}

public class CreateManagedListValidatorTests
{
    private readonly CreateManagedListValidator _validator = new();

    [Fact]
    public async Task ValidManagedList_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateManagedListRequest(Guid.NewGuid(), "Valid Managed List Name", "Description");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task EmptyProjectId_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateManagedListRequest(Guid.Empty, "Name", "Description");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Project ID is required.");
    }

    [Fact]
    public async Task EmptyName_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateManagedListRequest(Guid.NewGuid(), "", "Description");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Managed list name is required.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task NullName_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateManagedListRequest(Guid.NewGuid(), null!, "Description");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Managed list name is required.");
    }

    [Fact]
    public async Task NameExceeding200Characters_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('a', 201);
        var request = new CreateManagedListRequest(Guid.NewGuid(), longName, "Description");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Managed list name must not exceed 200 characters.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task NameExactly200Characters_ShouldPassValidation()
    {
        // Arrange
        var maxLengthName = new string('a', 200);
        var request = new CreateManagedListRequest(Guid.NewGuid(), maxLengthName, "Description");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task DescriptionExceeding1000Characters_ShouldFailValidation()
    {
        // Arrange
        var longDescription = new string('a', 1001);
        var request = new CreateManagedListRequest(Guid.NewGuid(), "Name", longDescription);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Description must not exceed 1000 characters.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task NullDescription_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateManagedListRequest(Guid.NewGuid(), "Name", null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}

public class UpdateManagedListValidatorTests
{
    private readonly UpdateManagedListValidator _validator = new();

    [Fact]
    public async Task ValidManagedList_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateManagedListRequest("Updated Managed List Name", "Updated Description");

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
        var request = new UpdateManagedListRequest("", "Description");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Managed list name is required.", result.Errors[0].ErrorMessage);
    }
}

public class CreateManagedListItemValidatorTests
{
    private readonly CreateManagedListItemValidator _validator = new();

    [Fact]
    public async Task ValidManagedListItem_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateManagedListItemRequest("Value1", "Label 1", 1);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidCodeWithUnderscores_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateManagedListItemRequest("COCA_COLA", "Coca-Cola", 1);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task EmptyValue_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateManagedListItemRequest("", "Label", 1);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Item value (code) is required.");
    }

    [Fact]
    public async Task EmptyLabel_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateManagedListItemRequest("Value", "", 1);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Item label (name) is required.");
    }

    [Fact]
    public async Task NegativeSortOrder_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateManagedListItemRequest("Value", "Label", -1);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Sort order must be greater than or equal to 0.");
    }

    [Fact]
    public async Task ValueExceeding100Characters_ShouldFailValidation()
    {
        // Arrange
        var longValue = new string('a', 101);
        var request = new CreateManagedListItemRequest(longValue, "Label", 1);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Item value (code) must not exceed 100 characters.");
    }

    [Fact]
    public async Task LabelExceeding200Characters_ShouldFailValidation()
    {
        // Arrange
        var longLabel = new string('a', 201);
        var request = new CreateManagedListItemRequest("Value", longLabel, 1);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Item label (name) must not exceed 200 characters.");
    }

    [Fact]
    public async Task CodeStartingWithNumber_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateManagedListItemRequest("123Value", "Label", 1);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Item value (code) must start with a letter and contain only alphanumeric characters and underscores.");
    }

    [Fact]
    public async Task CodeWithSpecialCharacters_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateManagedListItemRequest("Value-123", "Label", 1);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Item value (code) must start with a letter and contain only alphanumeric characters and underscores.");
    }

    [Fact]
    public async Task CodeWithSpaces_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateManagedListItemRequest("Value 123", "Label", 1);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Item value (code) must start with a letter and contain only alphanumeric characters and underscores.");
    }
}

public class AssignManagedListToQuestionValidatorTests
{
    private readonly AssignManagedListToQuestionValidator _validator = new();

    [Fact]
    public async Task ValidAssignment_ShouldPassValidation()
    {
        // Arrange
        var request = new AssignManagedListToQuestionRequest(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task EmptyQuestionnaireLineId_ShouldFailValidation()
    {
        // Arrange
        var request = new AssignManagedListToQuestionRequest(Guid.Empty, Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Questionnaire line ID is required.");
    }

    [Fact]
    public async Task EmptyManagedListId_ShouldFailValidation()
    {
        // Arrange
        var request = new AssignManagedListToQuestionRequest(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Managed list ID is required.");
    }
}
