namespace Kantar.StudyDesignerLite.Plugins.QuestionnaireLineManagedList
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using Kantar.StudyDesignerLite.Plugins.QuestionnaireLine;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.ManagedList;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    public class QuestionnaireLineManagedListPostOperation : PluginBase
    {
        public QuestionnaireLineManagedListPostOperation()
           : base(typeof(QuestionnaireLineManagedListPostOperation)) { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new InvalidPluginExecutionException(nameof(localContext));
            }

            var tracingService = localContext.TracingService;
            var service = localContext.SystemUserService;
            var context = localContext.PluginExecutionContext;

            // Target is available for Create/Update; for Delete use PreImage
            Entity sourceEntity = null;
            if (string.Equals(context.MessageName, nameof(ContextMessageEnum.Delete), StringComparison.Ordinal))
            {
                if (context.PreEntityImages.Contains("PreImage"))
                {
                    sourceEntity = context.PreEntityImages["PreImage"];
                    tracingService.Trace("Using PreImage for Delete operation.");
                }
            }
            else
            {
                if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity targetEntity))
                {
                    tracingService.Trace("Target entity is missing.");
                    return;
                }
                sourceEntity = targetEntity;
            }

            if (sourceEntity == null)
            {
                tracingService.Trace("Source entity is missing.");
                return;
            }

            if (sourceEntity.LogicalName != KTR_QuestionnaireLinesHaRedList.EntityLogicalName)
            {
                tracingService.Trace("The entity is not the expected KTR_QuestionnaireLinesHaRedList.");
                return;
            }

            EntityReference questionnaireLineRef = null;

            if (sourceEntity.Attributes.Contains(KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLine))
            {
                questionnaireLineRef = sourceEntity.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLine);
            }

            if (questionnaireLineRef == null && context.PreEntityImages.Contains("PreImage"))
            {
                var preImage = context.PreEntityImages["PreImage"];
                questionnaireLineRef = preImage.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLine);
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
                tracingService.Trace("Either QuestionnaireLine or ML is missing or empty.");
                return;
            }

            var answers = GetQuestionnaireLinesAnswerLists(service, questionnaireLineRef.Id);
            var managedListsAsRows = GetManagedLists(service, questionnaireLineRef.Id, KTR_Location.Row);
            var managedListsAsColumns = GetManagedLists(service, questionnaireLineRef.Id, KTR_Location.Column);

            var htmlContent = HtmlGenerationHelper.GenerateAnswerListHtml(answers, managedListsAsRows, managedListsAsColumns);

            question[KT_QuestionnaireLines.Fields.KTR_AnswerList] = htmlContent;
            service.Update(question);

            // Rebuild Managed Lists HTML on the Project impacted by this create/update/delete
            Guid? projectId = null;
            if (sourceEntity.Attributes.Contains(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ProjectId))
            {
                var projectRef = sourceEntity.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ProjectId);
                projectId = projectRef?.Id;
            }
            if (!projectId.HasValue && context.PreEntityImages.Contains("PreImage"))
            {
                var preImage = context.PreEntityImages["PreImage"];
                var projectRefPre = preImage.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ProjectId);
                projectId = projectRefPre?.Id;
            }

            if (projectId.HasValue)
            {
                var mlRepo = new ManagedListRepository(service);
                var projectHtml = HtmlGenerationHelper.RebuildProjectManagedListsHtml(mlRepo, tracingService, projectId.Value);

                var updateProject = new Entity(KT_Project.EntityLogicalName, projectId.Value)
                {
                    [KT_Project.Fields.KTR_ManagedListsHtml] = projectHtml ?? string.Empty
                };
                service.Update(updateProject);
            }
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
