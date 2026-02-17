namespace Kantar.StudyDesignerLite.Plugins.Study
{
    using System;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Language;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.ManagedList;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Project;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLineAnswerSnapshot;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLineSnapshot;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Study;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.SubsetDefinitionSnapshot;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.SubsetEntitiesSnapshot;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Services;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Custom API plugin to generate XML representation of a Study and store it in the Study entity.
    /// Input Parameter: StudyId (Guid)
    /// Updates the study record with the generated XML in the appropriate field.
    /// </summary>
    public class GenerateStudyXMLCustomAPI : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.GenerateStudyXMLCustomAPI";
        private const string StudyIdParameterName = "ktr_studyId";

        public GenerateStudyXMLCustomAPI()
            : base(typeof(GenerateStudyXMLCustomAPI))
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
            IOrganizationService service = localPluginContext.SystemUserService;

            tracingService.Trace($"{PluginName} triggered");

            // Validate input parameters
            if (!context.InputParameters.Contains(StudyIdParameterName) ||
                context.InputParameters[StudyIdParameterName] == null)
            {
                throw new InvalidPluginExecutionException($"Required input parameter '{StudyIdParameterName}' is missing.");
            }

            if (!Guid.TryParse(context.InputParameters[StudyIdParameterName].ToString(), out Guid studyId) ||
                studyId == Guid.Empty)
            {
                throw new InvalidPluginExecutionException($"Invalid StudyId provided: {context.InputParameters[StudyIdParameterName]}");
            }

            tracingService.Trace($"Processing XML StudyId: {studyId}");

            var studyRepository = new StudyRepository(service);
            var projectRepository = new ProjectRepository(service);
            var languageRepository = new LanguageRepository(service);
            var mlRepository = new ManagedListRepository(service);
            var subsetSnapshotRepository = new SubsetDefinitionSnapshotRepository(service);
            var subsetEntitiesSnapshotRepository = new SubsetEntitiesSnapshotRepository(service);
            var questionnaireLineSnapshotRepository = new QuestionnaireLineSnapshotRepository(service);
            var snapshotAnswerRepo = new QuestionnaireLineAnswerSnapshotRepository(service);
            var studyXmlService = new StudyXMLService(
                studyRepository,
                projectRepository,
                languageRepository,
                mlRepository,
                questionnaireLineSnapshotRepository,
                snapshotAnswerRepo,
                subsetSnapshotRepository,
                subsetEntitiesSnapshotRepository,
                tracingService);

            tracingService.Trace("Generating and storing Study XML...");
            var generatedXml = studyXmlService.GenerateAndStoreStudyXml(studyId);

            if (string.IsNullOrWhiteSpace(generatedXml))
            {
                tracingService.Trace($"Skipped XML generation for Study {studyId}.");
            }
            else
            {
                tracingService.Trace($"Successfully generated and stored XML for Study {studyId}. XML length: {generatedXml.Length} characters.");
            }
        }
    }
}
