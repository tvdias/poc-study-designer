using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services.Description;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLineAnswerList;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Plugins.QuestionnaireLine
{
    public class QuestionnaireLineCustomAnswerTogglePostOperation : PluginBase
    {
        public QuestionnaireLineCustomAnswerTogglePostOperation()
            : base(typeof(QuestionnaireLineCustomAnswerTogglePostOperation))
        { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            if (localContext == null)
            { throw new InvalidPluginExecutionException(nameof(localContext)); }

            var tracing = localContext.TracingService;
            var service = localContext.SystemUserService;
            var context = localContext.PluginExecutionContext;

            if (context.MessageName != "Update") { return; }
            if (!(context.InputParameters["Target"] is Entity target)) { return; }
            if (target.LogicalName != KT_QuestionnaireLines.EntityLogicalName) { return; }

            // Get new value (use false as default)
            bool editCustomAnswerCode = target.Contains(KT_QuestionnaireLines.Fields.KTR_EditCustomAnswerCode)
                ? target.GetAttributeValue<bool>(KT_QuestionnaireLines.Fields.KTR_EditCustomAnswerCode)
                : false;

            tracing.Trace($"Syncing related answers → New Value = {editCustomAnswerCode}");

            var repo = new QuestionnaireLineAnswerListRepository(service);

            var answers = repo.GetCustomAnswerListsByQuestionnaireLine(target.Id);

            tracing.Trace($"Fetched {answers.Count} answers");

            BulkUpdateQLAnswers(service, tracing, answers, editCustomAnswerCode);
        }

        private void BulkUpdateQLAnswers(
            IOrganizationService service,
            ITracingService tracing,
            List<KTR_QuestionnaireLinesAnswerList> answers,
            bool editCustomAnswerCode)
        {
            if (answers == null || answers.Count == 0)
            {
                tracing.Trace("No answers to update");
                return;
            }

            var requests = new OrganizationRequestCollection();
            foreach (var answer in answers)
            {
                var update = new Entity(KTR_QuestionnaireLinesAnswerList.EntityLogicalName)
                {
                    Id = answer.Id
                };
                update[KTR_QuestionnaireLinesAnswerList.Fields.KTR_EnableCustomAnswerCodeEditing] = editCustomAnswerCode;

                requests.Add(new UpdateRequest { Target = update });
            }

            if (requests.Count > 0)
            {
                var executeMultipleRequest = new ExecuteMultipleRequest
                {
                    Settings = new ExecuteMultipleSettings
                    {
                        ContinueOnError = true,
                        ReturnResponses = false
                    },
                    Requests = requests
                };

                service.Execute(executeMultipleRequest);

                tracing.Trace($"Bulk updated {requests.Count} answers to {editCustomAnswerCode}");
            }

        }
    }
}
