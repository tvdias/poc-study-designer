using System;
using System.Linq;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.ManagedLists;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLine;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Study;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Subset;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Services.Subset;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Plugins.StudyManagedListEntity
{
    public class QuestionnaireLineStudyMLESyncPostOperation : PluginBase
    {
        public QuestionnaireLineStudyMLESyncPostOperation()
            : base(typeof(QuestionnaireLineStudyMLESyncPostOperation)) { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            var tracing = localContext.TracingService;
            var service = localContext.CurrentUserService;
            var context = localContext.PluginExecutionContext;

            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity))
            {
                return;
            }

            var target = (Entity)context.InputParameters["Target"];

            if (target.LogicalName != KTR_StudyManagedListEntity.EntityLogicalName)
            {
                tracing.Trace("Skipping because entity is not Study MLE.");
                return;
            }

            var pre = context.PreEntityImages.Contains("PreImage") ? context.PreEntityImages["PreImage"] : null;
            var post = context.PostEntityImages.Contains("PostImage") ? context.PostEntityImages["PostImage"] : null;

            if (context.MessageName != nameof(ContextMessageEnum.Update))
            {
                tracing.Trace("Skipping because not an Update message.");
                return;
            }

            tracing.Trace("Processing activation/deactivation of Study MLE.");

            HandleStateSync(service, tracing, pre, post);
        }

        private void HandleStateSync(IOrganizationService service, ITracingService tracing, Entity pre, Entity post)
        {

            if (pre != null)
            { tracing.Trace($"PreImage attributes: {string.Join(",", pre.Attributes.Select(a => a.Key))}"); }
            if (post != null)
            { tracing.Trace($"PostImage attributes: {string.Join(",", post.Attributes.Select(a => a.Key))}"); }

            if (pre == null || post == null)
            {
                tracing.Trace("ERROR: Pre or Post Image is NULL. Check plugin registration.");
                return;
            }

            var wasInactive = pre.GetAttributeValue<OptionSetValue>(KTR_StudyManagedListEntity.Fields.StateCode)?.Value
                              == (int)KTR_StudyManagedListEntity_StateCode.Inactive;

            var isInactive = post.GetAttributeValue<OptionSetValue>(KTR_StudyManagedListEntity.Fields.StateCode)?.Value
                              == (int)KTR_StudyManagedListEntity_StateCode.Inactive;

            tracing.Trace($"wasInactive = {wasInactive}, isInactive = {isInactive}");

            // extract Study + MLE lookups
            var studyRef = post.GetAttributeValue<EntityReference>(KTR_StudyManagedListEntity.Fields.KTR_Study);
            var mleRef = post.GetAttributeValue<EntityReference>(KTR_StudyManagedListEntity.Fields.KTR_ManagedListEntity);

            tracing.Trace($"StudyRef is {(studyRef == null ? "NULL" : studyRef.Id.ToString())}");
            tracing.Trace($"MleRef is {(mleRef == null ? "NULL" : mleRef.Id.ToString())}");

            if (studyRef == null || mleRef == null)
            {
                tracing.Trace("Study or MLE reference missing — cannot continue.");
                return;
            }

            var studyId = studyRef.Id;
            var mleId = mleRef.Id;

            // CASE 1 – DEACTIVATED 
            if (!wasInactive && isInactive)
            {
                tracing.Trace("Study MLE deactivated → deactivating linked QL-ML-E.");
                UpdateLinkedQlmleState(service, studyId, mleId,
                    state: KTR_QuestionnaireLinemanAgedListEntity_StateCode.Inactive,
                    status: KTR_QuestionnaireLinemanAgedListEntity_StatusCode.Inactive);
            }
        }

        private void UpdateLinkedQlmleState(
            IOrganizationService service,
            Guid studyId,
            Guid mleId,
            KTR_QuestionnaireLinemanAgedListEntity_StateCode state,
            KTR_QuestionnaireLinemanAgedListEntity_StatusCode status)
        {
            var query = new QueryExpression(KTR_QuestionnaireLinemanAgedListEntity.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_QuestionnaireLinemanAgedListEntityId)
            };

            query.Criteria.AddCondition(KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_StudyId, ConditionOperator.Equal, studyId);
            query.Criteria.AddCondition(KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_ManagedListEntity, ConditionOperator.Equal, mleId);

            var rows = service.RetrieveMultiple(query);

            var batch = new ExecuteMultipleRequest
            {
                Requests = new OrganizationRequestCollection(),
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                    ReturnResponses = false
                }
            };

            foreach (var row in rows.Entities)
            {
                var update = new Entity(KTR_QuestionnaireLinemanAgedListEntity.EntityLogicalName, row.Id)
                {
                    [KTR_QuestionnaireLinemanAgedListEntity.Fields.StateCode] = new OptionSetValue((int)state),
                    [KTR_QuestionnaireLinemanAgedListEntity.Fields.StatusCode] = new OptionSetValue((int)status)
                };

                batch.Requests.Add(new UpdateRequest { Target = update });
            }

            if (batch.Requests.Any())
            {
                service.Execute(batch);
            }
        }
    }
}
