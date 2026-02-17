using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Models;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Subset;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Plugins.QuestionnaireLine
{
    public class QuestionnaireLineAnswerListPostOperation : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.QuestionnaireLineAnswerListPostOperation";

        public QuestionnaireLineAnswerListPostOperation()
            : base(typeof(QuestionnaireLineAnswerListPostOperation)) { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new InvalidPluginExecutionException(nameof(localContext));
            }

            var tracingService = localContext.TracingService;
            var service = localContext.SystemUserService;
            var context = localContext.PluginExecutionContext;

            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity targetEntity))
            {
                tracingService.Trace("Target entity is missing.");
                return;
            }

            if (targetEntity.LogicalName != KTR_QuestionnaireLinesAnswerList.EntityLogicalName)
            {
                tracingService.Trace("The entity is not the expected KTR_QuestionAnswerList.");
                return;
            }

            EntityReference questionnaireLineRef = null;

            if (targetEntity.Attributes.Contains(KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine))
            {
                questionnaireLineRef = targetEntity.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine);
            }

            if (questionnaireLineRef == null && context.PreEntityImages.Contains("Image"))
            {
                var preImage = context.PreEntityImages["Image"];
                questionnaireLineRef = preImage.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine);
            }

            if (questionnaireLineRef == null)
            {
                tracingService.Trace("QuestionnaireLine is missing from both Target and PreImage.");
                return;
            }

            Entity question = null;
            try
            {
                question = service.Retrieve(KT_QuestionnaireLines.EntityLogicalName, questionnaireLineRef.Id, new ColumnSet(KT_QuestionnaireLines.Fields.KT_QuestionType));
            }
            catch (FaultException)
            {
                tracingService.Trace("Error retrieving QuestionnaireLine. Not found after deletion.");
            }

            if (question == null)
            {
                tracingService.Trace("Either QuestionnaireLine or AnswerId is missing or empty.");
                return;
            }

            var answers = GetQuestionnaireLinesAnswerLists(service, questionnaireLineRef.Id);
            var managedListsAsRows = GetManagedLists(service, questionnaireLineRef.Id, KTR_Location.Row);
            var managedListsAsColumns = GetManagedLists(service, questionnaireLineRef.Id, KTR_Location.Column);

            var htmlContent = HtmlGenerationHelper.GenerateAnswerListHtml(answers, managedListsAsRows, managedListsAsColumns);

            question[KT_QuestionnaireLines.Fields.KTR_AnswerList] = htmlContent;
            service.Update(question);

            //-------------Added a call to updated the SusetHTML in Study QL--------------
            try
            {
                // Update SubsetHtml on all StudyQuestionnaireLine records that reference this Questionnaire Line
                var subsetRepo = new SubsetRepository(service);

                var sqQuery = new QueryExpression(KTR_StudyQuestionnaireLine.EntityLogicalName)
                {
                    ColumnSet = new ColumnSet(KTR_StudyQuestionnaireLine.Fields.Id, KTR_StudyQuestionnaireLine.Fields.KTR_Study),
                    Criteria = new FilterExpression(LogicalOperator.And)
                };
                sqQuery.Criteria.AddCondition(KTR_StudyQuestionnaireLine.Fields.KTR_QuestionnaireLine, ConditionOperator.Equal, questionnaireLineRef.Id);

                var sqLines = service.RetrieveMultiple(sqQuery).Entities;
                if (sqLines != null && sqLines.Count > 0)
                {
                    foreach (var sq in sqLines)
                    {
                        var studyRef = sq.GetAttributeValue<EntityReference>(KTR_StudyQuestionnaireLine.Fields.KTR_Study);
                        if (studyRef == null || studyRef.Id == Guid.Empty)
                        {
                            continue;
                        }

                        // Pre-check study status to avoid FaultException
                        var study = service.Retrieve(KT_Study.EntityLogicalName, studyRef.Id, new ColumnSet(KT_Study.Fields.StatusCode));
                        var statusOs = study.GetAttributeValue<OptionSetValue>(KT_Study.Fields.StatusCode);
                        var statusCode = statusOs?.Value;

                        tracingService.Trace($"Study {studyRef.Id} StatusCode={statusCode}");

                        // Replace with your actual Draft code value (early-bound enum preferred)
                        const int DraftStatusCode = (int)KT_Study_StatusCode.Draft;
                        if (statusCode != DraftStatusCode)
                        {
                            tracingService.Trace($"Study {studyRef.Id} not Draft; skipping subset HTML update.");
                            continue;
                        }

                        // Get all subsets with location for this study, then filter to the current questionnaire line
                        var qlSubsetsByLine = subsetRepo.GetQuestionnaireLineSubsetsWithLocation(studyRef.Id);
                        var subsetsForLine = qlSubsetsByLine.ContainsKey(questionnaireLineRef.Id)
                            ? qlSubsetsByLine[questionnaireLineRef.Id]
                            : new List<QuestionnaireLineSubsetWithLocation>();

                        var subsetsAsRows = subsetsForLine
                            .Where(s => string.Equals(s.Location, "Row", StringComparison.Ordinal))
                            .ToList();

                        var subsetsAsColumns = subsetsForLine
                            .Where(s => string.Equals(s.Location, "Column", StringComparison.Ordinal))
                            .ToList();

                        var subsetHtml = HtmlGenerationHelper.GenerateAnswerSubsetListHtml(answers, subsetsAsRows, subsetsAsColumns);

                        var updateStudyQLine = new Entity(KTR_StudyQuestionnaireLine.EntityLogicalName, sq.Id)
                        {
                            [KTR_StudyQuestionnaireLine.Fields.KTR_SubsetHtml] = subsetHtml ?? string.Empty
                        };
                        service.Update(updateStudyQLine);
                    }

                    tracingService.Trace($"Updated ktr_subsethtml for {sqLines.Count} ktr_studyquestionnaireline record(s) (AnswerList change).");
                }
            }
            catch (FaultException ex)
            {
                // Do not break the original flow (study not draft, subset inactive, etc.)
                tracingService.Trace($"Skipped updating ktr_subsethtml due to fault: {ex.Message}");
            }

            //-------------------------------------------------------------------------------
        }

        private static List<KTR_QuestionnaireLinesAnswerList> GetQuestionnaireLinesAnswerLists(IOrganizationService service, Guid questionnaireLineId)
        {
            var columns = new ColumnSet
            {
                AllColumns = true
            };

            var answersQuery = new QueryExpression(KTR_QuestionnaireLinesAnswerList.EntityLogicalName)
            {
                ColumnSet = columns,
                Criteria =
                    {
                        Conditions =
                        {
                            new ConditionExpression(KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine, ConditionOperator.Equal, questionnaireLineId),
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
