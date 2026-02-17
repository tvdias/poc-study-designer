using System;
using System.Linq;
using System.Runtime.Remoting.Services;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Plugins.Common
{
    public class QuestionAnswerSortOrderAssignmentPreOperation : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.QuestionAnswerSortOrderAssignmentPreOperation";
        public QuestionAnswerSortOrderAssignmentPreOperation() : base(typeof(QuestionAnswerSortOrderAssignmentPreOperation)) { }

        private const string StateCodeField = "statecode";
        private const int StateCode_Active = 0;

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            ITracingService tracing = localContext.TracingService;
            IOrganizationService service = localContext.CurrentUserService;
            IPluginExecutionContext context = localContext.PluginExecutionContext;

            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity entity))
            {
                return;
            }

            var message = context.MessageName;
            var logicalName = entity.LogicalName;

            switch (logicalName)
            {
                case KTR_QuestionAnswerList.EntityLogicalName:
                    var questionAnswerList = entity.ToEntity<KTR_QuestionAnswerList>();

                    HandleSortOrder(
                        entity, service, tracing, context,
                        parentLookupField: KTR_QuestionAnswerList.Fields.KTR_KT_QuestionBank,
                        sortOrderField: KTR_QuestionAnswerList.Fields.KTR_DisplayOrder,
                        sortOrderValue: questionAnswerList.KTR_DisplayOrder,
                        entityLogicalName: KTR_QuestionAnswerList.EntityLogicalName,
                        applyDepthCheck: false
                    );
                    break;

                case KTR_QuestionnaireLinesAnswerList.EntityLogicalName:
                    var questionnaireLinesAnswerList = entity.ToEntity<KTR_QuestionnaireLinesAnswerList>();

                    HandleSortOrder(
                        entity, service, tracing, context,
                        parentLookupField: KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine,
                        sortOrderField: KTR_QuestionnaireLinesAnswerList.Fields.KTR_DisplayOrder,
                        sortOrderValue: questionnaireLinesAnswerList.KTR_DisplayOrder,
                        entityLogicalName: KTR_QuestionnaireLinesAnswerList.EntityLogicalName,
                        applyDepthCheck: true
                    );
                    break;
                case KTR_ProductConfigQuestion.EntityLogicalName:
                    var productConfigQuestion = entity.ToEntity<KTR_ProductConfigQuestion>();
                    HandleSortOrder(
                        entity, service, tracing, context,
                        parentLookupField: KTR_ProductConfigQuestion.Fields.KTR_Product,
                        sortOrderField: KTR_ProductConfigQuestion.Fields.KTR_DisplayOrder,
                        sortOrderValue: productConfigQuestion.KTR_DisplayOrder,
                        entityLogicalName: KTR_ProductConfigQuestion.EntityLogicalName,
                        applyDepthCheck: true
                    );
                    break;

                default:
                    tracing.Trace("Entity not handled: " + logicalName);
                    break;
            }
        }

        private void HandleSortOrder(
            Entity entity,
            IOrganizationService service,
            ITracingService tracing,
            IPluginExecutionContext context,
            string parentLookupField,
            string sortOrderField,
            int? sortOrderValue,
            string entityLogicalName,
            bool applyDepthCheck)
        {
            //Check if order is already set
            if (sortOrderValue != null)
            {
                tracing.Trace($"Order is already set to {sortOrderValue}. Skipping plugin.");
                return;
            }

            if (applyDepthCheck && context.Depth > 1)
            {
                tracing.Trace("Depth > 1. Skipping.");
                return;
            }

            // Create 
            if (!entity.Attributes.Contains(parentLookupField))
            {
                tracing.Trace($"Missing parent field '{parentLookupField}' in current entity.");
                return;
            }

            var parent = entity[parentLookupField] as EntityReference;
            if (parent == null)
            {
                return;
            }

            var fetchQuery = new QueryExpression(entityLogicalName)
            {
                ColumnSet = new ColumnSet(sortOrderField),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(parentLookupField, ConditionOperator.Equal, parent.Id),
                        new ConditionExpression(StateCodeField, ConditionOperator.Equal, StateCode_Active) //hard coded because we cant use one specific entity for early bound
                    }
                }
            };

            var siblings = service.RetrieveMultiple(fetchQuery);

            var maxOrder = siblings.Entities.Count > 0
                ? siblings.Entities.Max(e => e.Contains(sortOrderField) ? (int)e[sortOrderField] : 0)
                : -1;

            entity[sortOrderField] = maxOrder + 1;
            tracing.Trace($"Set {sortOrderField} = {maxOrder + 1} for {entityLogicalName} ID: {entity.Id}");
        }
    }
}
