namespace Kantar.StudyDesignerLite.Plugins.Study
{
    using System.Linq;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    public class UpdateProjectStudyCreatedFlagPostOperation : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.UpdateProjectStudyCreatedFlagPostOperation";

        public UpdateProjectStudyCreatedFlagPostOperation() : base(typeof(UpdateProjectStudyCreatedFlagPostOperation))
        {
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            ITracingService tracingService = localPluginContext.TracingService;
            IPluginExecutionContext context = localPluginContext.PluginExecutionContext;
            IOrganizationService service = localPluginContext.SystemUserService;

            tracingService.Trace($"{PluginName} triggered. Message: {context.MessageName}");

            // Determine entity logical name
            string entityLogicalName = context.PrimaryEntityName;
            tracingService.Trace($"{PluginName} entity logical name resolved: {entityLogicalName}");

            if (entityLogicalName == KT_Study.EntityLogicalName &&
                (context.MessageName == nameof(ContextMessageEnum.Create) ||
                    context.MessageName == nameof(ContextMessageEnum.Update) ||
                    context.MessageName == nameof(ContextMessageEnum.Delete)))
            {
                SetStudyCreatedFlag(context, service, tracingService);
            }
        }

        private void SetStudyCreatedFlag(
            IPluginExecutionContext context,
            IOrganizationService service,
            ITracingService tracingService)
        {
            tracingService.Trace("Begin SetStudyCreatedFlag");

            Entity target = null;

            if (context.MessageName == nameof(ContextMessageEnum.Delete) &&
                context.InputParameters["Target"] is EntityReference er)
            {
                tracingService.Trace("Delete operation... Target is of type EntityReference");
            }
            else if (context.InputParameters["Target"] is Entity ent)
            {
                target = ent;
            }

            Entity preImage = context.PreEntityImages.TryGetValue("PreImage", out var p) ? p : null;

            EntityReference projectRef = null;

            if (target != null && target.Contains(KT_Study.Fields.KT_Project))
            {
                projectRef = target.GetAttributeValue<EntityReference>(KT_Study.Fields.KT_Project);
                tracingService.Trace("Project ID found in target.");
            }
            else if (preImage != null && preImage.Contains(KT_Study.Fields.KT_Project))
            {
                projectRef = preImage.GetAttributeValue<EntityReference>(KT_Study.Fields.KT_Project);
                tracingService.Trace("Project ID found in preImage.");
            }

            if (projectRef == null)
            {
                tracingService.Trace("No project reference found, exiting method.");
                return;
            }

            tracingService.Trace($"Checking for active studies linked to project ID: {projectRef.Id}");

            var query = new QueryExpression(KT_Study.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(false),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(KT_Study.Fields.KT_Project, ConditionOperator.Equal, projectRef.Id),
                    }
                }
            };

            var activeStudies = service.RetrieveMultiple(query);
            bool hasActiveStudies = activeStudies.Entities.Any();

            tracingService.Trace($"Active studies found: {hasActiveStudies}");

            var projectToUpdate = new KT_Project(projectRef.Id)
            {
                KTR_StudyCreated = hasActiveStudies
            };

            service.Update(projectToUpdate);

            tracingService.Trace($"Project updated. ktr_studycreated set to: {hasActiveStudies}");
        }
    }
}
