namespace Kantar.StudyDesignerLite.Plugins.Study
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    public class UpdateStudyPostOperation : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.UpdateStudyPostOperation";

        public UpdateStudyPostOperation() : base(typeof(UpdateStudyPostOperation))
        {
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            ITracingService tracingService = localPluginContext.TracingService;

            IPluginExecutionContext context = localPluginContext.PluginExecutionContext;
            IOrganizationService service = localPluginContext.SystemUserService;

            context.InputParameters.TryGetValue("Target", out Entity entity);

            tracingService.Trace($"{PluginName} {entity.LogicalName}");

            if (entity.LogicalName == KT_Study.EntityLogicalName)
            {
                var study = entity.ToEntity<KT_Study>();

                if (context.MessageName == nameof(ContextMessageEnum.Update))
                {
                    StudyOperations(context, service, tracingService, study);
                    DeleteRelatedFieldworkMarketLanguage(context, service, tracingService, study);
                }
            }
        }

        #region Study Operations based on Status Transitions
        private void StudyOperations(IPluginExecutionContext context, IOrganizationService service, ITracingService tracingService, KT_Study study)
        {
            var preEntity = context.PreEntityImages["Image"].ToEntity<KT_Study>();

            if (preEntity.StatusCode == KT_Study_StatusCode.Draft
                && study.StatusCode == KT_Study_StatusCode.ReadyForScripting)
            {
                tracingService.Trace("Study moved to ReadyForScripting – updating Lock Answer Code on Questionnaire Lines.");

                // Get study questionnaire lines including subset html
                var questionnaireLines = GetStudyQuestionnaireLines(service, study.Id);
                var questionnaireLineIds = JoinStudyQuestionnaireLineIds(questionnaireLines);

                var questionnaireLinesMetadata = GetQuestionnaireLines(service, questionnaireLineIds);

                //Update Lock Answer Code on Questionnaire Lines
                SetLockAnswerCodeOnQuestionnaireLines(service, questionnaireLinesMetadata, tracingService);
                tracingService.Trace("Lock Answer Code for CS users updated on Questionnaire Lines.");
            }
            else if (preEntity.StatusCode == KT_Study_StatusCode.ReadyForScripting
                && study.StatusCode == KT_Study_StatusCode.ApprovedForLaunch)
            {
                tracingService.Trace("Study moved to Approved for Launch – updating Lock Answer Code on Answer List.");

                // Fetch all questionnaire lines
                var questionnaireLines = GetStudyQuestionnaireLines(service, study.Id);
                var questionnaireLineIds = JoinStudyQuestionnaireLineIds(questionnaireLines);

                // Fetch answers for those lines
                var questionnaireLineAnswers = GetQuestionnaireLinesAnswers(service, questionnaireLineIds);

                // Only update Lock Answer Code for answers (NO snapshot logic)
                UpdateLockAnswerCodeInQuestionnaireLinesAnswerList(service, questionnaireLineAnswers);

                tracingService.Trace("Lock Answer Code updated for Approved for Launch transition.");
            }
            else
            {
                tracingService.Trace($"Status transition is not valid.");
            }
        }
        #endregion

        #region Delete Related Fieldwork Market Language
        private void DeleteRelatedFieldworkMarketLanguage(IPluginExecutionContext context, IOrganizationService service, ITracingService tracingService, KT_Study study)
        {
            // Get the pre-update image to compare the previous value
            var preEntity = context.PreEntityImages["Image"].ToEntity<KT_Study>();

            // Check if the Fieldwork Market field has changed
            var preMarket = preEntity.KTR_StudyFieldworkMarket?.Id;
            var postMarket = study.KTR_StudyFieldworkMarket?.Id;

            if (preMarket != postMarket && preMarket.HasValue && postMarket.HasValue)
            {
                // Query all related Fieldwork Market Language records by the old market value
                var query = new QueryExpression
                {
                    EntityName = KTR_FieldworkLanguages.EntityLogicalName,
                    ColumnSet = new ColumnSet(KTR_FieldworkLanguages.Fields.Id),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                            {
                                new ConditionExpression(KTR_FieldworkLanguages.Fields.KTR_FieldworkMarket, ConditionOperator.Equal, preEntity.KTR_StudyFieldworkMarket?.Id ?? Guid.Empty),
                                new ConditionExpression(KTR_FieldworkLanguages.Fields.KTR_Study, ConditionOperator.Equal, preEntity.Id)
                            }
                    }
                };

                var results = service.RetrieveMultiple(query);

                if (results.Entities.Count > 0)
                {
                    var requestCollection = new OrganizationRequestCollection();
                    foreach (var entity in results.Entities)
                    {
                        requestCollection.Add(new DeleteRequest
                        {
                            Target = new EntityReference(entity.LogicalName, entity.Id)
                        });
                    }

                    var executeMultiple = new ExecuteMultipleRequest
                    {
                        Requests = requestCollection,
                        Settings = new ExecuteMultipleSettings
                        {
                            ContinueOnError = true,
                            ReturnResponses = false
                        }
                    };

                    var response = (ExecuteMultipleResponse)service.Execute(executeMultiple);

                    if (response.IsFaulted)
                    {
                        throw new InvalidPluginExecutionException($"Some error happened while deleting fieldwork market language records (see plugin trace for details)");
                    }

                    tracingService.Trace($"Deleted {results.Entities.Count} related Fieldwork Market Language records for market {preMarket.Value}.");
                }
                else
                {
                    tracingService.Trace("No related Fieldwork Market Language records found to delete.");
                }

            }
            else
            {
                tracingService.Trace("Fieldwork Market not changed. No Fieldwork Market Language records deleted.");
            }
        }
        #endregion

        private void SetLockAnswerCodeOnQuestionnaireLines(
            IOrganizationService service,
            IList<KT_QuestionnaireLines> questionnaireLines,
            ITracingService tracing)
        {
            if (questionnaireLines == null || questionnaireLines.Count == 0)
            {
                tracing.Trace("No Questionnaire Lines found for Lock Answer Code update.");
                return;
            }

            tracing.Trace($"Updating Lock Answer Code on {questionnaireLines.Count} Questionnaire Lines.");

            var requests = new OrganizationRequestCollection();

            foreach (var ql in questionnaireLines)
            {
                var updateEntity = new Entity(KT_QuestionnaireLines.EntityLogicalName)
                {
                    Id = ql.Id
                };

                // Lock Answer Code = Yes --> BR will disable Enable Custom Answer Edit toggle.
                updateEntity[KT_QuestionnaireLines.Fields.KTR_LockAnswerCodeToggle] = true;

                // Enable Custom Answer Edit = No
                updateEntity[KT_QuestionnaireLines.Fields.KTR_EditCustomAnswerCode] = false;

                requests.Add(new UpdateRequest { Target = updateEntity });
            }

            var executeMultiple = new ExecuteMultipleRequest
            {
                Requests = requests,
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                    ReturnResponses = false
                }
            };

            service.Execute(executeMultiple);
        }

        #region Queries to Dataverse - StudyQuestionnaireLines
        private List<KTR_StudyQuestionnaireLine> GetStudyQuestionnaireLines(IOrganizationService service, Guid studyId)
        {
            var query = new QueryExpression()
            {
                EntityName = KTR_StudyQuestionnaireLine.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    KTR_StudyQuestionnaireLine.Fields.KTR_Study,
                    KTR_StudyQuestionnaireLine.Fields.KTR_QuestionnaireLine,
                    KTR_StudyQuestionnaireLine.Fields.KTR_SubsetHtml), // include subset html
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_StudyQuestionnaireLine.Fields.KTR_Study, ConditionOperator.Equal, studyId),
                        new ConditionExpression(KTR_StudyQuestionnaireLine.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_StudyQuestionnaireLine_StatusCode.Active),
                    }
                },
                NoLock = true,
            };

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_StudyQuestionnaireLine>())
                .ToList();
        }

        #endregion

        #region Queries to Dataverse - QuestionnaireLines
        private List<KT_QuestionnaireLines> GetQuestionnaireLines(IOrganizationService service, IList<Guid> questionnaireLineIds/*, Guid projectId*/)
        {
            if (questionnaireLineIds == null || questionnaireLineIds.Count == 0)
            {
                return new List<KT_QuestionnaireLines>();
            }

            var query = new QueryExpression()
            {
                EntityName = KT_QuestionnaireLines.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KT_QuestionnaireLines.Fields.KT_QuestionnaireLinesId, ConditionOperator.In, questionnaireLineIds.Cast<object>().ToArray()),
                        new ConditionExpression(KT_QuestionnaireLines.Fields.StatusCode, ConditionOperator.Equal, (int)KT_QuestionnaireLines_StatusCode.Active),
                        //new ConditionExpression(KT_QuestionnaireLines.Fields.KTR_Project, ConditionOperator.Equal, projectId),
                    }
                },
                NoLock = true,
            };

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KT_QuestionnaireLines>())
                .ToList();
        }

        #endregion

        #region Queries to Dataverse - QuestionnaireLinesAnswers
        private List<KTR_QuestionnaireLinesAnswerList> GetQuestionnaireLinesAnswers(IOrganizationService service, IList<Guid> questionnaireLineIds)
        {
            var query = new QueryExpression()
            {
                EntityName = KTR_QuestionnaireLinesAnswerList.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine, ConditionOperator.In, questionnaireLineIds.Cast<object>().ToArray()),
                        new ConditionExpression(KTR_QuestionnaireLinesAnswerList.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_QuestionnaireLinesAnswerList_StatusCode.Active),
                    }
                },
                NoLock = true,
            };

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_QuestionnaireLinesAnswerList>())
                .ToList();
        }
        #endregion

        #region Queries to Dataverse - Study

        private void UpdateLockAnswerCodeInQuestionnaireLinesAnswerList(IOrganizationService service, IList<KTR_QuestionnaireLinesAnswerList> questionnaireLinesAnswersMetadata)
        {
            if (questionnaireLinesAnswersMetadata != null && questionnaireLinesAnswersMetadata.Count > 0)
            {
                var requestCollection = new OrganizationRequestCollection();
                foreach (var qlAnswer in questionnaireLinesAnswersMetadata)
                {
                    if (qlAnswer != null)
                    {
                        qlAnswer.KTR_LockAnswerCode = true; // Set the lock answer code to true
                        requestCollection.Add(new UpdateRequest { Target = qlAnswer });
                    }
                }
                var executeMultiple = new ExecuteMultipleRequest
                {
                    Requests = requestCollection,
                    Settings = new ExecuteMultipleSettings
                    {
                        ContinueOnError = true,
                        ReturnResponses = false
                    }
                };
                var response = (ExecuteMultipleResponse)service.Execute(executeMultiple);
                if (response.IsFaulted)
                {
                    throw new InvalidPluginExecutionException($"Some error happened while updating study questionnaire answer list - update Lock Answer Code (see plugin traces for more detail)");
                }
            }
        }
        private void UpdateStudy(IOrganizationService service, Entity study)
        {
            service.Update(study);
        }

        #endregion

        #region Auxiliar

        public IList<Guid> JoinStudyQuestionnaireLineIds(List<KTR_StudyQuestionnaireLine> list)
        {
            var ids = new List<Guid>();
            foreach (var entity in list)
            {
                if (entity.Contains(KTR_StudyQuestionnaireLine.Fields.KTR_QuestionnaireLine)
                    && entity[KTR_StudyQuestionnaireLine.Fields.KTR_QuestionnaireLine] is EntityReference entityRef)
                {
                    ids.Add(entityRef.Id);
                }
            }
            return ids;
        }

        #endregion
    }
}
