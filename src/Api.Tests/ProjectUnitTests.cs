using Api.Features.Projects;
using FluentValidation.TestHelper;

namespace Api.Tests;

public class ProjectUnitTests
{
    private readonly CreateProjectRequestValidator _createValidator;
    private readonly UpdateProjectRequestValidator _updateValidator;

    public ProjectUnitTests()
    {
        _createValidator = new CreateProjectRequestValidator();
        _updateValidator = new UpdateProjectRequestValidator();
    }

    [Fact]
    public async Task CreateProject_ValidName_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateProjectRequest(
            Name: "Test Project",
            Description: null,
            ClientId: null,
            ProductId: null,
            Owner: null,
            Status: null,
            CostManagementEnabled: null
        );

        // Act
        var result = await _createValidator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task CreateProject_EmptyName_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProjectRequest(
            Name: "",
            Description: null,
            ClientId: null,
            ProductId: null,
            Owner: null,
            Status: null,
            CostManagementEnabled: null
        );

        // Act
        var result = await _createValidator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    [Fact]
    public async Task CreateProject_NameTooLong_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProjectRequest(
            Name: new string('A', 201), // 201 characters
            Description: null,
            ClientId: null,
            ProductId: null,
            Owner: null,
            Status: null,
            CostManagementEnabled: null
        );

        // Act
        var result = await _createValidator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 200 characters");
    }

    [Fact]
    public async Task CreateProject_NameMaxLength_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateProjectRequest(
            Name: new string('A', 200), // Exactly 200 characters
            Description: null,
            ClientId: null,
            ProductId: null,
            Owner: null,
            Status: null,
            CostManagementEnabled: null
        );

        // Act
        var result = await _createValidator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task CreateProject_DescriptionTooLong_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProjectRequest(
            Name: "Valid Project",
            Description: new string('A', 2001), // 2001 characters
            ClientId: null,
            ProductId: null,
            Owner: null,
            Status: null,
            CostManagementEnabled: null
        );

        // Act
        var result = await _createValidator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must not exceed 2000 characters");
    }

    [Fact]
    public async Task CreateProject_DescriptionMaxLength_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateProjectRequest(
            Name: "Valid Project",
            Description: new string('A', 2000), // Exactly 2000 characters
            ClientId: null,
            ProductId: null,
            Owner: null,
            Status: null,
            CostManagementEnabled: null
        );

        // Act
        var result = await _createValidator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task CreateProject_OwnerTooLong_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProjectRequest(
            Name: "Valid Project",
            Description: null,
            ClientId: null,
            ProductId: null,
            Owner: new string('A', 101), // 101 characters
            Status: null,
            CostManagementEnabled: null
        );

        // Act
        var result = await _createValidator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Owner)
            .WithErrorMessage("Owner must not exceed 100 characters");
    }

    [Fact]
    public async Task CreateProject_OwnerMaxLength_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateProjectRequest(
            Name: "Valid Project",
            Description: null,
            ClientId: null,
            ProductId: null,
            Owner: new string('A', 100), // Exactly 100 characters
            Status: null,
            CostManagementEnabled: null
        );

        // Act
        var result = await _createValidator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Owner);
    }

    [Fact]
    public async Task UpdateProject_ValidName_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateProjectRequest(
            Name: "Updated Project",
            Description: "Updated description",
            ClientId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            Owner: "John Doe",
            Status: ProjectStatus.Active,
            CostManagementEnabled: true
        );

        // Act
        var result = await _updateValidator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task UpdateProject_EmptyName_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateProjectRequest(
            Name: "",
            Description: null,
            ClientId: null,
            ProductId: null,
            Owner: null,
            Status: ProjectStatus.Active,
            CostManagementEnabled: false
        );

        // Act
        var result = await _updateValidator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    [Fact]
    public async Task UpdateProject_NameTooLong_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateProjectRequest(
            Name: new string('A', 201),
            Description: null,
            ClientId: null,
            ProductId: null,
            Owner: null,
            Status: ProjectStatus.Active,
            CostManagementEnabled: false
        );

        // Act
        var result = await _updateValidator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 200 characters");
    }
}
