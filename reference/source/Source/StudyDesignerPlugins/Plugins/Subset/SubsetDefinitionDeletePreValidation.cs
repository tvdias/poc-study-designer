using System;
using System.Collections.Generic;
using System.Linq;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Plugins.SubsetDefinition
{
    /// <summary>
    /// Plugin to upate the userFullList in QuestionSubset when SubsetDefinition is deleted
    /// </summary>
    public class SubsetDefinitionDeletePreValidation : PluginBase
    {
        public SubsetDefinitionDeletePreValidation()
            : base(typeof(SubsetDefinitionDeletePreValidation)) { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            var tracing = localContext.TracingService;
            var context = localContext.PluginExecutionContext;
            var service = localContext.CurrentUserService;

            // Validate context and input parameters
            if (context.MessageName != nameof(ContextMessageEnum.Delete) ||
                !context.InputParameters.Contains("Target") ||
                !(context.InputParameters["Target"] is EntityReference targetRef) ||
                targetRef.LogicalName != KTR_SubsetDefinition.EntityLogicalName)
            {
                return;
            }

            tracing.Trace("Entered UnsetSubsetDefinitionAndUseFullListOnDelete plugin.");

            var deletedSubsetDefinitionId = targetRef.Id;
            if (deletedSubsetDefinitionId == Guid.Empty)
            {
                tracing.Trace("Target EntityReference ID is empty — skipping.");
                return;
            }

            tracing.Trace($"Handling Delete for ktr_subsetdefinition id={deletedSubsetDefinitionId}");
            var relatedIds = GetRelatedQuestionnaireLineSubsetIds(service, deletedSubsetDefinitionId, tracing);
            if (relatedIds.Count == 0)
            {
                tracing.Trace("No related subset entities to update.");
                return;
            }
            UpdateQuestionnaireLineSubsets(service, relatedIds, tracing);
            tracing.Trace("UnsetSubsetDefinitionAndUseFullListOnDelete completed.");
        }
        private List<Guid> GetRelatedQuestionnaireLineSubsetIds(IOrganizationService service, Guid subsetDefinitionId, ITracingService tracing = null)
        {
            tracing?.Trace($"Querying ktr_questionnairelinesubset for subset definition id={subsetDefinitionId}");

            var query = new QueryExpression(KTR_QuestionnaireLineSubset.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(false) // no attributes required; Id is returned by default
            };

            query.Criteria.AddCondition(KTR_QuestionnaireLineSubset.Fields.KTR_SubsetDefinitionId, ConditionOperator.Equal, subsetDefinitionId);
            var results = service.RetrieveMultiple(query);
            var ids = results.Entities.Select(e => e.Id).ToList();
            tracing?.Trace($"Query returned {ids.Count} ktr_questionnairelinesubset records.");

            return ids;
        }
        private void UpdateQuestionnaireLineSubsets(IOrganizationService service, IEnumerable<Guid> ids, ITracingService tracing = null)
        {
            tracing?.Trace("Starting update of questionnaire line subsets.");

            foreach (var id in ids)
            {
                var updateEntity = new KTR_QuestionnaireLineSubset
                {
                    Id = id,
                    KTR_SubsetDefinitionId = null,
                    KTR_UsesFullList = true
                };

                service.Update(updateEntity);
                tracing?.Trace($"Updated subset {id} (cleared subsetdefinition & set usesfulllist=true)");
            }
        }
    }
}
