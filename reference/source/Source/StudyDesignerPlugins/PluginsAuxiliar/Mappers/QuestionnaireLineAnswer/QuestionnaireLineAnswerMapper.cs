using Kantar.StudyDesignerLite.Plugins;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Project.CreateProject;
using Microsoft.Xrm.Sdk;
using System;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Mappers.QuestionnaireLineAnswer
{
    public static class QuestionnaireLineAnswerMapper
    {
        public static KTR_QuestionnaireLinesAnswerList MapToEntity(
            this KTR_QuestionAnswerList answer,
            Guid questionnaireLineId)
        {
            return new KTR_QuestionnaireLinesAnswerList
            {
                KTR_QuestionnaireLine = new EntityReference(KT_QuestionnaireLines.EntityLogicalName, questionnaireLineId),
                KTR_QuestionAnswer = new EntityReference(KTR_QuestionAnswerList.EntityLogicalName, answer.Id),
                KTR_QuestionBank = new EntityReference(KT_QuestionBank.EntityLogicalName, answer.KTR_KT_QuestionBank.Id),
                KTR_Name = answer.KTR_Name,
                KTR_AnswerCode = answer.KTR_Name,
                KTR_AnswerId = answer.KTR_AnswerId,
                KTR_DisplayOrder = answer.KTR_DisplayOrder,
                KTR_IsActive = answer.KTR_IsActive,
                KTR_AnswerType = answer.KTR_AnswerType,
                KTR_IsTranslatable = answer.KTR_IsTranslatable,
                KTR_IsOpen = answer.KTR_IsOpen,
                KTR_IsExclusive = answer.KTR_IsExclusive,
                KTR_IsFixed = answer.KTR_IsFixed,
                KTR_CustomProperty = answer.KTR_CustomProperty,
                KTR_SourceId = answer.KTR_SourceId,
                KTR_Version = answer.KTR_Version,
                KTR_EffectiveDate = answer.KTR_EffectiveDate,
                KTR_EndDate = answer.KTR_EndDate,
                KTR_AnswerText = answer.KTR_AnswerText,
            };
        }

        public static KTR_QuestionnaireLinesAnswerList MapToEntity(
            this AnswerRequest request,
            Guid questionnaireLineId)
        {
            return new KTR_QuestionnaireLinesAnswerList
            {
                KTR_QuestionnaireLine = new EntityReference(KT_QuestionnaireLines.EntityLogicalName, questionnaireLineId),
                KTR_Name = request.Name,
                KTR_AnswerText = request.Text,
                KTR_AnswerType = request.Location
                    .GetEnum<KTR_AnswerType>(),
            };
        }
    }
}
