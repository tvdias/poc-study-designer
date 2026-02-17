using System;
using System.Linq;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Plugins.QuestionnaireLine
{
    public class QuestionnaireLineStudySyncPostOperation : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.QuestionnaireLineStudySyncPostOperation";
        public QuestionnaireLineStudySyncPostOperation()
            : base(typeof(QuestionnaireLineStudySyncPostOperation)) { }

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

            if (context.Depth > 1 && target.LogicalName != KT_QuestionnaireLines.EntityLogicalName)
            {
                tracing.Trace("Skipping plugin execution due to depth > 1.");
                return;
            }

            if (target.LogicalName != KT_QuestionnaireLines.EntityLogicalName)
            {
                tracing.Trace($"Skipping plugin because target entity is not {KT_QuestionnaireLines.EntityLogicalName}.");
                return;
            }

            var ql = target.ToEntity<KT_QuestionnaireLines>();
            var preImage = context.PreEntityImages?.Contains("PreImage") == true ? context.PreEntityImages["PreImage"] : null;
            var postImage = context.PostEntityImages?.Contains("PostImage") == true ? context.PostEntityImages["PostImage"] : null;

            if (context.MessageName == nameof(ContextMessageEnum.Create))
            {
                HandleCreate(service, ql, tracing);
            }
            else if (context.MessageName == nameof(ContextMessageEnum.Update))
            {
                HandleUpdate(service, ql, preImage, postImage, tracing);
            }
        }

        /// <summary>
        /// Handles creation of a Questionnaire Line by copying it to all related draft studies as a new Study Questionnaire Line.
        /// </summary>
        private void HandleCreate(IOrganizationService service, KT_QuestionnaireLines ql, ITracingService tracing)
        {
            tracing.Trace("HandleCreate: Start syncing newly created Questionnaire Line to related draft studies.");
            var projectRef = ql.KTR_Project;

            if (projectRef == null)
            {
                return;
            }

            var studies = GetDraftStudies(service, projectRef.Id);
            var requests = new OrganizationRequestCollection();

            foreach (var study in studies.Entities)
            {
                tracing.Trace($"HandleCreate: Preparing Study Line for Study {study.Id}.");
                var studyLine = new KTR_StudyQuestionnaireLine
                {
                    KTR_Study = new EntityReference(KT_Study.EntityLogicalName, study.Id),
                    KTR_SortOrder = ql.KT_QuestionSortOrder,
                    KTR_QuestionnaireLine = ql.ToEntityReference(),
                    KTR_Name = ql.KT_QuestionVariableName,
                };
                requests.Add(new CreateRequest { Target = studyLine });
            }

            if (requests.Any())
            {
                tracing.Trace($"HandleCreate: Executing {requests.Count} create requests for study lines.");
                foreach (var request in requests)
                {
                    if (request is CreateRequest createRequest)
                    {
                        try
                        {
                            var id = service.Create(createRequest.Target);
                            tracing.Trace($"Created Study Line ID: {id}");
                        }
                        catch (Exception ex)
                        {
                            tracing.Trace($"Failed to create Study Line: {ex.Message}");
                        }
                    }
                }
            }
            else
            {
                tracing.Trace("HandleCreate: No requests to execute.");
            }
        }

        /// <summary>
        /// Handles updates to Questionnaire Line:
        /// - Deactivates linked study lines if the question is deactivated
        /// - Reactivates (or creates) lines in draft studies if the question is reactivated
        /// - Syncs sort order change to all related study lines
        /// </summary>
        private void HandleUpdate(IOrganizationService service, KT_QuestionnaireLines ql, Entity preImage, Entity postImage, ITracingService tracing)
        {
            tracing.Trace("HandleUpdate: Start processing update on Questionnaire Line.");

            var questionnaireLineId = ql.Id;

            // Check for deactivation
            if (ql.StateCode == KT_QuestionnaireLines_StateCode.Inactive)
            {
                HandleDeactivation(service, preImage, questionnaireLineId, tracing);
            }
            else
            {
                tracing.Trace("HandleDeactivation Skipping.");
            }

            // Check for reactivation
            var wasInactive = preImage?.GetAttributeValue<OptionSetValue>(KT_QuestionnaireLines.Fields.StateCode)?.Value
                              == (int)KT_QuestionnaireLines_StateCode.Inactive;
            var isActive = ql.StateCode == KT_QuestionnaireLines_StateCode.Active;

            if (wasInactive && isActive)
            {
                HandleReactivation(service, ql, postImage, questionnaireLineId, tracing);
            }
            else
            {
                tracing.Trace("HandleReactivation Skipping.");
            }

            // Check for sort order change
            if (preImage.Contains(KT_QuestionnaireLines.Fields.KT_QuestionSortOrder) &&
                postImage.Contains(KT_QuestionnaireLines.Fields.KT_QuestionSortOrder))
            {
                var oldOrder = preImage.GetAttributeValue<int>(KT_QuestionnaireLines.Fields.KT_QuestionSortOrder);
                var newOrder = postImage.GetAttributeValue<int>(KT_QuestionnaireLines.Fields.KT_QuestionSortOrder);

                if (oldOrder != newOrder)
                {
                    HandleSortOrderChange(service, preImage, postImage,questionnaireLineId, tracing);
                }
            }
            else
            {
                tracing.Trace("HandleSortOrderChangeIfNeeded: Sort order attributes missing. Skipping.");
            }
        }

        private void HandleDeactivation(IOrganizationService service, Entity preImage, Guid questionnaireLineId, ITracingService tracing)
        {
            tracing.Trace("HandleDeactivationIfNeeded: Questionnaire Line deactivated. Deactivating related study lines.");

            var studyQuestionnaireLines = GetStudyQuestionnaireLines(service, questionnaireLineId: questionnaireLineId, onlyDraftStudies: true);
            var deactivateRequests = new OrganizationRequestCollection();

            foreach (var line in studyQuestionnaireLines.Entities)
            {
                var studyqlUpdate = new KTR_StudyQuestionnaireLine
                {
                    Id = line.Id,
                    StateCode = KTR_StudyQuestionnaireLine_StateCode.Inactive,
                    StatusCode = KTR_StudyQuestionnaireLine_StatusCode.Inactive
                };
                deactivateRequests.Add(new UpdateRequest { Target = studyqlUpdate });
            }

            if (deactivateRequests.Any())
            {
                var batch = new ExecuteMultipleRequest
                {
                    Requests = deactivateRequests,
                    Settings = new ExecuteMultipleSettings
                    {
                        ContinueOnError = true,
                        ReturnResponses = false
                    }
                };
                service.Execute(batch);
            }

            ReorderQuestionnaireLinesInDraftStudies(service, preImage, tracing);
        }

        private void HandleReactivation(IOrganizationService service, KT_QuestionnaireLines ql, Entity postImage, Guid questionnaireLineId, ITracingService tracing)
        {
            tracing.Trace("HandleReactivationIfNeeded: Questionnaire Line reactivated. Syncing to studies.");

            var projectRef = postImage?.GetAttributeValue<EntityReference>(KT_QuestionnaireLines.Fields.KTR_Project);

            if (projectRef == null)
            {
                tracing.Trace("HandleReactivationIfNeeded: Project reference is still null. Skipping reactivation.");
                return;
            }

            var studies = GetDraftStudies(service, projectRef.Id);
            var reactivationRequests = new OrganizationRequestCollection();

            foreach (var study in studies.Entities)
            {
                var nextSortOrder = GetNextSortOrderInStudy(service, study.Id);

                var existing = GetStudyQuestionnaireLines(service, studyId: study.Id, questionnaireLineId: questionnaireLineId, onlyActive: false).Entities.FirstOrDefault();

                if (existing != null)
                {
                    tracing.Trace($"Reactivating existing study line {existing.Id} for study {study.Id}.");

                    var update = new KTR_StudyQuestionnaireLine
                    {
                        Id = existing.Id,
                        StateCode = KTR_StudyQuestionnaireLine_StateCode.Active,
                        StatusCode = KTR_StudyQuestionnaireLine_StatusCode.Active,
                        KTR_SortOrder = nextSortOrder
                    };

                    reactivationRequests.Add(new UpdateRequest { Target = update });
                }
                else
                {
                    tracing.Trace($"Creating new study line for study {study.Id}.");

                    var studyLine = new KTR_StudyQuestionnaireLine
                    {
                        KTR_Study = new EntityReference(KT_Study.EntityLogicalName, study.Id),
                        KTR_SortOrder = ql.KT_QuestionSortOrder,
                        KTR_QuestionnaireLine = ql.ToEntityReference(),
                        KTR_Name = ql.KT_QuestionVariableName
                    };

                    reactivationRequests.Add(new CreateRequest { Target = studyLine });
                }
            }

            if (reactivationRequests.Any())
            {
                var batch = new ExecuteMultipleRequest
                {
                    Requests = reactivationRequests,
                    Settings = new ExecuteMultipleSettings
                    {
                        ContinueOnError = true,
                        ReturnResponses = false
                    }
                };
                service.Execute(batch);
            }
        }

        private void HandleSortOrderChange(IOrganizationService service, Entity preImage, Entity postImage, Guid questionnaireLineId, ITracingService tracing)
        {
            var oldOrder = preImage.GetAttributeValue<int>(KT_QuestionnaireLines.Fields.KT_QuestionSortOrder);
            var newOrder = postImage.GetAttributeValue<int>(KT_QuestionnaireLines.Fields.KT_QuestionSortOrder);

            if (oldOrder != newOrder)
            {
                tracing.Trace($"HandleSortOrderChangeIfNeeded: Sort order changed from {oldOrder} to {newOrder}. Syncing study lines.");
                UpdateSortOrderInStudyLines(service, questionnaireLineId, newOrder, tracing);
            }
            else
            {
                tracing.Trace("HandleSortOrderChangeIfNeeded: Sort order did not change. No update required.");
            }
        }

        /// <summary>
        /// Updates the sort order of all Study Questionnaire Lines that are linked to the given Questionnaire Line.
        /// </summary>
        private void UpdateSortOrderInStudyLines(IOrganizationService service, Guid questionnaireLineId, int newSortOrder, ITracingService tracing)
        {
            tracing.Trace($"UpdateSortOrderInStudyLines: Updating all study lines linked to Questionnaire Line {questionnaireLineId} to sort order {newSortOrder}.");

            var studyLines = GetStudyQuestionnaireLines(service, questionnaireLineId: questionnaireLineId, onlyDraftStudies: true);
            var updateRequests = new OrganizationRequestCollection();

            foreach (var line in studyLines.Entities)
            {
                var update = new Entity(KTR_StudyQuestionnaireLine.EntityLogicalName, line.Id)
                {
                    [KTR_StudyQuestionnaireLine.Fields.KTR_SortOrder] = newSortOrder
                };

                updateRequests.Add(new UpdateRequest { Target = update });
            }

            if (updateRequests.Any())
            {
                var batch = new ExecuteMultipleRequest
                {
                    Requests = updateRequests,
                    Settings = new ExecuteMultipleSettings
                    {
                        ContinueOnError = true,
                        ReturnResponses = false
                    }
                };

                service.Execute(batch);
                tracing.Trace($"UpdateSortOrderInStudyLines: Executed {updateRequests.Count} update requests in batch.");
            }
            else
            {
                tracing.Trace("UpdateSortOrderInStudyLines: No study lines found to update.");
            }
        }

        /// <summary>
        /// After a Questionnaire Line is deactivated, shifts up the sort order of Study Questionnaire Lines
        /// that were positioned after it, to maintain correct order.
        /// </summary>
        private void ReorderQuestionnaireLinesInDraftStudies(IOrganizationService service, Entity deactivatedLine, ITracingService tracing)
        {
            int oldOrder = deactivatedLine.GetAttributeValue<int>(KT_QuestionnaireLines.Fields.KT_QuestionSortOrder);
            var projectRef = deactivatedLine.GetAttributeValue<EntityReference>(KT_QuestionnaireLines.Fields.KTR_Project);
            if (projectRef == null)
            {
                return;
            }

            tracing.Trace($"ReorderQuestionnaireLinesInDraftStudies: Adjusting sort orders for lines after order {oldOrder} in draft studies for project {projectRef.Id}.");

            var draftStudies = GetDraftStudies(service, projectRef.Id);
            var batchRequests = new OrganizationRequestCollection();

            foreach (var study in draftStudies.Entities)
            {
                var linesToUpdate = GetStudyQuestionnaireLines(service, studyId: study.Id, minSortOrder: oldOrder, onlyDraftStudies: true);

                foreach (var line in linesToUpdate.Entities)
                {
                    int newOrder = oldOrder++;

                    var update = new Entity(KTR_StudyQuestionnaireLine.EntityLogicalName, line.Id)
                    {
                        [KTR_StudyQuestionnaireLine.Fields.KTR_SortOrder] = newOrder
                    };

                    tracing.Trace($"ReorderQuestionnaireLinesInDraftStudies: Preparing update for line {line.Id} from order {oldOrder} to {newOrder}.");
                    batchRequests.Add(new UpdateRequest { Target = update });
                }
            }

            if (batchRequests.Any())
            {
                tracing.Trace($"ReorderQuestionnaireLinesInDraftStudies: Executing batch update for {batchRequests.Count} lines.");
                var batch = new ExecuteMultipleRequest
                {
                    Requests = batchRequests,
                    Settings = new ExecuteMultipleSettings
                    {
                        ContinueOnError = true,
                        ReturnResponses = false
                    }
                };
                service.Execute(batch);
            }
            else
            {
                tracing.Trace("ReorderQuestionnaireLinesInDraftStudies: No lines needed updating.");
            }
        }

        /// <summary>
        /// Returns the next available sort order (i.e., last + 1) for a given study.
        /// Only considers active lines.
        /// </summary>
        private int GetNextSortOrderInStudy(IOrganizationService service, Guid studyId)
        {
            var result = GetStudyQuestionnaireLines(service, studyId: studyId, topOneDescBySortOrder: true, onlyDraftStudies: true);
            var lastOrder = result.Entities.FirstOrDefault()?.GetAttributeValue<int>(KTR_StudyQuestionnaireLine.Fields.KTR_SortOrder) ?? 0;
            return lastOrder + 1;
        }

        /// <summary>
        /// Returns the study questionnaire lines based on parameters
        /// </summary>
        private EntityCollection GetStudyQuestionnaireLines(
            IOrganizationService service,
            Guid? studyId = null,
            Guid? questionnaireLineId = null,
            int? minSortOrder = null,
            bool onlyActive = true,
            bool topOneDescBySortOrder = false,
            bool onlyDraftStudies = false)
        {
            var query = new QueryExpression(KTR_StudyQuestionnaireLine.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(
                    KTR_StudyQuestionnaireLine.Fields.KTR_StudyQuestionnaireLineId,
                    KTR_StudyQuestionnaireLine.Fields.KTR_SortOrder,
                    KTR_StudyQuestionnaireLine.Fields.StateCode
                )
            };

            var filter = new FilterExpression(LogicalOperator.And);

            if (studyId.HasValue)
            {
                filter.Conditions.Add(new ConditionExpression(
                    KTR_StudyQuestionnaireLine.Fields.KTR_Study, ConditionOperator.Equal, studyId.Value));
            }

            if (questionnaireLineId.HasValue)
            {
                filter.Conditions.Add(new ConditionExpression(
                    KTR_StudyQuestionnaireLine.Fields.KTR_QuestionnaireLine, ConditionOperator.Equal, questionnaireLineId.Value));
            }

            if (minSortOrder.HasValue)
            {
                filter.Conditions.Add(new ConditionExpression(
                    KTR_StudyQuestionnaireLine.Fields.KTR_SortOrder, ConditionOperator.GreaterThan, minSortOrder.Value));
            }

            if (onlyActive)
            {
                filter.Conditions.Add(new ConditionExpression(
                    KTR_StudyQuestionnaireLine.Fields.StateCode, ConditionOperator.Equal, (int)KTR_StudyQuestionnaireLine_StateCode.Active));
            }

            query.Criteria = filter;

            if (topOneDescBySortOrder)
            {
                query.Orders.Add(new OrderExpression(KTR_StudyQuestionnaireLine.Fields.KTR_SortOrder, OrderType.Descending));
                query.TopCount = 1;
            }

            if (onlyDraftStudies)
            {
                var studyLink = query.AddLink(KT_Study.EntityLogicalName, KTR_StudyQuestionnaireLine.Fields.KTR_Study, KT_Study.Fields.KT_StudyId);
                studyLink.LinkCriteria.AddCondition(KT_Study.Fields.StatusCode, ConditionOperator.Equal, (int)KT_Study_StatusCode.Draft);
            }

            return service.RetrieveMultiple(query);
        }

        /// <summary>
        /// Returns all draft study records related to a given project.
        /// </summary>
        private EntityCollection GetDraftStudies(IOrganizationService service, Guid projectId)
        {
            return service.RetrieveMultiple(new QueryExpression(KT_Study.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KT_Study.Fields.KT_StudyId),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(KT_Study.Fields.KT_Project, ConditionOperator.Equal, projectId),
                        new ConditionExpression(KT_Study.Fields.StatusCode, ConditionOperator.Equal, (int)KT_Study_StatusCode.Draft)
                    }
                }
            });
        }
    }
}
