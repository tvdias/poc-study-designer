using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.ManagedLists;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.ManagedList;

namespace Kantar.StudyDesignerLite.Plugins.ManagedListEntity
{
    public class CreateUpdateDeleteManagedListEntityPostOperation : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.ManagedListEntity.CreateUpdateDeleteManagedListEntityPostOperation";

        public CreateUpdateDeleteManagedListEntityPostOperation() : base(typeof(CreateUpdateDeleteManagedListEntityPostOperation)) { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {/*
            if (localContext == null) { throw new InvalidPluginExecutionException(nameof(localContext)); }
            var tracing = localContext.TracingService;
            var service = localContext.SystemUserService;
            var context = localContext.PluginExecutionContext;

            var start = DateTime.UtcNow;
            tracing.Trace($"{PluginName} START Message={context.MessageName} Stage={context.Stage} Depth={context.Depth}");

            if (!context.InputParameters.Contains("Target"))
            {
                tracing.Trace("InputParameters does not contain Target – exiting.");
                return;
            }

            var targetEntity = context.InputParameters["Target"] as Entity;
            var targetRef = targetEntity == null ? context.InputParameters["Target"] as EntityReference : null;
            var logicalName = targetEntity?.LogicalName ?? targetRef?.LogicalName;
            var id = targetEntity?.Id ?? targetRef?.Id ?? Guid.Empty;

            tracing.Trace($"Resolved Target LogicalName={logicalName} Id={id}");
            if (logicalName != KTR_ManagedListEntity.EntityLogicalName)
            {
                tracing.Trace("Not ktr_managedlistentity – exiting.");
                return;
            }

            if (context.MessageName == nameof(ContextMessageEnum.Delete))
            {
                if (context.Stage != (int)ContextStageEnum.PostOperation)
                {
                    tracing.Trace($"Delete received at unexpected stage {context.Stage}. Register only PostOperation with PreImage.");
                    return;
                }

                var preImage = context.PreEntityImages.Contains("PreImage") ? context.PreEntityImages["PreImage"] : null;
                if (preImage == null)
                {
                    tracing.Trace("Delete PostOperation: PreImage missing – ensure step registered with PreImage including ktr_managedlist.");
                    return;
                }

                var managedListRef = preImage.GetAttributeValue<EntityReference>(KTR_ManagedListEntity.Fields.KTR_ManagedList);
                if (managedListRef == null)
                {
                    tracing.Trace("Delete PostOperation: PreImage lacks ktr_managedlist – cannot rebuild.");
                    return;
                }

                tracing.Trace($"Delete PostOperation: ManagedList={managedListRef.Id}. Retrieving for project reference.");
                Entity managedList;
                try
                {
                    managedList = service.Retrieve(
                        KTR_ManagedList.EntityLogicalName,
                        managedListRef.Id,
                        new ColumnSet(KTR_ManagedList.Fields.KTR_Project, KTR_ManagedList.Fields.KTR_Name));
                }
                catch (Exception ex)
                {
                    tracing.Trace($"Retrieve ManagedList failed: {ex.Message}");
                    return;
                }

                var projectRef = managedList.GetAttributeValue<EntityReference>(KTR_ManagedList.Fields.KTR_Project);
                if (projectRef == null)
                {
                    tracing.Trace("ManagedList has no project – abort.");
                    return;
                }

                tracing.Trace($"Rebuilding managed lists HTML for Project={projectRef.Id} after Delete of ManagedListEntity={id}");
                TryRebuild(service, tracing, projectRef.Id, start);
                return;
            }

            // CREATE / UPDATE (PostOperation expected)
            EntityReference managedListReference = null;

            if (targetEntity != null)
            {
                managedListReference = targetEntity.GetAttributeValue<EntityReference>(KTR_ManagedListEntity.Fields.KTR_ManagedList);
                if (managedListReference != null)
                {
                    tracing.Trace($"ManagedList reference from Target: {managedListReference.Id}");
                }
            }

            if (managedListReference == null && context.PreEntityImages.Contains("PreImage"))
            {
                var pre = context.PreEntityImages["PreImage"];
                var preRef = pre.GetAttributeValue<EntityReference>(KTR_ManagedListEntity.Fields.KTR_ManagedList);
                if (preRef != null)
                {
                    managedListReference = preRef;
                    tracing.Trace($"ManagedList reference from PreImage: {managedListReference.Id}");
                }
            }

            if (managedListReference == null && id != Guid.Empty)
            {
                tracing.Trace("ManagedList reference null – attempting Retrieve for parent ManagedList (Create/Update).");
                try
                {
                    var full = service.Retrieve(
                        KTR_ManagedListEntity.EntityLogicalName,
                        id,
                        new ColumnSet(KTR_ManagedListEntity.Fields.KTR_ManagedList));
                    managedListReference = full.GetAttributeValue<EntityReference>(KTR_ManagedListEntity.Fields.KTR_ManagedList);
                    tracing.Trace(managedListReference != null
                        ? $"ManagedList reference retrieved: {managedListReference.Id}"
                        : "ManagedList reference still null after Retrieve.");
                }
                catch (Exception ex)
                {
                    tracing.Trace($"Retrieve ManagedListEntity failed: {ex.Message}");
                }
            }

            if (managedListReference == null)
            {
                tracing.Trace("ManagedList reference not found – abort rebuild.");
                return;
            }

            Entity managedListEntity;
            try
            {
                tracing.Trace($"Retrieving ManagedList {managedListReference.Id} for project reference.");
                managedListEntity = service.Retrieve(
                    KTR_ManagedList.EntityLogicalName,
                    managedListReference.Id,
                    new ColumnSet(KTR_ManagedList.Fields.KTR_Project, KTR_ManagedList.Fields.KTR_Name));
            }
            catch (Exception ex)
            {
                tracing.Trace($"Retrieve ManagedList failed: {ex.Message}");
                return;
            }

            var project = managedListEntity.GetAttributeValue<EntityReference>(KTR_ManagedList.Fields.KTR_Project);
            if (project == null)
            {
                tracing.Trace("ManagedList has no project – abort.");
                return;
            }

            tracing.Trace($"Rebuilding managed lists HTML for Project={project.Id} due to {context.MessageName} on ManagedListEntity={id}");
            TryRebuild(service, tracing, project.Id, start);
        }

        private void TryRebuild(IOrganizationService service, ITracingService tracing, Guid projectId, DateTime start)
        {
            try
            {
                var mlRepo = new ManagedListRepository(service);
                var mleRepo = new ManagedListEntityRepository(service);

                var html = HtmlGenerationHelper.RebuildProjectManagedListsHtml(mlRepo, mleRepo, tracing, projectId);

                var update = new Entity(KT_Project.EntityLogicalName, projectId)
                {
                    [KT_Project.Fields.KTR_ManagedListsHtml] = html ?? string.Empty
                };
                service.Update(update);
            }
            catch (Exception ex)
            {
                tracing.Trace($"Error during rebuild: {ex.Message}");
                throw;
            }
            finally
            {
                var elapsedMs = (int)(DateTime.UtcNow - start).TotalMilliseconds;
                tracing.Trace($"{PluginName} END ElapsedMs={elapsedMs}");
            }
        }*/
        }
    }
}
