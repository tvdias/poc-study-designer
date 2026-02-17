using System;
using System.Linq;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Plugins.Common
{
    public class QuestionAnswerSortOrderAssignmentPostOperation : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.QuestionAnswerSortOrderAssignmentPostOperation";
        public QuestionAnswerSortOrderAssignmentPostOperation() : base(typeof(QuestionAnswerSortOrderAssignmentPostOperation)) { }

        private const string StateCodeField = "statecode";
        private const int StateCode_Active = 0;
        private const int StateCode_Inactive = 1;

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            var tracing = localContext.TracingService;
            var service = localContext.CurrentUserService;
            var context = localContext.PluginExecutionContext;

            if (context.MessageName != nameof(ContextMessageEnum.Update))
            {
                return;
            }
            if (context.Depth > 1)
            {
                tracing.Trace("Depth > 1. Skipping.");
                return;
            }
            if (!context.InputParameters.TryGetValue("Target", out var targetObj) || !(targetObj is Entity target))
            {
                return;
            }
            if (!target.Attributes.Contains(StateCodeField)) //hard coded for reusability
            {
                return;
            }

            var newState = (OptionSetValue)target[StateCodeField];

            // Handle only supported entities
            string parentField, sortOrderField, primaryIdField;
            switch (target.LogicalName)
            {
                case KTR_QuestionAnswerList.EntityLogicalName:
                    parentField = KTR_QuestionAnswerList.Fields.KTR_KT_QuestionBank;
                    sortOrderField = KTR_QuestionAnswerList.Fields.KTR_DisplayOrder;
                    primaryIdField = KTR_QuestionAnswerList.Fields.KTR_QuestionAnswerListId;
                    break;

                case KTR_QuestionnaireLinesAnswerList.EntityLogicalName:
                    parentField = KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine;
                    sortOrderField = KTR_QuestionnaireLinesAnswerList.Fields.KTR_DisplayOrder;
                    primaryIdField = KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLinesAnswerListId;
                    break;

                case KTR_ProductConfigQuestion.EntityLogicalName:
                    parentField = KTR_ProductConfigQuestion.Fields.KTR_Product;
                    sortOrderField = KTR_ProductConfigQuestion.Fields.KTR_DisplayOrder;
                    primaryIdField = KTR_ProductConfigQuestion.Fields.KTR_ProductConfigQuestionId;
                    break;

                default:
                    tracing.Trace("Entity not handled.");
                    return;
            }

            if (newState.Value == StateCode_Inactive)
            {
                // Deactivation
                if (!context.PreEntityImages.TryGetValue("PreImage", out var preImage))
                {
                    tracing.Trace("No PreImage found.");
                    return;
                }

                HandleDeactivationReorder(service, tracing, preImage, target.LogicalName, parentField, sortOrderField);
            }
            else if (newState.Value == StateCode_Active)
            {
                // Reactivation
                tracing.Trace("Handling Reactivation in PostOperation.");
                HandleReactivationReorder(service, tracing, target, target.LogicalName, parentField, sortOrderField, primaryIdField);

            }
        }

        private void HandleDeactivationReorder(
            IOrganizationService service,
            ITracingService tracing,
            Entity preImage,
            string entityLogicalName,
            string parentField,
            string sortOrderField)
        {
            if (!preImage.Attributes.Contains(parentField) || !preImage.Attributes.Contains(sortOrderField))
            {
                tracing.Trace($"Preimage missing '{parentField}' or '{sortOrderField}'.");
                return;
            }

            var parentRef = preImage[parentField] as EntityReference;
            var oldSortOrder = (int)preImage[sortOrderField];

            var query = new QueryExpression(entityLogicalName)
            {
                ColumnSet = new ColumnSet(sortOrderField),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(parentField, ConditionOperator.Equal, parentRef.Id),
                        new ConditionExpression(StateCodeField, ConditionOperator.Equal, StateCode_Active),
                        new ConditionExpression(sortOrderField, ConditionOperator.GreaterEqual, oldSortOrder)
                    }
                }
            };

            var siblings = service.RetrieveMultiple(query);
            if (!siblings.Entities.Any())
            {
                tracing.Trace("No siblings to reorder.");
                return;
            }

            var batch = new ExecuteMultipleRequest
            {
                Requests = new OrganizationRequestCollection(),
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                    ReturnResponses = false
                }
            };

            foreach (var record in siblings.Entities)
            {
                record[sortOrderField] = oldSortOrder++;

                batch.Requests.Add(new UpdateRequest { Target = record });
                tracing.Trace($"Queued update for ID: {record.Id}, {sortOrderField}: {oldSortOrder} → {oldSortOrder - 1}");
            }

            service.Execute(batch);
            tracing.Trace($"Reordered {batch.Requests.Count} records for entity '{entityLogicalName}'.");
        }

        private void HandleReactivationReorder(
            IOrganizationService service,
            ITracingService tracing,
            Entity target,
            string entityLogicalName,
            string parentField,
            string sortOrderField,
            string primaryIdField)
        {
            // Retrieve full record because parent field may not be in the Target
            var fullRecord = service.Retrieve(entityLogicalName, target.Id, new ColumnSet(parentField));
            if (!fullRecord.Attributes.Contains(parentField))
            {
                tracing.Trace($"Parent field '{parentField}' not found in retrieved record.");
                return;
            }

            var parentRef = fullRecord[parentField] as EntityReference;

            var query = new QueryExpression(entityLogicalName)
            {
                ColumnSet = new ColumnSet(sortOrderField),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(parentField, ConditionOperator.Equal, parentRef.Id),
                        new ConditionExpression(StateCodeField, ConditionOperator.Equal, StateCode_Active),
                        new ConditionExpression(primaryIdField, ConditionOperator.NotEqual, target.Id)
                    }
                }
            };

            var siblings = service.RetrieveMultiple(query);

            int maxOrder = siblings.Entities.Count > 0
                ? siblings.Entities.Max(e => e.Contains(sortOrderField) ? (int)e[sortOrderField] : 0)
                : -1;

            var update = new Entity(entityLogicalName)
            {
                Id = target.Id,
                [sortOrderField] = maxOrder + 1
            };

            service.Update(update);
            tracing.Trace($"Moved reactivated record {target.Id} to bottom with {sortOrderField} = {maxOrder + 1}");
        }

    }
}
