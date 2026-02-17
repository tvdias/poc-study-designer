using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.ManagedLists;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.ManagedList;

namespace Kantar.StudyDesignerLite.Plugins.ManagedList
{
    public class CreateUpdateDeleteManagedListPostOperation : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.ManagedList.CreateUpdateDeleteManagedListPostOperation";

        public CreateUpdateDeleteManagedListPostOperation() : base(typeof(CreateUpdateDeleteManagedListPostOperation)) { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            if (localContext == null) { throw new InvalidPluginExecutionException(nameof(localContext)); }
            var tracing = localContext.TracingService;
            var service = localContext.SystemUserService;
            var context = localContext.PluginExecutionContext;
            var start = DateTime.UtcNow;
            tracing.Trace($"{PluginName} START Message={context.MessageName} Stage={context.Stage} Depth={context.Depth}");

            if (!context.InputParameters.Contains("Target"))
            {
                tracing.Trace("No Target in InputParameters. Exit.");
                return;
            }

            // (DELETE - PostOperation only; requires PreImage with ktr_project)
            if (context.MessageName == nameof(ContextMessageEnum.Delete))
            {
                if (context.Stage != (int)ContextStageEnum.PostOperation)
                {
                    tracing.Trace($"Delete received at unexpected Stage={context.Stage}. Register only PostOperation with a PreImage.");
                    return;
                }

                var targetRef = context.InputParameters["Target"] as EntityReference;
                if (targetRef == null || targetRef.LogicalName != KTR_ManagedList.EntityLogicalName)
                {
                    tracing.Trace("Delete: Target not a managed list EntityReference. Exit.");
                    return;
                }

                var preImage = context.PreEntityImages.Contains("PreImage") ? context.PreEntityImages["PreImage"] : null;
                if (preImage == null)
                {
                    tracing.Trace("Delete PostOperation: PreImage missing. Ensure step registered with PreImage including ktr_project.");
                    return;
                }

                var projectRef = preImage.GetAttributeValue<EntityReference>(KTR_ManagedList.Fields.KTR_Project);
                if (projectRef == null)
                {
                    tracing.Trace("Delete PostOperation: PreImage lacks project reference (ktr_project). Cannot rebuild.");
                    return;
                }

                tracing.Trace($"Delete PostOperation: Rebuilding managed lists HTML for Project={projectRef.Id} after ManagedList delete Id={targetRef.Id}");
                TryRebuild(service, tracing, projectRef.Id, start);
                return;
            }

            // CREATE / UPDATE path (PostOperation expected)
            if (!(context.InputParameters["Target"] is Entity target))
            {
                tracing.Trace("Target not an Entity. Exit.");
                return;
            }
            if (target.LogicalName != KTR_ManagedList.EntityLogicalName)
            {
                tracing.Trace("Target logical name is not ktr_managedlist. Exit.");
                return;
            }

            Entity pre = context.PreEntityImages.Contains("PreImage") ? context.PreEntityImages["PreImage"] : null;
            var projectRefFinal = target.GetAttributeValue<EntityReference>(KTR_ManagedList.Fields.KTR_Project) ?? pre?.GetAttributeValue<EntityReference>(KTR_ManagedList.Fields.KTR_Project);
            if (projectRefFinal == null && target.Id != Guid.Empty)
            {
                try
                {
                    var full = service.Retrieve(KTR_ManagedList.EntityLogicalName, target.Id, new ColumnSet(KTR_ManagedList.Fields.KTR_Project));
                    projectRefFinal = full.GetAttributeValue<EntityReference>(KTR_ManagedList.Fields.KTR_Project);
                }
                catch (Exception ex)
                {
                    tracing.Trace($"Retrieve for project failed: {ex.Message}");
                }
            }
            if (projectRefFinal == null)
            {
                tracing.Trace("Project reference not resolved – abort.");
                return;
            }

            tracing.Trace($"Rebuilding managed lists HTML for project {projectRefFinal.Id} after {context.MessageName}.");
            TryRebuild(service, tracing, projectRefFinal.Id, start);
        }

        private void TryRebuild(IOrganizationService service, ITracingService tracing, Guid projectId, DateTime start)
        {
            try
            {
                var mlRepo = new ManagedListRepository(service);

                var html = HtmlGenerationHelper.RebuildProjectManagedListsHtml(mlRepo, tracing, projectId);

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
        }
    }
}
