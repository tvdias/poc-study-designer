namespace Kantar.StudyDesignerLite.Plugins.ManagedListEntity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Services;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    public class ReorderManagedListEntityByMLPostOperation : PluginBase
    {
        public ReorderManagedListEntityByMLPostOperation()
          : base(typeof(ReorderManagedListEntityByMLPostOperation))
        {
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            var context = localPluginContext.PluginExecutionContext;
            var tracing = localPluginContext.TracingService;
            var service = localPluginContext.CurrentUserService;

            tracing.Trace($"Reordering ManagedListEntities on {nameof(ContextMessageEnum.Update)}.");

            KTR_ManagedListEntity managedListEntity = null;
            if (context.MessageName == nameof(ContextMessageEnum.Create))
            {
                // Validate context and input parameters
                if (!context.InputParameters.Contains("Target") ||
                    !(context.InputParameters["Target"] is Entity target) ||
                    target.LogicalName != KTR_ManagedListEntity.EntityLogicalName)
                {
                    return;
                }

                managedListEntity = target.ToEntity<KTR_ManagedListEntity>();
            }
            else if (context.MessageName == nameof(ContextMessageEnum.Update))
            {
                // Validate context and input parameters
                if (!context.InputParameters.Contains("Target") ||
                    !(context.InputParameters["Target"] is Entity target) ||
                    target.LogicalName != KTR_ManagedListEntity.EntityLogicalName)
                {
                    return;
                }

                // PreImage must include: KTR_ManagedList, statecode, statuscode
                if (!context.PreEntityImages.TryGetValue("PreImage", out var preImage))
                {
                    return;
                }

                var oldStatus = preImage.GetAttributeValue<OptionSetValue>("statuscode")?.Value;
                var newStatus = target.GetAttributeValue<OptionSetValue>("statuscode")?.Value;

                if (oldStatus == newStatus)
                {
                    tracing.Trace("Status did not change, skipping reorder.");
                    return;
                }

                managedListEntity = preImage.ToEntity<KTR_ManagedListEntity>();
            }
            else if (context.MessageName == nameof(ContextMessageEnum.Delete))
            {
                // PreImage must include: KTR_ManagedList
                if (!context.PreEntityImages.TryGetValue("PreImage", out var preImage))
                {
                    tracing.Trace("No PreImage found.");
                    return;
                }

                managedListEntity = preImage.ToEntity<KTR_ManagedListEntity>();
            }

            if (managedListEntity?.KTR_ManagedList == null)
            {
                tracing.Trace("ManagedList reference is null.");
                return;
            }

            var managedListEntitiesToReorder = GetManagedListEntitiesToReorder(
                service,
                managedListEntity.KTR_ManagedList.Id);
            tracing.Trace("GetManagedListEntitiesToReorder executed.");

            ReorderManagedListEntities(service, tracing, managedListEntitiesToReorder);
        }

        private void ReorderManagedListEntities(
            IOrganizationService service,
            ITracingService tracingService,
            IList<KTR_ManagedListEntity> managedListEntities)
        {
            if (managedListEntities == null || managedListEntities.Count == 0)
            {
                tracingService.Trace("No Managed List Entities to reorder.");
                return;
            }

            var reorderService = new ReorderService(
                service,
                tracingService,
                KTR_ManagedListEntity.EntityLogicalName,
                KTR_ManagedListEntity.Fields.KTR_DisplayOrder);

            var ids = managedListEntities
                .OrderBy(ql => ql.KTR_DisplayOrder)
                .ThenBy(ql => ql.CreatedOn)
                .Select(ql => ql.Id)
                .ToList();

            var success = reorderService.ReorderEntities(ids);

            if (!success)
            {
                tracingService.Trace("Reordering Managed List Entities failed.");
            }
        }

        private List<KTR_ManagedListEntity> GetManagedListEntitiesToReorder(
            IOrganizationService service,
            Guid managedListId)
        {
            var query = new QueryExpression
            {
                EntityName = KTR_ManagedListEntity.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    KTR_ManagedListEntity.Fields.Id,
                    KTR_ManagedListEntity.Fields.KTR_ManagedList,
                    KTR_ManagedListEntity.Fields.KTR_DisplayOrder,
                    KTR_ManagedListEntity.Fields.CreatedOn),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_ManagedListEntity.Fields.KTR_ManagedList, ConditionOperator.Equal, managedListId),
                        new ConditionExpression(KTR_ManagedListEntity.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ManagedListEntity_StatusCode.Active)

                    }
                },
                NoLock = true
            };

            return service.RetrieveMultiple(query).Entities
                .Select(x => x.ToEntity<KTR_ManagedListEntity>())
                .ToList();
        }
    }
}
