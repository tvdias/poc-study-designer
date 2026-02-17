namespace Kantar.StudyDesignerLite.Plugins.Study
{
    using System;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    public class NewStudyQuestionnaireLinesImportCustomAPI : PluginBase
    {
        private static readonly string s_customApiName = typeof(NewStudyQuestionnaireLinesImportCustomAPI).FullName;

        public NewStudyQuestionnaireLinesImportCustomAPI()
            : base(typeof(NewStudyQuestionnaireLinesImportCustomAPI))
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

            // Retrieve the target entity from the input parameters
            if (context.InputParameters.TryGetValue("study_id", out Guid studyId))
            {
                tracingService.Trace($"Custom API {s_customApiName} Running for Study ID: {studyId}");

                var study = service.Retrieve(KT_Study.EntityLogicalName, studyId, new ColumnSet(true)).ToEntity<KT_Study>()
                    ?? throw new InvalidPluginExecutionException($"Study with ID {studyId} not found.");

                tracingService.Trace($"Custom API {s_customApiName} Ending for Study ID: {studyId}");
            }
        }
    }
}
