namespace DigTx.Designer.FunctionApp.Mappers;

using System;
using DigTx.Designer.DesignerAssistant.FunctionApp.Models.Requests;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Xrm.Sdk;

public static class QuestionnaireLineMappers
{
    public static KT_QuestionnaireLines MapToEntity(
            this QuestionCreationRequest request,
            Guid projectId)
    {
        var questionLine = new KT_QuestionnaireLines
        {
            KT_QuestionnaireLinesId = Guid.NewGuid(),
            KTR_Project = new EntityReference(KT_Project.EntityLogicalName, projectId),
            KT_QuestionSortOrder = request.DisplayOrder,
            KT_QuestionVariableName = request.VariableName,
            KT_QuestionTitle = request.Title,
            KT_QuestionText2 = request.Text,
            KTR_ScripterNotes = request.ScripterNotes,
            KTR_QuestionRationale = request.QuestionRationale,
            KTR_QuestionVersion = 1,
            KTR_IsDummyQuestion = request.IsDummyQuestion,
        };
        questionLine[KT_QuestionnaireLines.Fields.KT_StandardOrCustom] = new OptionSetValue((int)KT_QuestionBank_KT_StandardOrCustom.Custom);
        questionLine[KT_QuestionnaireLines.Fields.KT_QuestionType] = new OptionSetValue((int)request.QuestionType);

        return questionLine;
    }

    public static KT_QuestionnaireLines MapToEntity(
        this KT_QuestionBank questionBank,
        IList<QuestionCreationRequest> questionRequests,
        Guid projectId)
    {
        var request = questionRequests
            .FirstOrDefault(x => x.Id == questionBank.Id);

        var questionLine = new KT_QuestionnaireLines
        {
            KT_QuestionnaireLinesId = Guid.NewGuid(),
            KTR_Project = new EntityReference(KT_Project.EntityLogicalName, projectId),
            KTR_Module = request.Module?.Id != null
                ? new EntityReference(KT_Module.EntityLogicalName, request.Module.Id)
                : null,
            KTR_QuestionBank = new EntityReference(KT_Project.EntityLogicalName, questionBank.Id),
            KT_QuestionSortOrder = request.DisplayOrder,
            KT_QuestionVariableName = questionBank.KT_Name,
            KT_QuestionTitle = questionBank.KT_QuestionTitle,
            KT_QuestionText2 = questionBank.KT_DefaultQuestionText,
            KTR_ScripterNotes = questionBank.KTR_ScripterNotes,
            KTR_QuestionRationale = questionBank.KT_QuestionRationale,
            KTR_QuestionVersion = questionBank.KT_QuestionVersion,
            KTR_AnswerList = questionBank.KTR_AnswerList,
            KTR_IsDummyQuestion = questionBank.KT_IsDummyQuestion,
        };
        questionLine[KT_QuestionnaireLines.Fields.KT_StandardOrCustom] = new OptionSetValue((int)questionBank.KT_StandardOrCustom);
        questionLine[KT_QuestionnaireLines.Fields.KT_QuestionType] = new OptionSetValue((int)questionBank.KT_QuestionType);

        return questionLine;
    }
}
