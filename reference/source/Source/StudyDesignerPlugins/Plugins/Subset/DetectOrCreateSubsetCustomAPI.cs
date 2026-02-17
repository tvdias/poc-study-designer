namespace Kantar.StudyDesignerLite.Plugins.Subset
{
    using System;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.ManagedLists;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLine;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Study;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Subset;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Services.Subset;
    using Newtonsoft.Json;

    public class DetectOrCreateSubsetCustomAPI : PluginBase
    {
        public DetectOrCreateSubsetCustomAPI()
            : base(typeof(DetectOrCreateSubsetCustomAPI))
        { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            var tracingService = localPluginContext.TracingService;

            var context = localPluginContext.PluginExecutionContext;

            var service = localPluginContext.CurrentUserService;

            var studyId = context.GetInputParameter<Guid>("studyId");

            if (studyId == Guid.Empty)
            {
                tracingService.Trace("StudyId is empty.");

                throw new ArgumentNullException($"{nameof(localPluginContext)} studyId");
            }

            var repository = new SubsetRepository(service);
            var qLMLErepository = new QuestionnaireLineManagedListEntityRepository(service, tracingService);
            var studyRepository = new StudyRepository(service);
            var managedListEntityRepository = new ManagedListEntityRepository(service);

            var subsetService = new SubsetDefinitionService(
                tracingService,
                repository,
                qLMLErepository,
                studyRepository,
                managedListEntityRepository);

            var response = subsetService.ProcessSubsetLogic(studyId);

            tracingService.Trace($"Subset for StudyId {studyId} is processed.");

            context.OutputParameters["subset_response"] = JsonConvert.SerializeObject(response);
        }
    }
}

