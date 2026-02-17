using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Microsoft.Xrm.Sdk;
using System;

namespace Kantar.StudyDesignerLite.Plugins.QuestionnaireLine
{
    public class QuestionnaireLineScriptletsPostOperation : PluginBase
    {
        public QuestionnaireLineScriptletsPostOperation()
           : base(typeof(QuestionnaireLineScriptletsPostOperation))
        {
        }
        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new InvalidPluginExecutionException(nameof(localContext));
            }

            var tracingService = localContext.TracingService;
            var service = localContext.CurrentUserService;
            var context = localContext.PluginExecutionContext;

            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity entity))
            {
                tracingService.Trace("Target entity is missing.");
                return;
            }

            var qline = entity.ToEntity<KT_QuestionnaireLines>();
            var qlineId = qline.Id;

            // Create Scriptlet
            var scriptlet = new KTR_Scriptlets
            {
                KTR_Name = $"Scriptlet - {qlineId}"
            };
            var scriptletId = service.Create(scriptlet);
            tracingService.Trace($"Scriptlet created with ID: {scriptletId}");

            // Update Questionnaire Line with Scriptlet reference
            var updateQLine = new KT_QuestionnaireLines
            {
                Id = qlineId,
                KTR_ScriptletsLookup = new EntityReference(KTR_Scriptlets.EntityLogicalName, scriptletId)
            };
            service.Update(updateQLine);
            tracingService.Trace("Questionnaire Line updated with Scriptlet reference.");
        }
    }
}
