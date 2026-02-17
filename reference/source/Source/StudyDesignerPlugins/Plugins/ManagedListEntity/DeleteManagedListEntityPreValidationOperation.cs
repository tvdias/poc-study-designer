using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Web.UI.WebControls;
using Kantar.StudyDesignerLite.Plugins.ManagedList;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Plugins.ManagedListEntity
{
    public class DeleteManagedListEntityPreValidationOperation : PluginBase
    {
        public DeleteManagedListEntityPreValidationOperation()
            : base(typeof(DeleteManagedListEntityPreValidationOperation)) { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            var tracing = localContext.TracingService;
            var context = localContext.PluginExecutionContext;
            var service = localContext.CurrentUserService;

            if (context.MessageName != nameof(ContextMessageEnum.Delete) || !context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is EntityReference targetRef))
            {
                return;
            }

            if (targetRef.LogicalName != KTR_ManagedListEntity.EntityLogicalName)
            {
                return;
            }

            // fetch the full entity manually because we only get an ID in Delete
            var managedListEntity = service.Retrieve(
                KTR_ManagedListEntity.EntityLogicalName,
                targetRef.Id,
                new ColumnSet(
                    KTR_ManagedListEntity.Fields.KTR_ManagedList,
                    KTR_ManagedListEntity.Fields.KTR_EverInSnapshot)
            ).ToEntity<KTR_ManagedListEntity>();

            tracing.Trace($"Validating deletion of Managed List Entity with ID: {managedListEntity.Id}");

            // Check 1: If related Managed list has associated with ktr_questionnairelinesharedlist in active status
            var activeQuestionnaireLineAssociations = GetActiveQuestionnaireLineAssociations(service, managedListEntity);
            if (activeQuestionnaireLineAssociations.Count > 0)
            {
                throw new InvalidPluginExecutionException($"Cannot delete: Currently associated with {activeQuestionnaireLineAssociations.Count} question(s) or referenced in snapshot history.");
            }

            // Check 2: Check if ktr_everinsnapshot = true in the target record
            if (managedListEntity.KTR_EverInSnapshot == true)
            {
                throw new InvalidPluginExecutionException("Cannot delete: Currently associated with [1] question(s) or referenced in snapshot history.");
            }

            //Check 3: Check if this managed list entity is associated with any subset definitions
            var subsetCount = GetSubsetsWithThisManagedListEntity(service, managedListEntity);
            if (subsetCount > 0)
            {
                throw new InvalidPluginExecutionException($"Cannot delete: Currently associated with {subsetCount} subset definitions(s).");
            }

            tracing.Trace("All validations passed. Deletion allowed.");
        }

        // Check if the managed list entity has active questionnaire line associations
        private List<Entity> GetActiveQuestionnaireLineAssociations(IOrganizationService service, KTR_ManagedListEntity managedListEntity)
        {
            if (managedListEntity.KTR_ManagedList == null)
            {
                return new List<Entity>();
            }

            var query = new QueryExpression(KTR_QuestionnaireLinesHaRedList.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLinesHaRedListId),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList, ConditionOperator.Equal, managedListEntity.KTR_ManagedList.Id),
                    }
                }
            };

            var results = service.RetrieveMultiple(query);
            return results.Entities.ToList();
        }

        private int GetSubsetsWithThisManagedListEntity(IOrganizationService service, KTR_ManagedListEntity managedListEntity)
        {
            var query = new QueryExpression(KTR_SubsetEntities.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KTR_SubsetEntities.Fields.KTR_SubsetEntitiesId),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_SubsetEntities.Fields.KTR_ManagedListEntity, ConditionOperator.Equal, managedListEntity.Id),
                    }
                }
            };
            var results = service.RetrieveMultiple(query);
            return results.Entities.Count();
        }
    }
}
