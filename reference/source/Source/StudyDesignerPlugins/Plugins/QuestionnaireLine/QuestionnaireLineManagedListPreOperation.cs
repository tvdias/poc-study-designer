using System;
using System.Linq;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;

namespace Kantar.StudyDesignerLite.Plugins.QuestionnaireLine
{
    public class QuestionnaireLineManagedListPreOperation : PluginBase
    {
        public QuestionnaireLineManagedListPreOperation()
            : base(typeof(QuestionnaireLineManagedListPreOperation)) { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new InvalidPluginExecutionException(nameof(localPluginContext));
            }
            var tracingService = localPluginContext.TracingService;
            var service = localPluginContext.CurrentUserService;
            var context = localPluginContext.PluginExecutionContext;
            string errorMessage = string.Empty;
            Entity preImage = null;

            // Log the start of the plugin execution
            tracingService.Trace("Plugin execution started.");

            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity targetEntity))
            {
                tracingService.Trace("Target entity is missing.");
                return;
            }

            if (targetEntity.LogicalName != KTR_QuestionnaireLinesHaRedList.EntityLogicalName && targetEntity.LogicalName != KTR_ManagedList.EntityLogicalName)
            {
                tracingService.Trace("The entity is not the expected KTR_QuestionnaireLinesHaRedList.");
                return;
            }

            bool isCreate = context.MessageName == nameof(ContextMessageEnum.Create);
            bool isUpdate = context.MessageName == nameof(ContextMessageEnum.Update);

            if (isUpdate && context.PreEntityImages.ContainsKey("PreImage"))
            {
                preImage = context.PreEntityImages["PreImage"];
            }
            if (targetEntity.LogicalName == KTR_ManagedList.EntityLogicalName)
            {
                tracingService.Trace("Detected update on KTR_ManagedList entity.");
                //If Managed List's name is updated
                if (targetEntity.Attributes.Contains(KTR_ManagedList.Fields.KTR_Name))
                {
                    var newName = targetEntity.GetAttributeValue<string>(KTR_ManagedList.Fields.KTR_Name);
                    tracingService.Trace($"Managed List name changed to: {newName}");
                    UpdateRelatedQuestionnaireLines(service, tracingService, targetEntity.Id, newName);
                }

                return;
            }

            if (isCreate)
            {
                HandleCreate(targetEntity, service, tracingService);
            }
            else if (isUpdate)
            {
                HandleUpdate(targetEntity, preImage, service, tracingService);
            }
        }

        private void HandleCreate(Entity targetEntity, IOrganizationService service, ITracingService tracingService)
        {
            tracingService.Trace("Handling create for KTR_QuestionnaireLinesHaRedList entity.");

            if (targetEntity.Attributes.Contains(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList) ||
                targetEntity.Attributes.Contains(KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location))
            {
                var managedListRef = targetEntity.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList);
                var locationOptionSet = targetEntity.GetAttributeValue<OptionSetValue>(KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location);

                string locationName = string.Empty;
                if (locationOptionSet != null && targetEntity.FormattedValues.ContainsKey(KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location))
                {
                    locationName = targetEntity.FormattedValues[KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location];
                }
                else if (locationOptionSet != null)
                {
                    locationName = GetOptionSetLabel(service, targetEntity.LogicalName, KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location, locationOptionSet.Value);
                }

                if ((managedListRef != null && managedListRef.Id != Guid.Empty) || !string.IsNullOrWhiteSpace(locationName))
                {
                    var managedListEntity = service.Retrieve(
                        managedListRef.LogicalName,
                        managedListRef.Id,
                        new ColumnSet(KTR_ManagedList.Fields.KTR_Name)
                    );
                    var managedListName = managedListEntity.GetAttributeValue<string>(KTR_ManagedList.Fields.KTR_Name);
                    if (!string.IsNullOrWhiteSpace(managedListName))
                    {
                        targetEntity[KTR_QuestionnaireLinesHaRedList.Fields.KTR_Name] = GetCombinedName(managedListName, locationName);
                    }
                }
            }
        }

        private void HandleUpdate(Entity targetEntity, Entity preImage, IOrganizationService service, ITracingService tracingService)
        {
            tracingService.Trace("Handling update for KTR_QuestionnaireLinesHaRedList entity.");

            EntityReference managedListRef = null;
            
            if (targetEntity.Attributes.Contains(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList))
            {
                managedListRef = targetEntity.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList);
            }
            
            else if (preImage != null && preImage.Attributes.Contains(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList))
            {
                managedListRef = preImage.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList);
            }

            OptionSetValue locationOptionSet = null;
            string locationName = string.Empty;
            if (targetEntity.Attributes.Contains(KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location))
            {
                locationOptionSet = targetEntity.GetAttributeValue<OptionSetValue>(KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location);
                if (locationOptionSet != null && targetEntity.FormattedValues.ContainsKey(KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location))
                {
                    locationName = targetEntity.FormattedValues[KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location];
                }
                else if (locationOptionSet != null)
                {
                    locationName = GetOptionSetLabel(service, targetEntity.LogicalName, KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location, locationOptionSet.Value);
                }
            }
            else if (preImage != null && preImage.Attributes.Contains(KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location))
            {
                locationOptionSet = preImage.GetAttributeValue<OptionSetValue>(KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location);
                if (locationOptionSet != null && preImage.FormattedValues.ContainsKey(KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location))
                {
                    locationName = preImage.FormattedValues[KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location];
                }
                else if (locationOptionSet != null)
                {
                    locationName = GetOptionSetLabel(service, preImage.LogicalName, KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location, locationOptionSet.Value);
                }
            }

            if ((managedListRef != null && managedListRef.Id != Guid.Empty) || !string.IsNullOrWhiteSpace(locationName))
            {
                var managedListEntity = service.Retrieve(
                    managedListRef.LogicalName,
                    managedListRef.Id,
                    new ColumnSet(KTR_ManagedList.Fields.KTR_Name)
                );
                var managedListName = managedListEntity.GetAttributeValue<string>(KTR_ManagedList.Fields.KTR_Name);
                if (!string.IsNullOrWhiteSpace(managedListName))
                {
                    targetEntity[KTR_QuestionnaireLinesHaRedList.Fields.KTR_Name] = GetCombinedName(managedListName, locationName);
                }
            }
        }

        private string GetCombinedName(string managedListName, string locationName)
        {
            return $"{managedListName} - {locationName}";
        }
        private void UpdateRelatedQuestionnaireLines(
    IOrganizationService service,
    ITracingService tracingService,
    Guid managedListId,
    string newManagedListName)
        {
            var relatedRecords = GetRelatedQuestionnaireLines(service, managedListId);
            tracingService.Trace($"Found {relatedRecords.Entities.Count} related records for managed list {managedListId}.");

            foreach (var record in relatedRecords.Entities)
            {
                string locationName = string.Empty;
                var locationOptionSet = record.GetAttributeValue<OptionSetValue>(KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location);

                if (locationOptionSet != null && record.FormattedValues.ContainsKey(KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location))
                {
                    locationName = record.FormattedValues[KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location];
                }
                else if (locationOptionSet != null)
                {
                    locationName = GetOptionSetLabel(
                        service,
                        record.LogicalName,
                        KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location,
                        locationOptionSet.Value
                    );
                }

                var newCombinedName = GetCombinedName(newManagedListName, locationName);

                tracingService.Trace($"Updating {record.Id} with name: {newCombinedName}");

                var updateEntity = new Entity(KTR_QuestionnaireLinesHaRedList.EntityLogicalName, record.Id)
                {
                    [KTR_QuestionnaireLinesHaRedList.Fields.KTR_Name] = newCombinedName
                };
                service.Update(updateEntity);
            }
        }
        private EntityCollection GetRelatedQuestionnaireLines(IOrganizationService service, Guid managedListId)
        {
            var query = new QueryExpression(KTR_QuestionnaireLinesHaRedList.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(
                    KTR_QuestionnaireLinesHaRedList.Fields.KTR_Name,
                    KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList,
                    KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location
                ),
                Criteria = new FilterExpression
                {
                    Conditions =
            {
                new ConditionExpression(
                    KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList,
                    ConditionOperator.Equal,
                    managedListId)
            }
                }
            };

            return service.RetrieveMultiple(query);
        }

        private string GetOptionSetLabel(IOrganizationService service, string entityLogicalName, string attributeLogicalName, int optionSetValue)
        {
            var retrieveAttributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityLogicalName,
                LogicalName = attributeLogicalName,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveAttributeResponse)service.Execute(retrieveAttributeRequest);
            var attributeMetadata = response.AttributeMetadata as PicklistAttributeMetadata;
            if (attributeMetadata != null)
            {
                var option = attributeMetadata.OptionSet.Options.FirstOrDefault(o => o.Value == optionSetValue);
                if (option != null && option.Label != null && option.Label.UserLocalizedLabel != null)
                {
                    return option.Label.UserLocalizedLabel.Label;
                }
            }
            return string.Empty;
        }
    }
}
