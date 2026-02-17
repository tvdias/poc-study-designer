using Kantar.StudyDesignerLite.Plugins;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.ProductTemplate;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Project.CreateProject;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Mappers.QuestionnaireLine
{
    public static class QuestionnaireLineMapper
    {
        public static KT_QuestionnaireLines MapToEntity(
            this NewQuestionRequest request,
            Guid projectId)
        {
            var questionType = request.QuestionType
                .GetEnum<KT_QuestionType>();

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
            questionLine[KT_QuestionnaireLines.Fields.KT_QuestionType] = new OptionSetValue((int)questionType);

            return questionLine;
        }

        public static KT_QuestionnaireLines MapToEntity(
            this KT_QuestionBank questionBank,
            IList<ExistingQuestionRequest> questionRequests,
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

        public static KT_QuestionnaireLines MapToEntity(
            TemplateLineQuestionResult templateLineResult,
            KT_QuestionBank questionBank,
            Guid projectId,
            int sortOrder)
        {
            var questionLine = new KT_QuestionnaireLines
            {
                KT_QuestionnaireLinesId = Guid.NewGuid(),
                KTR_Project = new EntityReference(KT_Project.EntityLogicalName, projectId),
                KTR_Module = templateLineResult.ModuleId != null && templateLineResult.ModuleId != Guid.Empty
                    ? new EntityReference(KT_Module.EntityLogicalName, templateLineResult.ModuleId.GetValueOrDefault())
                    : null,
                KTR_QuestionBank = new EntityReference(KT_Project.EntityLogicalName, questionBank.Id),
                KT_QuestionSortOrder = sortOrder,
                KT_QuestionVariableName = questionBank.KT_Name,
                KT_QuestionTitle = questionBank.KT_QuestionTitle,
                KT_QuestionText2 = questionBank.KT_DefaultQuestionText,
                KTR_ScripterNotes = questionBank.KTR_ScripterNotes,
                KTR_QuestionRationale = questionBank.KT_QuestionRationale,
                KTR_QuestionVersion = questionBank.KT_QuestionVersion,
                KTR_AnswerList = questionBank.KTR_AnswerList,
                KTR_RowSortOrder = questionBank.KTR_RowSortOrder,
                KTR_ColumnSortOrder = questionBank.KTR_ColumnSortOrder,
                KTR_AnswerMin = questionBank.KTR_AnswerMin,
                KTR_AnswerMax = questionBank.KTR_AnswerMax,
                KTR_QuestionFormatDetails = questionBank.KTR_QuestionFormatDetails,
                KTR_CustomNotes = questionBank.KTR_CustomNotes,
                KTR_IsDummyQuestion = questionBank.KT_IsDummyQuestion
            };

            questionLine[KT_QuestionnaireLines.Fields.KT_StandardOrCustom] = new OptionSetValue((int)questionBank.KT_StandardOrCustom);
            questionLine[KT_QuestionnaireLines.Fields.KT_QuestionType] = new OptionSetValue((int)questionBank.KT_QuestionType);
            
            return questionLine;
        }

        public static KT_QuestionnaireLines MapToEntity(
            this KT_QuestionBank questionBank,
            Guid projectId,
            Guid? moduleId,
            int sortOrder)
        {
            var questionLine = new KT_QuestionnaireLines
            {
                KT_QuestionnaireLinesId = Guid.NewGuid(),
                KTR_Project = new EntityReference(KT_Project.EntityLogicalName, projectId),
                KTR_Module = moduleId != null && moduleId != Guid.Empty
                    ? new EntityReference(KT_Module.EntityLogicalName, moduleId.Value)
                    : null,
                KTR_QuestionBank = new EntityReference(KT_Project.EntityLogicalName, questionBank.Id),
                KT_QuestionSortOrder = sortOrder,
                KT_QuestionVariableName = questionBank.KT_Name,
                KT_QuestionTitle = questionBank.KT_QuestionTitle,
                KT_QuestionText2 = questionBank.KT_DefaultQuestionText,
                KTR_ScripterNotes = questionBank.KTR_ScripterNotes,
                KTR_QuestionRationale = questionBank.KT_QuestionRationale,
                KTR_QuestionVersion = questionBank.KT_QuestionVersion,
                KTR_AnswerList = questionBank.KTR_AnswerList,
                KTR_IsDummyQuestion = questionBank.KT_IsDummyQuestion,
                KTR_RowSortOrder = questionBank.KTR_RowSortOrder,
                KTR_ColumnSortOrder = questionBank.KTR_ColumnSortOrder,
                KTR_QuestionFormatDetails = questionBank.KTR_QuestionFormatDetails,
                KTR_AnswerMin = questionBank.KTR_AnswerMin,
                KTR_AnswerMax = questionBank.KTR_AnswerMax,
                KTR_CustomNotes = questionBank.KTR_CustomNotes,
            };
            questionLine[KT_QuestionnaireLines.Fields.KT_StandardOrCustom] = new OptionSetValue((int)questionBank.KT_StandardOrCustom);
            questionLine[KT_QuestionnaireLines.Fields.KT_QuestionType] = new OptionSetValue((int)questionBank.KT_QuestionType);

            return questionLine;
        }
    }
}
