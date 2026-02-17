namespace Kantar.StudyDesignerLite.Plugins.ManagedListEntity
{
    using System;
    using System.Linq;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    public class UpdateManagedListEntityPreValidationOperation : PluginBase
    {
        private const string ErrorDuplicateAnswerCode = "Answer Code with the same code exist.";
        private const string PreImageName = "PreImage";

        public UpdateManagedListEntityPreValidationOperation() : base(typeof(UpdateManagedListEntityPreValidationOperation)) { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            var context = localContext.PluginExecutionContext;
            var tracing = localContext.TracingService;
            var service = localContext.CurrentUserService;

            if (context.MessageName != "Update" || !context.InputParameters.Contains("Target"))
            {
                return;
            }

            if (!(context.InputParameters["Target"] is Entity target) || target.LogicalName != KTR_ManagedListEntity.EntityLogicalName)
            {
                return;
            }

            if (!target.Attributes.Contains(KTR_ManagedListEntity.Fields.KTR_AnswerCode))
            {
                return; // answer code not changing
            }

            // Get possible new managed list reference directly from target first
            var managedListRef = target.GetAttributeValue<EntityReference>(KTR_ManagedListEntity.Fields.KTR_ManagedList);

            // Pre-image for old values
            Entity preImage = null;
            if (context.PreEntityImages != null && (context.PreEntityImages.Contains(PreImageName)))
            {
                preImage = context.PreEntityImages[PreImageName];
            }

            var oldAnswerCode = string.Empty;
            if (preImage != null)
            {
                oldAnswerCode = preImage.GetAttributeValue<string>(KTR_ManagedListEntity.Fields.KTR_AnswerCode) ?? string.Empty;
                // If managed list not on target, fall back to pre-image
                if (managedListRef == null)
                {
                    managedListRef = preImage.GetAttributeValue<EntityReference>(KTR_ManagedListEntity.Fields.KTR_ManagedList);
                }
            }

            // If still missing old values, retrieve
            if (managedListRef == null || string.IsNullOrEmpty(oldAnswerCode))
            {
                var existing = service.Retrieve(
                    KTR_ManagedListEntity.EntityLogicalName,
                    target.Id,
                    new ColumnSet(
                        KTR_ManagedListEntity.Fields.KTR_AnswerCode,
                        KTR_ManagedListEntity.Fields.KTR_ManagedList))
                    .ToEntity<KTR_ManagedListEntity>();
                if (string.IsNullOrEmpty(oldAnswerCode))
                {
                    oldAnswerCode = existing.KTR_AnswerCode ?? string.Empty;
                }
                if (managedListRef == null)
                {
                    managedListRef = existing.KTR_ManagedList;
                }
            }

            var newAnswerCode = target.GetAttributeValue<string>(KTR_ManagedListEntity.Fields.KTR_AnswerCode) ?? string.Empty;

            if (string.IsNullOrEmpty(oldAnswerCode) && !string.IsNullOrEmpty(newAnswerCode))
            {
                tracing.Trace("Old Answer Code is null and new Answer Code is populated. Skipping pre-validation checks for Power Automate update.");
                return; // allow update
            }

            if (string.Equals(oldAnswerCode, newAnswerCode, StringComparison.OrdinalIgnoreCase))
            {
                return; // no actual change
            }

            tracing.Trace($"Validating answer code change on ManagedListEntity {target.Id} from '{oldAnswerCode}' to '{newAnswerCode}'.");

            if (managedListRef == null)
            {
                tracing.Trace("Managed list reference missing; aborting further validations.");
                return;
            }

            if (IsDuplicateAnswerCode(service, managedListRef.Id, target.Id, newAnswerCode))
            {
                throw new InvalidPluginExecutionException(ErrorDuplicateAnswerCode);
            }

            tracing.Trace("All UpdateManagedListEntity pre-validation rules passed.");
        }

        private bool IsDuplicateAnswerCode(IOrganizationService service, Guid managedListId, Guid currentEntityId, string newAnswerCode)
        {
            if (string.IsNullOrWhiteSpace(newAnswerCode))
            {
                return false;
            }
            var query = new QueryExpression(KTR_ManagedListEntity.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KTR_ManagedListEntity.Fields.KTR_ManagedListEntityId),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_ManagedListEntity.Fields.KTR_ManagedList, ConditionOperator.Equal, managedListId),
                        new ConditionExpression(KTR_ManagedListEntity.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ManagedListEntity_StatusCode.Active),
                        new ConditionExpression(KTR_ManagedListEntity.Fields.KTR_AnswerCode, ConditionOperator.Equal, newAnswerCode)
                    }
                }
            };
            var results = service.RetrieveMultiple(query);
            return results.Entities.Any(e => e.Id != currentEntityId);
        }
    }
}
