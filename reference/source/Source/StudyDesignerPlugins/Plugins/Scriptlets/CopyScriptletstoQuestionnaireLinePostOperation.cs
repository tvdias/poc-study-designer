using System;
using System.Linq;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Plugins.Scriptlets
{
    public class CopyScriptletstoQuestionnaireLinePostOperation : PluginBase
    {
        public CopyScriptletstoQuestionnaireLinePostOperation()
            : base(typeof(CopyScriptletstoQuestionnaireLinePostOperation))
        {
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new InvalidPluginExecutionException(nameof(localContext));
            }

            var tracing = localContext.TracingService;
            var context = localContext.PluginExecutionContext;
            var service = localContext.SystemUserService;

            if (context.Depth > 1)
            {
                tracing.Trace("Depth > 1, exiting to prevent recursion.");
                return;
            }

            if (!(context.InputParameters["Target"] is Entity entity))
            {
                return;
            }

            var scriptlet = entity.ToEntity<KTR_Scriptlets>();

            var scriptletId = scriptlet.Id;

            // Get the new value
            var newScriptletValue = scriptlet.GetAttributeValue<string>(KTR_Scriptlets.Fields.KTR_ScriptLetsInput);

            // Fetch related Questionnaire Line that has this scriptlet in the lookup
            var qline = GetQuestionnaireLineReferencingScriptlet(service, scriptletId);

            if (qline == null)
            {
                tracing.Trace("No Questionnaire Line found referencing this Scriptlet.");
                return;
            }

            qline[KT_QuestionnaireLines.Fields.KTR_Scriptlets] = newScriptletValue;
            service.Update(qline);
            tracing.Trace("Updated Questionnaire Line with new Scriptlet value.");
        }

        private static Entity GetQuestionnaireLineReferencingScriptlet(IOrganizationService service, Guid scriptletId)
        {
            var fetchQline = new QueryExpression(KT_QuestionnaireLines.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KT_QuestionnaireLines.Fields.KTR_Scriptlets),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KT_QuestionnaireLines.Fields.KTR_ScriptletsLookup, ConditionOperator.Equal, scriptletId)
                    }
                }
            };

            var results = service.RetrieveMultiple(fetchQline);
            return results.Entities.FirstOrDefault();
        }
    }
}
