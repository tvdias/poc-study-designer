using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Xml.Linq;

namespace Kantar.StudyDesignerLite.Plugins.Common
{
    public class ManagedlistDuplicatePreventionPreOperation : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.Common.ManagedlistDuplicatePreventionPreOperation";

        public ManagedlistDuplicatePreventionPreOperation()
            : base(typeof(ManagedlistDuplicatePreventionPreOperation)) { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            var tracing = localContext.TracingService;
            var context = localContext.PluginExecutionContext;
            var service = localContext.CurrentUserService;

            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity target))
            {
                tracing.Trace("Target not found or is not an Entity. Exiting plugin.");
                return;
            }

            var message = context.MessageName;
            var entityName = target.LogicalName;

            tracing.Trace($"Target entity: {entityName}");

            switch (entityName)
            {
                case KTR_ManagedList.EntityLogicalName:
                    ValidateManagedList(target, service, tracing, context, message);
                    break;

                case KTR_QuestionnaireLinesHaRedList.EntityLogicalName:
                    ValidateQLineManagedList(target, service, tracing, context, message);
                    break;

                default:
                    tracing.Trace($"Entity '{entityName}' not handled.");
                    break;
            }
            tracing.Trace($"Exiting {PluginName}.ExecuteCdsPlugin()");
        }

        #region Duplication Validation

        private void ValidateManagedList(Entity target, IOrganizationService service, ITracingService tracing, IPluginExecutionContext context, string message)
        {
            // Load PreImage only for Update
            var preImage = context.PreEntityImages.Contains("PreImage") ? context.PreEntityImages["PreImage"] : null;

            var managedList = target.ToEntity<KTR_ManagedList>();

            // Use value from Target first, then fallback to PreImage
            var name = managedList.Contains(KTR_ManagedList.Fields.KTR_Name) ? managedList.GetAttributeValue<string>(KTR_ManagedList.Fields.KTR_Name)
                         : preImage?.GetAttributeValue<string>(KTR_ManagedList.Fields.KTR_Name);

            var projectRef = managedList.Contains(KTR_ManagedList.Fields.KTR_Project) ? managedList.GetAttributeValue<EntityReference>(KTR_ManagedList.Fields.KTR_Project)
                                      : preImage?.GetAttributeValue<EntityReference>(KTR_ManagedList.Fields.KTR_Project);

            if (string.IsNullOrWhiteSpace(name) || projectRef == null)
            {
                tracing.Trace("Validation skipped due to missing name or project.");
                return;
            }

            tracing.Trace($"Checking for duplicates: ktr_name = {name}, ktr_project = {projectRef?.Id}");

            var duplicatedManagedLists = GetManagedLists(service, message, tracing, name, projectRef, managedList);

            tracing.Trace($"Found {duplicatedManagedLists.Count} potential duplicates.");

            if (duplicatedManagedLists.Any())
            {
                throw new InvalidPluginExecutionException("A Managed List with the same name already exists in this Project. Please try again.");
            }
        }

        private void ValidateQLineManagedList(Entity target, IOrganizationService service, ITracingService tracing, IPluginExecutionContext context, string message)
        {
            var preImage = context.PreEntityImages.Contains("PreImage") ? context.PreEntityImages["PreImage"] : null;

            var qLineManagedList = target.ToEntity<KTR_QuestionnaireLinesHaRedList>();

            var managedListRef = qLineManagedList.Contains(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList)
                ? qLineManagedList.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList)
                : preImage?.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList);

            var projectRef = qLineManagedList.Contains(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ProjectId)
                ? qLineManagedList.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ProjectId)
                : preImage?.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ProjectId);

            var qlineRef = qLineManagedList.Contains(KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLine)
                ? qLineManagedList.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLine)
                : preImage?.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLine);

            if (managedListRef == null || projectRef == null || qlineRef == null)
            {
                tracing.Trace("Missing one or more required fields: ktr_managedlist, ktr_projectid, ktr_questionnaireline.");
                return;
            }

            tracing.Trace($"Checking QLine Managed List duplicate: ManagedList={managedListRef?.Id}, Project={projectRef?.Id}, QLine={qlineRef?.Id}");

            var duplicatedQLineManagedLists = GetQuestionnaireLineManagedList(service, message, projectRef, managedListRef, qlineRef, qLineManagedList);

            tracing.Trace($"Found {duplicatedQLineManagedLists.Count} potential duplicates.");

            if (duplicatedQLineManagedLists.Any())
            {
                string managedListName = GetEntityName(service, managedListRef);
                string projectName = GetEntityName(service, projectRef);
                string qlineName = GetEntityName(service, qlineRef);

                throw new InvalidPluginExecutionException(
                    "A Questionnaire Line Managed List with the same combination already exists:\r\n" +
                    $"Managed List: {managedListName}. " +
                    $"Project: {projectName}. " +
                    $"Questionnaire Line: {qlineName}."
                );
            }
        }

        #endregion

        #region Queries to Dataverse - Managed List
        private List<KTR_ManagedList> GetManagedLists(IOrganizationService service, string message, ITracingService tracing, string name, EntityReference projectRef, KTR_ManagedList managedList)
        {
            var query = new QueryExpression(KTR_ManagedList.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KTR_ManagedList.Fields.KTR_ManagedListId),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_ManagedList.Fields.KTR_Name, ConditionOperator.Equal, name),
                        new ConditionExpression(KTR_ManagedList.Fields.KTR_Project, ConditionOperator.Equal, projectRef.Id),
                        new ConditionExpression(KTR_ManagedList.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ManagedList_StatusCode.Active)
                    }
                }
            };

            if (message == nameof(ContextMessageEnum.Update))
            {
                query.Criteria.AddCondition(new ConditionExpression(KTR_ManagedList.Fields.KTR_ManagedListId, ConditionOperator.NotEqual, managedList.Id));
                tracing.Trace("Update operation: excluding current record from check.");
            }

            var results = service.RetrieveMultiple(query);
            return results.Entities
                .Select(e => e.ToEntity<KTR_ManagedList>())
                .ToList();
        }

        #endregion

        #region Queries to Dataverse - Questionnaire Line Managed List
        private List<KTR_QuestionnaireLinesHaRedList> GetQuestionnaireLineManagedList(IOrganizationService service, string message, EntityReference projectRef, EntityReference managedListRef, EntityReference qlineRef, KTR_QuestionnaireLinesHaRedList qLineManagedList)
        {
            var query = new QueryExpression(KTR_QuestionnaireLinesHaRedList.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLinesHaRedListId),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList, ConditionOperator.Equal, managedListRef.Id),
                        new ConditionExpression(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ProjectId, ConditionOperator.Equal, projectRef.Id),
                        new ConditionExpression(KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLine, ConditionOperator.Equal, qlineRef.Id),
                        new ConditionExpression(KTR_QuestionnaireLinesHaRedList.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_QuestionnaireLinesHaRedList_StatusCode.Active)
                    }
                }
            };

            if (message == nameof(ContextMessageEnum.Update))
            {
                query.Criteria.AddCondition(new ConditionExpression(KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLinesHaRedListId, ConditionOperator.NotEqual, qLineManagedList.Id));
            }

            var results = service.RetrieveMultiple(query);
            return results.Entities
                .Select(e => e.ToEntity<KTR_QuestionnaireLinesHaRedList>())
                .ToList();
        }
        #endregion

        #region Helper methods for better error messages
        private string GetEntityName(IOrganizationService service, EntityReference entityRef)
        {
            string logicalName = entityRef.LogicalName;
            string primaryField = GetPrimaryFieldForEntity(logicalName);

            if (string.IsNullOrEmpty(primaryField))
            { return $"[{logicalName} - name not configured]"; }

            try
            {
                var entity = service.Retrieve(logicalName, entityRef.Id, new ColumnSet(primaryField));
                return entity.GetAttributeValue<string>(primaryField) ?? "(no name)";
            }
            catch
            {
                return $"[{logicalName} - failed to retrieve]";
            }
        }

        private string GetPrimaryFieldForEntity(string logicalName)
        {
            switch (logicalName)
            {
                case KTR_ManagedList.EntityLogicalName: return KTR_ManagedList.Fields.KTR_Name;
                case KT_Project.EntityLogicalName: return KT_Project.Fields.KT_Name;
                case KT_QuestionnaireLines.EntityLogicalName: return KT_QuestionnaireLines.Fields.KT_QuestionVariableName;
                default: return null;
            }
        }
        #endregion
    }
}
