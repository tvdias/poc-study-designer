namespace DigTx.Designer.FunctionApp.Tests.Mappers;

using System;
using DigTx.Designer.DesignerAssistant.FunctionApp.Models.Requests;
using DigTx.Designer.FunctionApp.Mappers;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Xrm.Sdk;
using Xunit;

public class QuestionnaireLineAnswerMappersTests
{
    [Fact]
    public void MapToEntity_FromQuestionAnswerList_MapsAllExpectedFields()
    {
        // Arrange
        var questionnaireLineId = Guid.NewGuid();
        var answerId = Guid.NewGuid();
        var questionBankId = Guid.NewGuid();

        var source = new KTR_QuestionAnswerList
        {
            KTR_Name = "Answer Name",
            KTR_AnswerId = "ANS-001",
            KTR_DisplayOrder = 5,
            KTR_IsActive = true,
            KTR_AnswerType = KTR_AnswerType.Column,
            KTR_IsTranslatable = false,
            KTR_IsOpen = true,
            KTR_IsExclusive = false,
            KTR_IsFixed = true,
            KTR_CustomProperty = "{\"meta\":\"value\"}",
            KTR_SourceId = "SRC-123",
            KTR_Version = "v1",
            KTR_EffectiveDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            KTR_EndDate = new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            KTR_AnswerText = "Answer display text",
            KTR_KT_QuestionBank = new EntityReference(KT_QuestionBank.EntityLogicalName, questionBankId)
        };
        source.Id = answerId;

        // Act
        var result = source.MapToEntity(questionnaireLineId);

        // Assert
        Assert.NotNull(result);

        Assert.NotNull(result.KTR_QuestionnaireLine);
        Assert.Equal(KT_QuestionnaireLines.EntityLogicalName, result.KTR_QuestionnaireLine.LogicalName);
        Assert.Equal(questionnaireLineId, result.KTR_QuestionnaireLine.Id);

        Assert.NotNull(result.KTR_QuestionAnswer);
        Assert.Equal(KTR_QuestionAnswerList.EntityLogicalName, result.KTR_QuestionAnswer.LogicalName);
        Assert.Equal(answerId, result.KTR_QuestionAnswer.Id);

        Assert.NotNull(result.KTR_QuestionBank);
        Assert.Equal(KT_QuestionBank.EntityLogicalName, result.KTR_QuestionBank.LogicalName);
        Assert.Equal(questionBankId, result.KTR_QuestionBank.Id);

        Assert.Equal(source.KTR_Name, result.KTR_Name);
        Assert.Equal(source.KTR_Name, result.KTR_AnswerCode);
        Assert.Equal(source.KTR_AnswerId, result.KTR_AnswerId);
        Assert.Equal(source.KTR_DisplayOrder, result.KTR_DisplayOrder);
        Assert.Equal(source.KTR_IsActive, result.KTR_IsActive);
        Assert.Equal(source.KTR_AnswerType, result.KTR_AnswerType);
        Assert.Equal(source.KTR_IsTranslatable, result.KTR_IsTranslatable);
        Assert.Equal(source.KTR_IsOpen, result.KTR_IsOpen);
        Assert.Equal(source.KTR_IsExclusive, result.KTR_IsExclusive);
        Assert.Equal(source.KTR_IsFixed, result.KTR_IsFixed);
        Assert.Equal(source.KTR_CustomProperty, result.KTR_CustomProperty);
        Assert.Equal(source.KTR_SourceId, result.KTR_SourceId);
        Assert.Equal(source.KTR_Version, result.KTR_Version);
        Assert.Equal(source.KTR_EffectiveDate, result.KTR_EffectiveDate);
        Assert.Equal(source.KTR_EndDate, result.KTR_EndDate);
        Assert.Equal(source.KTR_AnswerText, result.KTR_AnswerText);
    }

    [Fact]
    public void MapToEntity_FromAnswerCreationRequest_WithNullLocation_SetsNullAnswerType()
    {
        // Arrange
        var questionnaireLineId = Guid.NewGuid();
        var request = new AnswerCreationRequest
        {
            Name = "New Answer",
            Text = "New Answer Text",
            Location = null // Ensures KTR_AnswerType remains null
        };

        // Act
        var result = request.MapToEntity(questionnaireLineId);

        // Assert
        Assert.NotNull(result);

        Assert.NotNull(result.KTR_QuestionnaireLine);
        Assert.Equal(questionnaireLineId, result.KTR_QuestionnaireLine.Id);
        Assert.Equal(KT_QuestionnaireLines.EntityLogicalName, result.KTR_QuestionnaireLine.LogicalName);

        Assert.Equal(request.Name, result.KTR_Name);
        Assert.Equal(request.Text, result.KTR_AnswerText);
        Assert.Null(result.KTR_AnswerType);
    }
}
