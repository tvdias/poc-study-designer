using System;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Plugins.QuestionBank
{
    public class AddSuffixToQuestionVariableNamePostOperation : PluginBase
    {
        public AddSuffixToQuestionVariableNamePostOperation()
            : base(typeof(AddSuffixToQuestionVariableNamePostOperation))
        {
        }

        private const string suffix = "_CUST";

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new InvalidPluginExecutionException(nameof(localContext));
            }

            ITracingService tracingService = localContext.TracingService;
            IOrganizationService orgService = localContext.CurrentUserService;
            IPluginExecutionContext context = localContext.PluginExecutionContext;

            tracingService.Trace($"{nameof(AddSuffixToQuestionVariableNamePostOperation)} plugin execution started.");
            
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity targetEntity)
            {
                if (context.MessageName == "Update" &&
                    !targetEntity.Attributes.Contains(KT_QuestionnaireLines.Fields.KT_QuestionVariableName))
                {
                    tracingService.Trace("KT_QuestionVariableName not updated. Exiting.");
                    return;
                }

                var questionnaireLine = orgService.Retrieve(
                    targetEntity.LogicalName,
                    targetEntity.Id,
                    new ColumnSet(
                        KT_QuestionnaireLines.Fields.KT_QuestionVariableName,
                        KT_QuestionnaireLines.Fields.KT_StandardOrCustom,
                        KT_QuestionnaireLines.Fields.KTR_XmlVariableName)
                ).ToEntity<KT_QuestionnaireLines>();

                if (!string.IsNullOrEmpty(questionnaireLine.KT_QuestionVariableName))
                {
                    var originalValue = questionnaireLine.KT_QuestionVariableName;
                    var newValue = originalValue;

                    if (questionnaireLine.KT_StandardOrCustom == KT_QuestionnaireLines_KT_StandardOrCustom.Custom)
                    {
                        newValue = originalValue + suffix;
                        tracingService.Trace("Custom");
                    }

                    // Perform the update
                    var currentXmlName = questionnaireLine.Contains(KT_QuestionnaireLines.Fields.KTR_XmlVariableName)
                        ? questionnaireLine.KTR_XmlVariableName
                        : null;

                    if (currentXmlName != newValue)
                    {
                        questionnaireLine[KT_QuestionnaireLines.Fields.KTR_XmlVariableName] = newValue;
                        tracingService.Trace($"Updating KTR_XmlVariableName to '{newValue}'");
                        orgService.Update(questionnaireLine);
                    }
                    else
                    {
                        tracingService.Trace("No update required. KTR_XmlVariableName already set correctly.");
                    }
                }
                else
                {
                    tracingService.Trace("KT_QuestionVariableName is null or empty.");
                }
            }
            else
            {
                tracingService.Trace("Target is not an Entity.");
            }

            tracingService.Trace($"{nameof(AddSuffixToQuestionVariableNamePostOperation)} plugin execution finished.");
        }
    }
}
