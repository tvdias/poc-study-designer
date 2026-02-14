using Api.Features.QuestionBank;
using Api.Features.QuestionBank.Validators;
using Xunit;

namespace Api.Tests.QuestionBank;

public class CreateQuestionBankItemValidatorTests
{
    private readonly CreateQuestionBankItemValidator _validator = new();

    [Fact]
    public async Task ValidQuestionBankItem_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateQuestionBankItemRequest(
            VariableName: "Q1_AGE",
            Version: 1,
            QuestionType: "Logic",
            QuestionText: "What is your age?",
            Classification: "Standard",
            IsDummy: false,
            QuestionTitle: "Age Question",
            Status: "Active",
            Methodology: "CAWI",
            DataQualityTag: null,
            RowSortOrder: null,
            ColumnSortOrder: null,
            AnswerMin: null,
            AnswerMax: null,
            QuestionFormatDetails: null,
            ScraperNotes: null,
            CustomNotes: null,
            MetricGroupId: Guid.NewGuid(),
            TableTitle: null,
            QuestionRationale: null,
            SingleOrMulticode: "Single",
            ManagedListReferences: null,
            IsTranslatable: false,
            IsHidden: false,
            IsQuestionActive: true,
            IsQuestionOutOfUse: false,
            AnswerRestrictionMin: null,
            AnswerRestrictionMax: null,
            RestrictionDataType: null,
            RestrictedToClient: null,
            AnswerTypeCode: null,
            IsAnswerRequired: false,
            ScalePoint: null,
            ScaleType: null,
            DisplayType: null,
            InstructionText: null,
            ParentQuestionId: null,
            QuestionFacet: null
        );

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
        var request = new CreateQuestionBankItemRequest(
            VariableName: "",
            Version: 1,
            QuestionType: null,
            QuestionText: null,
            Classification: null,
            IsDummy: false,
            QuestionTitle: null,
            Status: null,
            Methodology: null,
            DataQualityTag: null,
            RowSortOrder: null,
            ColumnSortOrder: null,
            AnswerMin: null,
            AnswerMax: null,
            QuestionFormatDetails: null,
            ScraperNotes: null,
            CustomNotes: null,
            MetricGroupId: null,
            TableTitle: null,
            QuestionRationale: null,
            SingleOrMulticode: null,
            ManagedListReferences: null,
            IsTranslatable: false,
            IsHidden: false,
            IsQuestionActive: true,
            IsQuestionOutOfUse: false,
            AnswerRestrictionMin: null,
            AnswerRestrictionMax: null,
            RestrictionDataType: null,
            RestrictedToClient: null,
            AnswerTypeCode: null,
            IsAnswerRequired: false,
            ScalePoint: null,
            ScaleType: null,
            DisplayType: null,
            InstructionText: null,
            ParentQuestionId: null,
            QuestionFacet: null
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "VariableName");
    }

    [Fact]
    public async Task VariableNameTooLong_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('A', 201);
        var request = new CreateQuestionBankItemRequest(
            VariableName: longName,
            Version: 1,
            QuestionType: null,
            QuestionText: null,
            Classification: null,
            IsDummy: false,
            QuestionTitle: null,
            Status: null,
            Methodology: null,
            DataQualityTag: null,
            RowSortOrder: null,
            ColumnSortOrder: null,
            AnswerMin: null,
            AnswerMax: null,
            QuestionFormatDetails: null,
            ScraperNotes: null,
            CustomNotes: null,
            MetricGroupId: null,
            TableTitle: null,
            QuestionRationale: null,
            SingleOrMulticode: null,
            ManagedListReferences: null,
            IsTranslatable: false,
            IsHidden: false,
            IsQuestionActive: true,
            IsQuestionOutOfUse: false,
            AnswerRestrictionMin: null,
            AnswerRestrictionMax: null,
            RestrictionDataType: null,
            RestrictedToClient: null,
            AnswerTypeCode: null,
            IsAnswerRequired: false,
            ScalePoint: null,
            ScaleType: null,
            DisplayType: null,
            InstructionText: null,
            ParentQuestionId: null,
            QuestionFacet: null
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "VariableName");
    }

    [Fact]
    public async Task ZeroVersion_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateQuestionBankItemRequest(
            VariableName: "Q1",
            Version: 0,
            QuestionType: null,
            QuestionText: null,
            Classification: null,
            IsDummy: false,
            QuestionTitle: null,
            Status: null,
            Methodology: null,
            DataQualityTag: null,
            RowSortOrder: null,
            ColumnSortOrder: null,
            AnswerMin: null,
            AnswerMax: null,
            QuestionFormatDetails: null,
            ScraperNotes: null,
            CustomNotes: null,
            MetricGroupId: null,
            TableTitle: null,
            QuestionRationale: null,
            SingleOrMulticode: null,
            ManagedListReferences: null,
            IsTranslatable: false,
            IsHidden: false,
            IsQuestionActive: true,
            IsQuestionOutOfUse: false,
            AnswerRestrictionMin: null,
            AnswerRestrictionMax: null,
            RestrictionDataType: null,
            RestrictedToClient: null,
            AnswerTypeCode: null,
            IsAnswerRequired: false,
            ScalePoint: null,
            ScaleType: null,
            DisplayType: null,
            InstructionText: null,
            ParentQuestionId: null,
            QuestionFacet: null
        );

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Version");
    }
}
