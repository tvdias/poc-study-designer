using Api.Features.ManagedLists;
using Api.Features.ManagedLists.Validators;

namespace Api.Tests;

public class SubsetSignatureBuilderTests
{
    [Fact]
    public void BuildSignature_WithValidIds_ReturnsConsistentHash()
    {
        // Arrange
        var ids = new List<Guid>
        {
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Guid.Parse("33333333-3333-3333-3333-333333333333")
        };

        // Act
        var signature1 = SubsetSignatureBuilder.BuildSignature(ids);
        var signature2 = SubsetSignatureBuilder.BuildSignature(ids);

        // Assert
        Assert.Equal(signature1, signature2);
        Assert.NotEmpty(signature1);
    }

    [Fact]
    public void BuildSignature_WithDifferentOrder_ReturnsSameHash()
    {
        // Arrange
        var ids1 = new List<Guid>
        {
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Guid.Parse("33333333-3333-3333-3333-333333333333")
        };
        var ids2 = new List<Guid>
        {
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Guid.Parse("22222222-2222-2222-2222-222222222222")
        };

        // Act
        var signature1 = SubsetSignatureBuilder.BuildSignature(ids1);
        var signature2 = SubsetSignatureBuilder.BuildSignature(ids2);

        // Assert
        Assert.Equal(signature1, signature2);
    }

    [Fact]
    public void BuildSignature_WithDuplicates_HandlesDuplication()
    {
        // Arrange
        var idsWithDuplicates = new List<Guid>
        {
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Guid.Parse("11111111-1111-1111-1111-111111111111")
        };
        var idsWithoutDuplicates = new List<Guid>
        {
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Guid.Parse("22222222-2222-2222-2222-222222222222")
        };

        // Act
        var signature1 = SubsetSignatureBuilder.BuildSignature(idsWithDuplicates);
        var signature2 = SubsetSignatureBuilder.BuildSignature(idsWithoutDuplicates);

        // Assert
        Assert.Equal(signature1, signature2);
    }

    [Fact]
    public void BuildSignature_WithDifferentIds_ReturnsDifferentHash()
    {
        // Arrange
        var ids1 = new List<Guid>
        {
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Guid.Parse("22222222-2222-2222-2222-222222222222")
        };
        var ids2 = new List<Guid>
        {
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Guid.Parse("33333333-3333-3333-3333-333333333333")
        };

        // Act
        var signature1 = SubsetSignatureBuilder.BuildSignature(ids1);
        var signature2 = SubsetSignatureBuilder.BuildSignature(ids2);

        // Assert
        Assert.NotEqual(signature1, signature2);
    }

    [Fact]
    public void BuildSignature_WithEmptyCollection_ThrowsException()
    {
        // Arrange
        var emptyIds = new List<Guid>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => SubsetSignatureBuilder.BuildSignature(emptyIds));
    }

    [Fact]
    public void BuildSignature_ReturnsLowercaseHexString()
    {
        // Arrange
        var ids = new List<Guid>
        {
            Guid.Parse("11111111-1111-1111-1111-111111111111")
        };

        // Act
        var signature = SubsetSignatureBuilder.BuildSignature(ids);

        // Assert
        Assert.Equal(signature, signature.ToLowerInvariant());
        Assert.Matches("^[0-9a-f]+$", signature);
    }
}

public class SaveQuestionSelectionValidatorTests
{
    private readonly SaveQuestionSelectionValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldPassValidation()
    {
        // Arrange
        var request = new SaveQuestionSelectionRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
        );

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
        var request = new SaveQuestionSelectionRequest(
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new List<Guid> { Guid.NewGuid() }
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Project ID is required.");
    }

    [Fact]
    public async Task EmptyQuestionnaireLineId_ShouldFailValidation()
    {
        // Arrange
        var request = new SaveQuestionSelectionRequest(
            Guid.NewGuid(),
            Guid.Empty,
            Guid.NewGuid(),
            new List<Guid> { Guid.NewGuid() }
        );

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
        var request = new SaveQuestionSelectionRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            new List<Guid> { Guid.NewGuid() }
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Managed list ID is required.");
    }

    [Fact]
    public async Task EmptySelectedItems_ShouldFailValidation()
    {
        // Arrange
        var request = new SaveQuestionSelectionRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new List<Guid>()
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "At least one managed list item must be selected.");
    }

    [Fact]
    public async Task NullSelectedItems_ShouldFailValidation()
    {
        // Arrange
        var request = new SaveQuestionSelectionRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null!
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Selected items list is required.");
    }
}
