namespace Kantar.StudyDesignerLite.Plugins.QuestionnaireLine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.UI.WebControls;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.ManagedList;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLine;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLineAnswerList;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Services;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Services.QuestionnaireLine;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    public class QuestionnaireLineRegenerateHTMLCustomAPI : PluginBase
    {
        public QuestionnaireLineRegenerateHTMLCustomAPI()
               : base(typeof(QuestionnaireLineRegenerateHTMLCustomAPI))
        {
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            var tracingService = localPluginContext.TracingService;
            var context = localPluginContext.PluginExecutionContext;
            var service = localPluginContext.SystemUserService;

            var projectId = context.GetInputParameter<Guid>("projectId");

            // Get QuestionnaireLines by ProjectId
            var qlRepository = new QuestionnaireLineRepository(service);
            var qls = qlRepository.GetQuestionnaireLinesByProjectId(projectId);

            if (qls == null)
            {
                tracingService.Trace($"Project {projectId} doesn't have any QuestionnaireLines.");
                throw new InvalidPluginExecutionException($"Project {projectId} doesn't have any QuestionnaireLines.");
            }

            // Regenerate HTML for each QuestionnaireLine
            var qlAnswerRepository = new QuestionnaireLineAnswerListRepository(service);
            var managedListRepository = new ManagedListRepository(service);
            var qlService = new QuestionnaireLineService(service, tracingService, qlAnswerRepository, managedListRepository);
            qlService.RegenerateHtmlField(qls.Select(x => x.Id).ToList());
            tracingService.Trace($"Finished regenerating HTML for Project {projectId}.");
        }
    }
}
