using System;
using System.Collections.Generic;
using System.Linq;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Plugins.QuestionnaireLine
{
    public class QuestionnaireLineRegenerateXMLandHTMLCustomAPI : PluginBase
    {
        private static readonly string PluginName = "Kantar.StudyDesignerLite.Plugins.QuestionnaireLineRegenerateXMLandHTMLCustomAPI";

        public QuestionnaireLineRegenerateXMLandHTMLCustomAPI()
               : base(typeof(QuestionnaireLineRegenerateXMLandHTMLCustomAPI))
        {
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            ITracingService tracingService = localPluginContext.TracingService;
            IPluginExecutionContext context = localPluginContext.PluginExecutionContext;
            IOrganizationService service = localPluginContext.SystemUserService;

            var questionnaireLineId = context.GetInputParameter<Guid>("questionnaireLineId");

            //Get questionnaire line and answer list
            var questionToUpdate = (KT_QuestionnaireLines)service.Retrieve(KT_QuestionnaireLines.EntityLogicalName, questionnaireLineId, new ColumnSet(KT_QuestionnaireLines.Fields.KT_QuestionType));
            if (questionToUpdate == null)
            {
                tracingService.Trace("QuestionnaireLineId is missing or empty.");
                throw new InvalidPluginExecutionException($"An error occurred executing {PluginName}. Question not found");
            }

            var answers = GetQuestionnaireLinesAnswerLists(service, questionnaireLineId);
            tracingService.Trace($"Found {answers.Count} answers.");
            var managedListsAsRows = GetManagedLists(service, questionnaireLineId, KTR_Location.Row);
            tracingService.Trace($"Found {managedListsAsRows.Count} managed List as Rows.");
            var managedListsAsColumns = GetManagedLists(service, questionnaireLineId, KTR_Location.Column);

            //Update Answer List HTML
            questionToUpdate.KTR_AnswerList = HtmlGenerationHelper.GenerateAnswerListHtml(answers, managedListsAsRows, managedListsAsColumns);

            //Update Scripter Notes Output
            var columnSet = new ColumnSet(HtmlGenerationHelper.GetFieldsToInclude().Values.ToArray());
            var questionWithFields = service.Retrieve(KT_QuestionnaireLines.EntityLogicalName, questionnaireLineId, columnSet);
            questionToUpdate.KTR_ScripterNotesOutput = HtmlGenerationHelper.GenerateScripterNotesHtml(questionWithFields);

            service.Update(questionToUpdate);
        }

        private static List<KTR_QuestionnaireLinesAnswerList> GetQuestionnaireLinesAnswerLists(IOrganizationService service, Guid questionnaireLineRefId)
        {
            var columns = new ColumnSet();
            columns.AllColumns = true;

            var answersQuery = new QueryExpression(KTR_QuestionnaireLinesAnswerList.EntityLogicalName)
            {
                ColumnSet = columns,
                Criteria =
                    {
                        Conditions =
                        {
                            new ConditionExpression(KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine, ConditionOperator.Equal, questionnaireLineRefId),
                            new ConditionExpression(KTR_QuestionnaireLinesAnswerList.Fields.StateCode, ConditionOperator.Equal, (int)KTR_QuestionnaireLinesAnswerList_StateCode.Active)
                        }
                    }
            };

            var answers = service.RetrieveMultiple(answersQuery).Entities.Select(e => e.ToEntity<KTR_QuestionnaireLinesAnswerList>()).ToList();
            return answers;
        }

        private static List<KTR_ManagedList> GetManagedLists(IOrganizationService service, Guid questionnaireLineId, KTR_Location location)
        {
            var columns = new ColumnSet
            {
                AllColumns = true
            };

            var query = new QueryExpression(KTR_ManagedList.EntityLogicalName)
            {
                ColumnSet = columns,
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_ManagedList.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ManagedList_StatusCode.Active)
                    }
                }
            };

            query.AddLink(
                KTR_QuestionnaireLinesHaRedList.EntityLogicalName,
                KTR_ManagedList.Fields.KTR_ManagedListId,
                KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList,
                JoinOperator.Inner)
                .LinkCriteria.Conditions.AddRange(new[]
                {
                    new ConditionExpression(KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLine, ConditionOperator.Equal, questionnaireLineId),
                    new ConditionExpression(KTR_QuestionnaireLinesHaRedList.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_QuestionnaireLinesHaRedList_StatusCode.Active),
                    new ConditionExpression(KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location, ConditionOperator.Equal, (int)location)
                });

            var managedLists = service.RetrieveMultiple(query).Entities
                .Select(e => e.ToEntity<KTR_ManagedList>())
                .ToList();

            return managedLists;
        }
    }
}
