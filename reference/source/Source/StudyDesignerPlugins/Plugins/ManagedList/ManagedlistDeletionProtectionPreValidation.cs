using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services.Description;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Plugins.ManagedList
{
    public class ManagedlistDeletionProtectionPreValidation : PluginBase
    {
        public ManagedlistDeletionProtectionPreValidation()
            : base(typeof(ManagedlistDeletionProtectionPreValidation)) { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            var tracing = localContext.TracingService;
            var context = localContext.PluginExecutionContext;
            var service = localContext.CurrentUserService;

            if (!context.InputParameters.Contains("Target"))
            {
                return;
            }
            if (context.MessageName == nameof(ContextMessageEnum.Delete))
            {
                var targetRef = context.InputParameters["Target"] as EntityReference;
                if (targetRef == null || targetRef.LogicalName != KTR_ManagedList.EntityLogicalName)
                {
                    return;
                }
                // fetch the full entity manually because we only get an ID in Delete
                var managedList = GetManagedList(service, targetRef.Id);
                ValidateIfCanDelete(tracing, service, managedList);
                return;
            }
            else if (context.MessageName == nameof(ContextMessageEnum.Update))
            {
                ValidateIfCanDeactivate(context, service, tracing);
                return;
            }
            else
            {
                return;
            }

        }

        private void ValidateIfCanDeactivate(IPluginExecutionContext context, IOrganizationService service, ITracingService tracing)

        {
            tracing.Trace("Message = Update");

            var targetEntity = context.InputParameters["Target"] as Entity;
            if (targetEntity == null || targetEntity.LogicalName != KTR_ManagedList.EntityLogicalName)
            {
                tracing.Trace("Update: invalid target - exiting");
                return;
            }

            if (context.PreEntityImages == null || !context.PreEntityImages.Contains("PreImage"))
            {
                tracing.Trace("Update: missing PreImage 'PreImage' - exiting");
                return;
            }

            var preImage = context.PreEntityImages["PreImage"];
            var prevState = preImage.GetAttributeValue<OptionSetValue>(KTR_ManagedList.Fields.StateCode)?.Value;
            var newState = targetEntity.GetAttributeValue<OptionSetValue>(KTR_ManagedList.Fields.StateCode)?.Value;
            // if both present and active->inactive, validate
            if (prevState == 0 && newState == 1)
            {
                var id = targetEntity.Id != Guid.Empty ? targetEntity.Id : preImage.Id;
                tracing.Trace($"Update: deactivating ManagedList {id} - validating...");
                var managedList = GetManagedList(service, id);
                ValidateQuestionnaireLines(service, managedList); 
                tracing.Trace("Update: validation finished");
            }

            return;

        }

        private void ValidateIfCanDelete(
            ITracingService tracing,
            IOrganizationService service,
            KTR_ManagedList managedList)
        {
            tracing.Trace($"Validating deletion of Managed List with ID: {managedList.Id}");

            ValidateQuestionnaireLines(service, managedList);

            ValidateEverInSnapshot(managedList);

        }

        private void ValidateQuestionnaireLines(
            IOrganizationService service,
            KTR_ManagedList managedList)
        {
            var questionnaireLists = GetQuestionnaireLinesByManagedList(service, managedList);

            if (questionnaireLists.Count > 0)
            {
                throw new InvalidPluginExecutionException($"Cannot delete/deactivate this Managed List. It is associated with {questionnaireLists.Count} Questionnaire Line record(s). Please remove those associations before deleting/deactivating.");
            }
        }

        private void ValidateEverInSnapshot(
            KTR_ManagedList managedList)
        {
            if (managedList.KTR_EverInSnapshot.GetValueOrDefault(false))
            {
                throw new InvalidPluginExecutionException($"Cannot delete this Managed List because it's already present in Snapshot.");
            }
        }

        #region Queries to Dataverse - KTR_QuestionnaireLinesHaRedList

        private List<KTR_QuestionnaireLinesHaRedList> GetQuestionnaireLinesByManagedList(IOrganizationService service, KTR_ManagedList managedList)
        {
            var query = new QueryExpression(KTR_QuestionnaireLinesHaRedList.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLinesHaRedListId),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList, ConditionOperator.Equal, managedList.Id),

                    }
                }
            };

            var link = query.AddLink(
                KT_QuestionnaireLines.EntityLogicalName,
                KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLine,
                KT_QuestionnaireLines.Fields.KT_QuestionnaireLinesId,
                JoinOperator.Inner);

            link.LinkCriteria.AddCondition(
                KT_QuestionnaireLines.Fields.StatusCode,
                ConditionOperator.In,
                    new object[]
                    {
                        (int)KT_QuestionnaireLines_StatusCode.Active,
                        (int)KT_QuestionnaireLines_StatusCode.Inactive
                    });

            var results = service.RetrieveMultiple(query);

            return results.Entities
               .Select(e => e.ToEntity<KTR_QuestionnaireLinesHaRedList>())
               .ToList();
        }

        #endregion

        #region Queries to Dataverse - KT_Study
        private List<KT_Study> GetStudiesByProject(IOrganizationService service, Guid projectId)
        {
            var query = new QueryExpression(KT_Study.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KT_Study.Fields.StatusCode),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(KT_Study.Fields.KT_Project, ConditionOperator.Equal, projectId)
                    }
                }
            };

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KT_Study>())
                .ToList();
        }
        #endregion

        #region Queries to Dataverse - KTR_ManagedList
        public KTR_ManagedList GetManagedList(IOrganizationService service, Guid managedListId)
        {
            return service.Retrieve(
                KTR_ManagedList.EntityLogicalName,
                managedListId,
                new ColumnSet(
                    KTR_ManagedList.Fields.Id,
                    KTR_ManagedList.Fields.KTR_Project,
                    KTR_ManagedList.Fields.KTR_EverInSnapshot)
            ).ToEntity<KTR_ManagedList>();
        }
        #endregion
    }
}
