namespace DigTx.Designer.FunctionApp.Tests.Mappers;

using System;
using System.Collections.Generic;
using DigTx.Designer.FunctionApp.Mappers;
using DigTx.Designer.FunctionApp.Models;
using global::DigTx.Designer.DesignerAssistant.FunctionApp.Models.Requests;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Xrm.Sdk;

public class QuestionnaireLineMappersTests
{

    [Fact]
    public void MapToEntity_FromRequest_Maps_All_Expected_Fields()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var questionType = QuestionType.DisplayScreen;

        var request = new QuestionCreationRequest
        {
            Origin = OriginType.New,
            Id = Guid.NewGuid(),
            DisplayOrder = 5,
            VariableName = "Q_VAR",
            Title = "Question Title",
            Text = "Question Text Long",
            ScripterNotes = "Some notes",
            QuestionRationale = "Rationale",
            IsDummyQuestion = true,
            QuestionType = questionType
        };

        // Act
        var entity = request.MapToEntity(projectId);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal(projectId, entity.KTR_Project?.Id);
        Assert.Equal(request.DisplayOrder, entity.KT_QuestionSortOrder);
        Assert.Equal(request.VariableName, entity.KT_QuestionVariableName);
        Assert.Equal(request.Title, entity.KT_QuestionTitle);
        Assert.Equal(request.Text, entity.KT_QuestionText2);
        Assert.Equal(request.ScripterNotes, entity.KTR_ScripterNotes);
        Assert.Equal(request.QuestionRationale, entity.KTR_QuestionRationale);
        Assert.Equal(1, entity.KTR_QuestionVersion);
        Assert.Equal(request.IsDummyQuestion, entity.KTR_IsDummyQuestion);

        var stdOrCustom = (OptionSetValue)entity[KT_QuestionnaireLines.Fields.KT_StandardOrCustom];
        Assert.Equal((int)KT_QuestionBank_KT_StandardOrCustom.Custom, stdOrCustom.Value);

        var qType = (OptionSetValue)entity[KT_QuestionnaireLines.Fields.KT_QuestionType];
        Assert.Equal((int)questionType, qType.Value);
    }

    [Fact]
    public void MapToEntity_FromQuestionBank_Maps_All_Expected_Fields()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var bankId = Guid.NewGuid();

        var anyQuestionType = KT_QuestionType.DisplayScreen;

        var questionBank = new KT_QuestionBank(bankId)
        {
            KT_QuestionTitle = "Bank Title",
            KT_DefaultQuestionText = "Bank Text",
            KTR_ScripterNotes = "Bank Notes",
            KT_QuestionRationale = "Bank Rationale",
            KT_QuestionVersion = 3,
            KTR_AnswerList = "1|Yes;2|No",
            KT_IsDummyQuestion = false,
            KT_StandardOrCustom = KT_QuestionBank_KT_StandardOrCustom.Standard,
            KT_QuestionType = anyQuestionType,
            KT_Name = "BANK_VAR"
        };

        var moduleId = Guid.NewGuid();

        var request = new QuestionCreationRequest
        {
            Origin = OriginType.QuestionBank,
            Id = bankId,
            DisplayOrder = 10,
            Module = new ModuleCreationRequest { Id = moduleId }
        };

        var list = new List<QuestionCreationRequest> { request };

        // Act
        var entity = questionBank.MapToEntity(list, projectId);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal(projectId, entity.KTR_Project?.Id);
        Assert.Equal(moduleId, entity.KTR_Module?.Id);
        Assert.Equal(bankId, entity.KTR_QuestionBank?.Id);
        Assert.Equal(request.DisplayOrder, entity.KT_QuestionSortOrder);
        Assert.Equal(questionBank.KT_Name, entity.KT_QuestionVariableName);
        Assert.Equal(questionBank.KT_QuestionTitle, entity.KT_QuestionTitle);
        Assert.Equal(questionBank.KT_DefaultQuestionText, entity.KT_QuestionText2);
        Assert.Equal(questionBank.KTR_ScripterNotes, entity.KTR_ScripterNotes);
        Assert.Equal(questionBank.KT_QuestionRationale, entity.KTR_QuestionRationale);
        Assert.Equal(questionBank.KT_QuestionVersion, entity.KTR_QuestionVersion);
        Assert.Equal(questionBank.KTR_AnswerList, entity.KTR_AnswerList);
        Assert.Equal(questionBank.KT_IsDummyQuestion, entity.KTR_IsDummyQuestion);

        var stdOrCustom = (OptionSetValue)entity[KT_QuestionnaireLines.Fields.KT_StandardOrCustom];
        Assert.Equal((int)questionBank.KT_StandardOrCustom, stdOrCustom.Value);

        var qType = (OptionSetValue)entity[KT_QuestionnaireLines.Fields.KT_QuestionType];
        Assert.Equal((int)questionBank.KT_QuestionType, qType.Value);
    }

    [Fact]
    public void MapToEntity_FromQuestionBank_WithoutMatchingRequest_Throws()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var questionBank = new KT_QuestionBank(Guid.NewGuid())
        {
            KT_QuestionTitle = "Bank Title",
            KT_DefaultQuestionText = "Bank Text",
            KT_QuestionVersion = 1,
            KT_StandardOrCustom = KT_QuestionBank_KT_StandardOrCustom.Standard,
            KT_QuestionType = KT_QuestionType.SmallTextInput,
            KT_Name = "BANK_VAR"
        };

        var emptyRequests = new List<QuestionCreationRequest>(); // No matching request

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => questionBank.MapToEntity(emptyRequests, projectId));
    }
}
