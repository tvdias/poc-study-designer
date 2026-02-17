namespace Kantar.StudyDesignerLite.Plugins.Project
{
    using System;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Services.Project;
    using Microsoft.Xrm.Sdk;

    public class ReorderProjectQuestionnaireCustomAPI : PluginBase
    {
        public ReorderProjectQuestionnaireCustomAPI()
          : base(typeof(ReorderProjectQuestionnaireCustomAPI))
        {
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            ITracingService tracingService = localPluginContext.TracingService;

            IPluginExecutionContext context = localPluginContext.PluginExecutionContext;
            IOrganizationService service = localPluginContext.CurrentUserService;

            var projectId = context.GetInputParameter<Guid>("projectId");

            var projectService = new ProjectService(
                service,
                tracingService);

            var idsReordered = projectService.ReorderProjectQuestionnaire(projectId);

            if (idsReordered.Count == 0)
            {
                tracingService.Trace($"No questionnaireLines found for {projectId}.");
            }
            else
            {
                tracingService.Trace($"QuestionnaireLines reordered for {projectId}: {string.Join(", ", idsReordered)}.");
            }
        }
    }
}
