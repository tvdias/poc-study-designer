using System;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Plugins.QuestionnaireLine
{
    public class QuestionnaireLineAnswerListPreOperation : PluginBase
    {
        public QuestionnaireLineAnswerListPreOperation()
            : base(typeof(QuestionnaireLineAnswerListPreOperation)) { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new InvalidPluginExecutionException(nameof(localContext));
            }
            var tracingService = localContext.TracingService;
            var service = localContext.CurrentUserService;
            var context = localContext.PluginExecutionContext;
            string errorMessage = string.Empty;

            // Log the start of the plugin execution
            tracingService.Trace("Plugin execution started.");

            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity targetEntity))
            {
                tracingService.Trace("Target entity is missing.");
                return;
            }

            if (targetEntity.LogicalName != KTR_QuestionnaireLinesAnswerList.EntityLogicalName)
            {
                tracingService.Trace("The entity is not the expected KTR_QuestionnaireLinesAnswerList.");
                return;
            }

            bool isCreate = context.MessageName == "Create";
            bool isUpdate = context.MessageName == "Update";

            // Fetch PreImage if it's an Update
            Entity preImage = null;
            if (isUpdate && context.PreEntityImages.Contains("AnswerIdValidate"))
            {
                preImage = context.PreEntityImages["AnswerIdValidate"];
                tracingService.Trace("PreImage found for Update operation.");
            }
            else
            {
                tracingService.Trace("No PreImage found or not an Update operation.");
            }

            // Step 1: Retrieve values from Target or PreImage (for Update)
            EntityReference questionnaireLineRef = targetEntity.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine)
                ?? preImage?.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine);

            string answerCode = targetEntity.GetAttributeValue<string>(KTR_QuestionnaireLinesAnswerList.Fields.KTR_Name)
                ?? preImage?.GetAttributeValue<string>(KTR_QuestionnaireLinesAnswerList.Fields.KTR_Name);

            if (questionnaireLineRef == null )
            {
                tracingService.Trace(" QuestionnaireLine is missing or empty.");
                return;
            }

            Guid questionnaireLineId = questionnaireLineRef.Id;

            if (questionnaireLineRef == null)
            {
                tracingService.Trace("QuestionnaireLine is missing. Cannot continue.");
                return;
            }

            // Only do duplicate validation if Answer code is not null.
            if (string.IsNullOrWhiteSpace(answerCode))
            {
                tracingService.Trace("AnswerCode is null or empty. Skipping duplicate validation.");
            }
            else
            {
                // Only run duplicate validation when answerCode exists
                AnswerListDuplicateValidation(
                    tracingService,
                    service,
                    errorMessage,
                    targetEntity,
                    isUpdate,
                    preImage,
                    answerCode,
                    questionnaireLineId
                );
            }

            // Set Answer Toggle based on Parent QL Toggle
            SetAnswerEditFlag(service, tracingService, targetEntity, questionnaireLineId);
        }

        private static void AnswerListDuplicateValidation(ITracingService tracingService, IOrganizationService service, string errorMessage, Entity targetEntity, bool isUpdate, Entity preImage, string answerCode, Guid questionnaireLineId)
        {
            var query = new QueryExpression(KTR_QuestionnaireLinesAnswerList.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLinesAnswerListId,
                                                KTR_QuestionnaireLinesAnswerList.Fields.KTR_AnswerType),
                Criteria =
                    {
                        Conditions =
                        {
                            new ConditionExpression(KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine, ConditionOperator.Equal, questionnaireLineId),
                            new ConditionExpression(KTR_QuestionnaireLinesAnswerList.Fields.KTR_Name, ConditionOperator.Equal, answerCode)
                        }
                    }
            };

            // Exclude current record ID for Update operations
            if (isUpdate && targetEntity.Id != Guid.Empty)
            {
                query.Criteria.AddCondition(KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLinesAnswerListId, ConditionOperator.NotEqual, targetEntity.Id);
            }

            var duplicates = service.RetrieveMultiple(query);

            var currentType = targetEntity.GetAttributeValue<OptionSetValue>(KTR_QuestionnaireLinesAnswerList.Fields.KTR_AnswerType)?.Value
                    ?? preImage?.GetAttributeValue<OptionSetValue>(KTR_QuestionnaireLinesAnswerList.Fields.KTR_AnswerType)?.Value;

            if (duplicates.Entities.Count == 0)
            {
                tracingService.Trace("No duplicates found.");
            }
            else if (duplicates.Entities.Count == 1)
            {
                var existingType = duplicates.Entities[0].GetAttributeValue<OptionSetValue>(KTR_QuestionnaireLinesAnswerList.Fields.KTR_AnswerType)?.Value;

                if (existingType == currentType)
                {
                    errorMessage = $"An answer already exists on the questionnaire line ({questionnaireLineId}) with the same AnswerCode '{answerCode}' and the same type.";
                    tracingService.Trace("Duplicate found with same AnswerCode and same type.");
                    throw new InvalidPluginExecutionException(errorMessage);
                }

                tracingService.Trace("Duplicate found with same AnswerCode, but types differ (Row vs Column). Allowing.");
            }
            else // More than 1 duplicate with same AnswerCode
            {
                errorMessage = $"Only one Row and one Column AnswerCode '{answerCode}' can exist per Questionnaire Line ({questionnaireLineId}).";
                tracingService.Trace("More than one duplicate found with same AnswerCode. Cannot proceed.");
                throw new InvalidPluginExecutionException(errorMessage);
            }
        }

        private void SetAnswerEditFlag(IOrganizationService service, ITracingService tracingService, Entity targetEntity, Guid questionnaireLineId)
        {
            var ql = service.Retrieve(KT_QuestionnaireLines.EntityLogicalName, questionnaireLineId,
                new ColumnSet(KT_QuestionnaireLines.Fields.KTR_EditCustomAnswerCode));

            if (ql.GetAttributeValue<bool>(KT_QuestionnaireLines.Fields.KTR_EditCustomAnswerCode))
            {
                targetEntity[KTR_QuestionnaireLinesAnswerList.Fields.KTR_EnableCustomAnswerCodeEditing] = true; // Set YES on the answer toggle, if parent QL toggle is already yes.
                tracingService.Trace("Answer Edit Enabled set to YES based on parent Questionnaire Line.");
            }
        }
    }
}
