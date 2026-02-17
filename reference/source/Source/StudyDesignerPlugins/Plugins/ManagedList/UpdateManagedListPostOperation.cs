using System;
using System.Collections.Generic;
using System.Linq;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Plugins.ManagedList
{
    /// <summary>
    /// Plugin to synchronize ManagedListEntity records status when ManagedList status changes
    /// </summary>
    public class UpdateManagedListPostOperation : PluginBase
    {
        public UpdateManagedListPostOperation()
            : base(typeof(UpdateManagedListPostOperation)) { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            var tracing = localContext.TracingService;
            var context = localContext.PluginExecutionContext;
            var service = localContext.CurrentUserService;

            // Validate context and input parameters
            if (context.MessageName != nameof(ContextMessageEnum.Update) ||
                !context.InputParameters.Contains("Target") ||
                !(context.InputParameters["Target"] is Entity target) ||
                target.LogicalName != KTR_ManagedList.EntityLogicalName)
            {
                return;
            }

            tracing.Trace("Entered UpdateManagedListPostOperation plugin.");

            var managedList = target.ToEntity<KTR_ManagedList>();

            // Only proceed if statecode is being updated
            if (!managedList.Attributes.Contains(KTR_ManagedList.Fields.StateCode))
            {
                tracing.Trace("StateCode not in target — skipping.");
                return;
            }

            // Get the new state code value
            var newStateCodeValue = managedList.GetAttributeValue<OptionSetValue>(KTR_ManagedList.Fields.StateCode)?.Value ?? -1;
            if (newStateCodeValue != (int)KTR_ManagedList_StateCode.Active && newStateCodeValue != (int)KTR_ManagedList_StateCode.Inactive)
            {
                tracing.Trace("Unexpected statecode value — skipping.");
                return;
            }

            var newStateCode = (KTR_ManagedList_StateCode)newStateCodeValue;
            var newStatusCode = newStateCode == KTR_ManagedList_StateCode.Active
                ? KTR_ManagedListEntity_StatusCode.Active
                : KTR_ManagedListEntity_StatusCode.Inactive;

            tracing.Trace($"ManagedList StateCode changed to: {newStateCode}. Updating related ManagedListEntity records.");

            // Get the ManagedList ID
            var managedListId = managedList.Id;
            if (managedListId == Guid.Empty)
            {
                // Try to get ID from context if not in target
                managedListId = context.PrimaryEntityId;
            }

            if (managedListId == Guid.Empty)
            {
                tracing.Trace("Could not determine ManagedList ID — skipping.");
                return;
            }

            tracing.Trace($"Processing related ManagedListEntity records for ManagedList ID: {managedListId}");

            // Get all ManagedListEntity records related to this ManagedList
            var relatedManagedListEntities = GetRelatedManagedListEntities(service, managedListId);

            tracing.Trace($"Found {relatedManagedListEntities.Count} related ManagedListEntity records to update.");

            // Update each related ManagedListEntity record
            foreach (var entityId in relatedManagedListEntities)
            {
                try
                {
                    var updateEntity = new KTR_ManagedListEntity
                    {
                        Id = entityId,
                        StateCode = newStateCode == KTR_ManagedList_StateCode.Active
                            ? KTR_ManagedListEntity_StateCode.Active
                            : KTR_ManagedListEntity_StateCode.Inactive,
                        StatusCode = newStatusCode
                    };

                    service.Update(updateEntity);
                    tracing.Trace($"Successfully updated ManagedListEntity {entityId} to {newStateCode} status.");
                }
                catch (Exception ex)
                {
                    tracing.Trace($"Error updating ManagedListEntity {entityId}: {ex.Message}");
                    // Continue processing other records even if one fails
                }
            }

            tracing.Trace("UpdateManagedListPostOperation completed successfully.");
        }

        /// <summary>
        /// Gets all ManagedListEntity records related to the specified ManagedList
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="managedListId">ID of the ManagedList</param>
        /// <returns>List of ManagedListEntity IDs</returns>
        private List<Guid> GetRelatedManagedListEntities(IOrganizationService service, Guid managedListId)
        {
            var query = new QueryExpression(KTR_ManagedListEntity.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KTR_ManagedListEntity.Fields.KTR_ManagedListEntityId),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            KTR_ManagedListEntity.Fields.KTR_ManagedList,
                            ConditionOperator.Equal,
                            managedListId)
                    }
                }
            };

            var results = service.RetrieveMultiple(query);
            return results.Entities.Select(e => e.Id).ToList();
        }
    }
}
