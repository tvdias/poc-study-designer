using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Linq.Expressions;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Plugins.QuestionnaireLine
{
    public class QuestionnaireLineManagedListStatusSyncPostOperation : PluginBase
    {
        public QuestionnaireLineManagedListStatusSyncPostOperation()
            : base(typeof(QuestionnaireLineManagedListStatusSyncPostOperation)) { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            var tracing = localContext.TracingService;
            var context = localContext.PluginExecutionContext;
            var service = localContext.CurrentUserService;

            if (context.MessageName != nameof(ContextMessageEnum.Update) ||
                !context.InputParameters.Contains("Target") ||
                !(context.InputParameters["Target"] is Entity target) ||
                target.LogicalName != KT_QuestionnaireLines.EntityLogicalName)
            {
                return;
            }

            tracing.Trace("Entered QuestionnaireLineManagedListStatusSyncPostOperation plugin.");

            var qLine = target.ToEntity<KT_QuestionnaireLines>();

            // Only proceed if statecode is being updated
            if (!qLine.Attributes.Contains(KT_QuestionnaireLines.Fields.StateCode))
            {
                tracing.Trace("Statecode not in target — skipping.");
                return;
            }

            // Get statecode value (0 = Active, 1 = Inactive)
            int newStateCode = qLine.GetAttributeValue<OptionSetValue>(KT_QuestionnaireLines.Fields.StateCode)?.Value ?? -1;
            if (newStateCode != (int)KT_QuestionnaireLines_StateCode.Active && newStateCode != (int)KT_QuestionnaireLines_StateCode.Inactive)
            {
                tracing.Trace("Unexpected statecode value — skipping.");
                return;
            }

            int newStatusCode = newStateCode == (int)KT_QuestionnaireLines_StateCode.Active
                ? (int)KTR_QuestionnaireLinesHaRedList_StatusCode.Active
                : (int)KTR_QuestionnaireLinesHaRedList_StatusCode.Inactive;

            tracing.Trace($"Statecode changed. New state: {(newStateCode == (int)KT_QuestionnaireLines_StateCode.Active ? "Active" : "Inactive")}");

            // Get ID from context
            var questionnaireLineId = qLine.Id;
            tracing.Trace($"Processing related records for Questionnaire Line ID: {questionnaireLineId}");

            var relatedManagedListQliness = GetManagedListsLinkedToQuestionnaireLine(service, questionnaireLineId);

            foreach (var managedList in relatedManagedListQliness)
            {
                tracing.Trace($"Updating Managed List record ID: {managedList.Id}");

                var setStateRequest = new SetStateRequest
                {
                    EntityMoniker = managedList.ToEntityReference(),
                    State = new OptionSetValue(newStateCode),
                    Status = new OptionSetValue(newStatusCode)
                };

                service.Execute(setStateRequest);
            }

            tracing.Trace($"Successfully updated {relatedManagedListQliness.Count} related records.");
        }

        #region Dataverse Queries

        private List<Entity> GetManagedListsLinkedToQuestionnaireLine(IOrganizationService service, Guid questionnaireLineId)
        {
            var query = new QueryExpression(KTR_QuestionnaireLinesHaRedList.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLinesHaRedListId),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLine, ConditionOperator.Equal, questionnaireLineId)
                    }
                }
            };

            return service.RetrieveMultiple(query).Entities.ToList();
        }

        #endregion
    }
}
