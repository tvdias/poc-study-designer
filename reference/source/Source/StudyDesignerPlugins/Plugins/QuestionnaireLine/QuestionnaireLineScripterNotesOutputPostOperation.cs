using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Services.Description;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Plugins.QuestionnaireLine
{
    public class QuestionnaireLineScripterNotesOutputPostOperation : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.QuestionnaireLineScripterNotesOutputPostOperation";

        public QuestionnaireLineScripterNotesOutputPostOperation()
            : base(typeof(QuestionnaireLineScripterNotesOutputPostOperation)) { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new InvalidPluginExecutionException(nameof(localContext));
            }

            var tracingService = localContext.TracingService;
            var service = localContext.CurrentUserService;
            var context = localContext.PluginExecutionContext;

            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity targetEntity))
            {
                tracingService.Trace("Target entity is missing.");
                return;
            }

            if (targetEntity.LogicalName != KT_QuestionnaireLines.EntityLogicalName)
            {
                tracingService.Trace("Incorrect entity. Expected KT_QuestionnaireLines.");
                return;
            }

            var columnSet = new ColumnSet(HtmlGenerationHelper.GetFieldsToInclude().Values.ToArray());
            var questionWithFields = service.Retrieve(KT_QuestionnaireLines.EntityLogicalName, targetEntity.Id, columnSet);
            string htmlContent = HtmlGenerationHelper.GenerateScripterNotesHtml(questionWithFields);

            UpdateScripterNotesOutput(service, targetEntity.Id, htmlContent);
        }

        // Set and update Scripter Notes Output field
        private static void UpdateScripterNotesOutput(IOrganizationService service, Guid targetEntityId, string htmlContent)
        {
            var simpleRecord = new KT_QuestionnaireLines(targetEntityId)
            {
                KTR_ScripterNotesOutput = htmlContent
            };
            service.Update(simpleRecord);
        }
    }
}
