namespace Kantar.StudyDesignerLite.Plugins.Study
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Kantar.StudyDesignerLite.Plugins.QuestionnaireLine;
    using Kantar.StudyDesignerLite.Plugins.StudyQuestionnaireLine;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.ManagedLists;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLine;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Study;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Subset;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Services.Subset;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Import Questionnaire Lines from Project to Study on Study Creation, 
    /// Questionnaire Lines Managed List Entities relation and Subset Definition processing.
    /// [IMPORTANT] This is a synchronous post-operation plugin doing a havy data operation and should be monitored for performance impact.
    /// </summary>
    public class CreateStudyQuestionnaireLinesSyncPostOperation : PluginBase
    {
        private static readonly string s_pluginName = typeof(CreateStudyQuestionnaireLinesSyncPostOperation).FullName;

        public CreateStudyQuestionnaireLinesSyncPostOperation()
            : base(typeof(CreateStudyQuestionnaireLinesSyncPostOperation))
        {
        }
        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            var tracingService = localPluginContext.TracingService;
            var context = localPluginContext.PluginExecutionContext;
            var service = localPluginContext.SystemUserService;

            // Retrieve the target entity from the input parameters
            if (context.InputParameters.TryGetValue("Target", out Entity target))
            {
                tracingService.Trace($"{s_pluginName} {target.LogicalName}");

                if (target.LogicalName == KT_Study.EntityLogicalName)
                {
                    var study = target.ToEntity<KT_Study>();

                    if (context.MessageName == nameof(ContextMessageEnum.Create))
                    {
                        CopyQuestionnaireLinesToStudy(service, tracingService, study);

                        ProcessSubsetLogic(service, tracingService, study);
                    }
                }
            }
        }

        /// <summary>
        /// Process QuestionnaireLineManagedListEntity aiming to create Subset Definitions
        /// </summary>
        /// <param name="service"> IOrganizationService instance</param>
        /// <param name="tracingService"> ITracingService instance</param>
        /// <param name="study"> new study</param>
        private void ProcessSubsetLogic(
            IOrganizationService service,
            ITracingService tracingService,
            KT_Study study)
        {
            var repository = new SubsetRepository(service);
            var qLMLErepository = new QuestionnaireLineManagedListEntityRepository(service, tracingService);
            var studyRepository = new StudyRepository(service);
            var managedListEntityRepository = new ManagedListEntityRepository(service);

            var subsetService = new SubsetDefinitionService(
                tracingService,
                repository,
                qLMLErepository,
                studyRepository,
                managedListEntityRepository);

            tracingService.Trace("START: SubsetDefinitionService.ProcessSubsetLogic");
            subsetService.ProcessSubsetLogic(study.Id);
            tracingService.Trace("END: SubsetDefinitionService.ProcessSubsetLogic");
        }

        private void CopyQuestionnaireLinesToStudy(IOrganizationService service, ITracingService tracing, KT_Study study)
        {
            if (study.KT_Project == null || study.KTR_ParentStudy == null)
            {   //try getting the project and other info from the study
                study = GetStudyInfo(service, study.Id);
                tracing.Trace($"Study Info retrieved. {study.Id}-{study.KT_Name}");
            }

            if (study.KT_Project != null)
            {
                var questionnaireLines = GetQuestionnaireLines(service, study.KT_Project.Id);
                tracing.Trace("GetQuestionnaireLines executed.");
                var studyQuestionnaireLines = study.KTR_ParentStudy != null ? GetStudyQuestionnaireLines(service, study.KTR_ParentStudy.Id) : null;
                tracing.Trace("GetStudyQuestionnaireLines executed.");
                var createdStudyLines = InsertIntoStudyQuestionnaireLines(service, tracing, questionnaireLines, studyQuestionnaireLines, study.Id);
                tracing.Trace("InsertIntoStudyQuestionnaireLines executed.");

                // After creation, use those StudyQuestionnaireLine records to derive questionnaire line ids for managed list entity copy.
                var qlManagedLists = CopyManagedListEntitiesForQuestionnaireLines(service, tracing, createdStudyLines, study.KTR_ParentStudy?.Id, study.Id);

                // After copying managed list entities, also copy Questionnaire Line Managed List Entities from the parent study (if any)
                // Pass the createdStudyLines and qlManagedLists so we do not need to re-query when no parent study exists.
                CopyQuestionnaireLineManagedListEntitiesForStudy(service, tracing, study.KTR_ParentStudy?.Id, study.Id, createdStudyLines, qlManagedLists);
            }
        }

        #region Queries to Dataverse - Study
        private KT_Study GetStudyInfo(IOrganizationService service, Guid studyId)
        {
            var study = service.Retrieve(
                KT_Study.EntityLogicalName,
                studyId,
                new ColumnSet(KT_Study.Fields.KT_StudyId,
                              KT_Study.Fields.KT_Project,
                              KT_Study.Fields.KTR_ParentStudy));

            return study
                .ToEntity<KT_Study>();
        }
        #endregion

        #region Queries to Dataverse - QuestionnaireLines
        private List<KT_QuestionnaireLines> GetQuestionnaireLines(
            IOrganizationService service,
            Guid projectId)
        {

            var query = new QueryExpression()
            {
                EntityName = KT_QuestionnaireLines.EntityLogicalName,
                ColumnSet = new ColumnSet(KT_QuestionnaireLines.Fields.KT_QuestionnaireLinesId,
                                          KT_QuestionnaireLines.Fields.KT_QuestionSortOrder,
                                          KT_QuestionnaireLines.Fields.KT_QuestionVariableName,
                                          KT_QuestionnaireLines.Fields.StateCode,
                                          KT_QuestionnaireLines.Fields.StatusCode),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KT_QuestionnaireLines.Fields.KTR_Project, ConditionOperator.Equal, projectId)
                    }
                },
                NoLock = true
            };

            var results = service.RetrieveMultiple(query);
            return results.Entities
                .Select(e => e.ToEntity<KT_QuestionnaireLines>())
                .ToList();
        }

        private List<KTR_StudyQuestionnaireLine> GetStudyQuestionnaireLines(
            IOrganizationService service,
            Guid originalStudyId)
        {

            var query = new QueryExpression()
            {
                EntityName = KTR_StudyQuestionnaireLine.EntityLogicalName,
                ColumnSet = new ColumnSet(KTR_StudyQuestionnaireLine.Fields.KTR_QuestionnaireLine,
                                          KTR_StudyQuestionnaireLine.Fields.StateCode,
                                          KTR_StudyQuestionnaireLine.Fields.StatusCode),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_StudyQuestionnaireLine.Fields.KTR_Study, ConditionOperator.Equal, originalStudyId)
                    }
                },
                NoLock = true
            };

            var results = service.RetrieveMultiple(query);
            return results.Entities
                .Select(e => e.ToEntity<KTR_StudyQuestionnaireLine>())
                .ToList();
        }
        #endregion

        #region Insert into StudyQuestionnaireLines
        private List<KTR_StudyQuestionnaireLine> InsertIntoStudyQuestionnaireLines(
             IOrganizationService service,
             ITracingService tracing,
             List<KT_QuestionnaireLines> questionnaireLines,
             List<KTR_StudyQuestionnaireLine> studyQuestionnaireLines,
             Guid studyId)
        {
            var created = new List<KTR_StudyQuestionnaireLine>();

            var createRequests = new OrganizationRequestCollection();
            var updateRequests = new OrganizationRequestCollection();
            var stateUpdateRequests = new OrganizationRequestCollection();

            // Batch all updates first
            foreach (var ql in questionnaireLines)
            {
                var qlToUpdate = new Entity(KT_QuestionnaireLines.EntityLogicalName, ql.Id);
                qlToUpdate[KT_QuestionnaireLines.Fields.KTR_LockAnswerCodeToggle] = false;
                var qlUpdateRequest = new UpdateRequest
                {
                    Target = qlToUpdate
                };

                qlUpdateRequest.Parameters["BypassBusinessLogicExecutionStepIds"]
                    = $"{QuestionnaireLinePreValidation.UpdateStepID},{QuestionnaireLinesBlockScripterOnNonDummyQuestionsPreValidation.UpdateStepID}";

                updateRequests.Add(qlUpdateRequest);

                var studyLine = new KTR_StudyQuestionnaireLine
                {
                    Id = Guid.NewGuid(),
                    KTR_Study = new EntityReference(KT_Study.EntityLogicalName, studyId),
                    KTR_SortOrder = ql.KT_QuestionSortOrder,
                    KTR_QuestionnaireLine = ql.ToEntityReference(),
                    KTR_Name = ql.KT_QuestionVariableName
                };

                var studyQlRequest = new CreateRequest { Target = studyLine };
                studyQlRequest.Parameters["BypassBusinessLogicExecutionStepIds"] = $"{StudyQuestionnaireLinePreValidation.CreateStepID}";

                createRequests.Add(studyQlRequest);

                var state = ql.StateCode == KT_QuestionnaireLines_StateCode.Active ?
                    KTR_StudyQuestionnaireLine_StateCode.Active : KTR_StudyQuestionnaireLine_StateCode.Inactive;

                if (studyQuestionnaireLines != null)
                {
                    var studyQL = studyQuestionnaireLines.FirstOrDefault(sq => sq.KTR_QuestionnaireLine.Id == ql.Id);

                    if (studyQL != null && studyQL.StateCode == KTR_StudyQuestionnaireLine_StateCode.Inactive)
                    {
                        state = KTR_StudyQuestionnaireLine_StateCode.Inactive;
                    }
                }

                if (state == KTR_StudyQuestionnaireLine_StateCode.Inactive)
                {
                    var setStateRequest = new SetStateRequest
                    {
                        EntityMoniker = new EntityReference(KTR_StudyQuestionnaireLine.EntityLogicalName, studyLine.Id),
                        State = new OptionSetValue((int)KTR_StudyQuestionnaireLine_StateCode.Inactive),
                        Status = new OptionSetValue((int)KTR_StudyQuestionnaireLine_StatusCode.Inactive)
                    };

                    setStateRequest.Parameters["BypassBusinessLogicExecutionStepIds"] = $"{StudyQuestionnaireLinePreValidation.CreateStepID}";
                    stateUpdateRequests.Add(setStateRequest);
                }

                created.Add(studyLine);
            }

            // Execute batch updates
            tracing.Trace($"Executing {createRequests.Count} Create/Update requests for Study Questionnaire Lines and Questionnaire Lines.");
            ExecuteTransactionRequest(service, tracing, updateRequests);
            ExecuteTransactionRequest(service, tracing, createRequests);
            ExecuteTransactionRequest(service, tracing, stateUpdateRequests);

            return created;
        }
        #endregion

        #region Managed List Entities Copy
        private List<KTR_QuestionnaireLinesHaRedList> CopyManagedListEntitiesForQuestionnaireLines(
            IOrganizationService service,
            ITracingService tracing,
            List<KTR_StudyQuestionnaireLine> studyLines,
            Guid? parentStudyId,
            Guid studyId)
        {
            if (studyLines == null || studyLines.Count == 0)
            {
                tracing.Trace("No study questionnaire lines found. Skipping Managed List Entity copy.");
                return new List<KTR_QuestionnaireLinesHaRedList>();
            }

            var questionnaireLineIds = studyLines
                .Where(sl => sl.KTR_QuestionnaireLine != null)
                .Select(sl => sl.KTR_QuestionnaireLine.Id)
                .Distinct()
                .ToList();

            if (questionnaireLineIds.Count == 0)
            {
                tracing.Trace("Study questionnaire lines do not reference questionnaire lines. Skipping.");
                return new List<KTR_QuestionnaireLinesHaRedList>();
            }

            var qlManagedLists = GetQuestionnaireLineManagedListsByQuestionnaireLines(service, questionnaireLineIds);
            tracing.Trace("GetQuestionnaireLineManagedListsByQuestionnaireLines executed.");
            if (qlManagedLists.Count == 0)
            {
                tracing.Trace("No questionnaire line managed list records found for referenced questionnaire lines.");
                return qlManagedLists; // empty
            }

            var managedListIds = qlManagedLists
                .Where(x => x.KTR_ManagedList != null)
                .Select(x => x.KTR_ManagedList.Id)
                .Distinct()
                .ToList();
            if (managedListIds.Count == 0)
            {
                tracing.Trace("No managed list references detected.");
                return qlManagedLists;
            }

            var managedListEntities = GetManagedListEntities(service, managedListIds);
            if (managedListEntities.Count == 0)
            {
                tracing.Trace("No managed list entities found.");
                return qlManagedLists;
            }

            if (parentStudyId.HasValue)
            {
                var parentStudyMLEs = GetStudyManagedListEntities(service, parentStudyId.Value);
                tracing.Trace("GetStudyManagedListEntities executed.");
                CopyStudyManagedListEntities(service, tracing, managedListEntities, parentStudyMLEs, studyId);
                tracing.Trace("CopyStudyManagedListEntities executed.");
            }
            else
            {
                InsertStudyManagedListEntities(service, tracing, managedListEntities, studyId);
                tracing.Trace("InsertStudyManagedListEntities executed.");
            }

            return qlManagedLists;
        }

        private List<KTR_QuestionnaireLinesHaRedList> GetQuestionnaireLineManagedListsByQuestionnaireLines(IOrganizationService service, IList<Guid> questionnaireLineIds)
        {
            if (questionnaireLineIds == null || questionnaireLineIds.Count == 0)
            {
                return new List<KTR_QuestionnaireLinesHaRedList>();
            }

            var query = new QueryExpression(KTR_QuestionnaireLinesHaRedList.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(
                    KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList,
                    KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLine),
                Criteria = new FilterExpression
                {
                    Conditions = { new ConditionExpression(KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLine, ConditionOperator.In, questionnaireLineIds.Cast<object>().ToArray()) }
                },
                NoLock = true
            };
            return service.RetrieveMultiple(query).Entities.Select(e => e.ToEntity<KTR_QuestionnaireLinesHaRedList>()).ToList();
        }

        private List<KTR_ManagedListEntity> GetManagedListEntities(IOrganizationService service, IList<Guid> managedListIds)
        {
            if (managedListIds == null || managedListIds.Count == 0)
            {
                return new List<KTR_ManagedListEntity>();
            }
            var query = new QueryExpression(KTR_ManagedListEntity.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(
                    KTR_ManagedListEntity.Fields.KTR_ManagedList,
                    KTR_ManagedListEntity.Fields.KTR_AnswerText,
                    KTR_ManagedListEntity.Fields.StateCode,
                    KTR_ManagedListEntity.Fields.StatusCode),
                Criteria = new FilterExpression
                {
                    Conditions = { new ConditionExpression(KTR_ManagedListEntity.Fields.KTR_ManagedList, ConditionOperator.In, managedListIds.Cast<object>().ToArray()) }
                },
                NoLock = true
            };
            return service.RetrieveMultiple(query).Entities.Select(e => e.ToEntity<KTR_ManagedListEntity>()).ToList();
        }

        private List<KTR_StudyManagedListEntity> GetStudyManagedListEntities(IOrganizationService service, Guid studyId)
        {
            var query = new QueryExpression(KTR_StudyManagedListEntity.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(
                    KTR_StudyManagedListEntity.Fields.KTR_ManagedListEntity,
                    KTR_StudyManagedListEntity.Fields.StateCode,
                    KTR_StudyManagedListEntity.Fields.StatusCode),
                Criteria = new FilterExpression
                {
                    Conditions = {
                        new ConditionExpression(KTR_StudyManagedListEntity.Fields.KTR_Study, ConditionOperator.Equal, studyId)
                    }
                },
                NoLock = true
            };
            return service.RetrieveMultiple(query).Entities.Select(e => e.ToEntity<KTR_StudyManagedListEntity>()).ToList();
        }

        private void CopyStudyManagedListEntities(
            IOrganizationService service,
            ITracingService tracing,
            List<KTR_ManagedListEntity> managedListEntities,
            List<KTR_StudyManagedListEntity> parentStudyMLEs,
            Guid studyId)
        {
            tracing?.Trace($"CopyStudyManagedListEntities: {managedListEntities?.Count ?? 0} entities");
            tracing?.Trace($"CopyStudyManagedListEntities: {parentStudyMLEs?.Count ?? 0} StudyMLEs");

            if (parentStudyMLEs == null || parentStudyMLEs.Count == 0)
            {
                tracing?.Trace($"CopyStudyManagedListEntities: No KTR_StudyManagedListEntity found.");
                return;
            }
            var requests = new OrganizationRequestCollection();
            var stateUpdateRequests = new OrganizationRequestCollection();

            tracing?.Trace($"START: Create KTR_StudyManagedListEntity");
            foreach (var parentStudyMle in parentStudyMLEs)
            {
                var mle = managedListEntities
                    .FirstOrDefault(x => x.Id == parentStudyMle.KTR_ManagedListEntity.Id);

                if (mle == null)
                {
                    tracing?.Trace($"CopyStudyManagedListEntities: No matching KTR_ManagedListEntity found for {parentStudyMle.KTR_ManagedListEntity.Id}.");
                    continue;
                }

                var studyMLE = new KTR_StudyManagedListEntity
                {
                    Id = Guid.NewGuid(),
                    KTR_Study = new EntityReference(KT_Study.EntityLogicalName, studyId),
                    KTR_ManagedListEntity = new EntityReference(KTR_ManagedListEntity.EntityLogicalName, mle.Id),
                    KTR_Name = mle.KTR_AnswerText,
                };

                requests.Add(new CreateRequest { Target = studyMLE });

                if (mle.StateCode == KTR_ManagedListEntity_StateCode.Inactive || parentStudyMle.StateCode == KTR_StudyManagedListEntity_StateCode.Inactive)
                {
                    var setStateRequest = new SetStateRequest
                    {
                        EntityMoniker = new EntityReference(KTR_StudyManagedListEntity.EntityLogicalName, studyMLE.Id),
                        State = new OptionSetValue((int)KTR_StudyManagedListEntity_StateCode.Inactive),
                        Status = new OptionSetValue((int)KTR_StudyManagedListEntity_StatusCode.Inactive)
                    };

                    stateUpdateRequests.Add(setStateRequest);
                    tracing?.Trace($"Inactivated KTR_StudyManagedListEntity: {studyMLE.Id}");
                }
            }
            tracing.Trace($"  Total KTR_StudyManagedListEntity to create: {requests.Count}");
            ExecuteTransactionRequest(service, tracing, requests);
            ExecuteTransactionRequest(service, tracing, stateUpdateRequests);
            tracing?.Trace($"END: Create KTR_StudyManagedListEntity");
        }

        private void InsertStudyManagedListEntities(
            IOrganizationService service,
            ITracingService tracing,
            List<KTR_ManagedListEntity> managedListEntities,
            Guid studyId)
        {
            tracing?.Trace($"InsertStudyManagedListEntities: {managedListEntities?.Count ?? 0} entities");

            if (managedListEntities == null || managedListEntities.Count == 0)
            {
                tracing?.Trace($"InsertStudyManagedListEntities: No KTR_ManagedListEntity found.");
                return;
            }

            var requests = new OrganizationRequestCollection();
            var stateUpdateRequests = new OrganizationRequestCollection();

            tracing?.Trace($"START: Create KTR_StudyManagedListEntity");
            foreach (var mle in managedListEntities)
            {
                var state = mle.StateCode == KTR_ManagedListEntity_StateCode.Active
                    ? KTR_StudyManagedListEntity_StateCode.Active
                    : KTR_StudyManagedListEntity_StateCode.Inactive;

                var studyMLE = new KTR_StudyManagedListEntity
                {
                    Id = Guid.NewGuid(),
                    KTR_Study = new EntityReference(KT_Study.EntityLogicalName, studyId),
                    KTR_ManagedListEntity = new EntityReference(KTR_ManagedListEntity.EntityLogicalName, mle.Id),
                    KTR_Name = mle.KTR_AnswerText
                };

                requests.Add(new CreateRequest { Target = studyMLE });

                if (state == KTR_StudyManagedListEntity_StateCode.Inactive)
                {
                    var setStateRequest = new SetStateRequest
                    {
                        EntityMoniker = new EntityReference(KTR_StudyManagedListEntity.EntityLogicalName, studyMLE.Id),
                        State = new OptionSetValue((int)KTR_StudyManagedListEntity_StateCode.Inactive),
                        Status = new OptionSetValue((int)KTR_StudyManagedListEntity_StatusCode.Inactive)
                    };

                    stateUpdateRequests.Add(setStateRequest);
                    tracing?.Trace($"Inactivate KTR_StudyManagedListEntity: {studyMLE.Id}");
                }
            }

            tracing.Trace($"  Total KTR_StudyManagedListEntity to create: {requests.Count}");
            ExecuteTransactionRequest(service, tracing, requests);
            ExecuteTransactionRequest(service, tracing, stateUpdateRequests);
            tracing?.Trace($"END: Create KTR_StudyManagedListEntity");
        }
        #endregion

        #region Questionnaire Line Managed List Entity Copy
        // Copy KTR_QuestionnaireLinemanAgedListEntity records from parent study to the current study.
        private void CopyQuestionnaireLineManagedListEntitiesForStudy(
            IOrganizationService service,
            ITracingService tracing,
            Guid? parentStudyId,
            Guid currentStudyId,
            List<KTR_StudyQuestionnaireLine> existingStudyLines = null,
            List<KTR_QuestionnaireLinesHaRedList> existingQlManagedLists = null)
        {
            if (!parentStudyId.HasValue)
            {
                // Revised logic (no parent study): For each Questionnaire Line in the study, find Shared List -> Managed List -> Managed List Entities and create records only when MLE exists.
                tracing.Trace("No parent study present. Building QL-ML-MLE chains for current study.");

                // Use existing created study lines if provided, otherwise query.
                var studyLines = existingStudyLines ?? GetStudyQuestionnaireLines(service, currentStudyId);
                tracing.Trace("GetStudyQuestionnaireLines executed.");

                if (studyLines == null || studyLines.Count == 0)
                {
                    tracing.Trace("Current study has no Study Questionnaire Lines. Skipping creation.");
                    return;
                }

                var questionnaireLineIds = studyLines
                    .Where(sl => sl.KTR_QuestionnaireLine != null)
                    .Select(sl => sl.KTR_QuestionnaireLine.Id)
                    .Distinct()
                    .ToList();

                if (questionnaireLineIds.Count == 0)
                {
                    tracing.Trace("No Questionnaire Line references found in study lines. Skipping creation.");
                    return;
                }

                //2. Get managed list records (QuestionnaireLineManagedList) for those questionnaire lines
                var qlManagedLists = existingQlManagedLists ?? GetQuestionnaireLineManagedListsByQuestionnaireLines(service, questionnaireLineIds);
                tracing.Trace("GetQuestionnaireLineManagedListsByQuestionnaireLines executed.");
                if (qlManagedLists == null || qlManagedLists.Count == 0)
                {
                    tracing.Trace("No Questionnaire Line Managed List records found for current study. Skipping creation.");
                    return;
                }

                //3. Collect distinct Managed List IDs from shared list records
                var managedListIds = qlManagedLists
                    .Where(x => x.KTR_ManagedList != null)
                    .Select(x => x.KTR_ManagedList.Id)
                    .Distinct()
                    .ToList();
                if (managedListIds.Count == 0)
                {
                    tracing.Trace("No Managed Lists referenced by Shared List records. Skipping creation.");
                    return;
                }

                //4. Retrieve Managed List Entities for those Managed Lists
                var managedListEntities = GetManagedListEntities(service, managedListIds);
                tracing.Trace("GetManagedListEntities executed.");

                if (managedListEntities == null || managedListEntities.Count == 0)
                {
                    tracing.Trace("No Managed List Entities found for referenced Managed Lists. Skipping creation.");
                    return;
                }

                // Group Managed List Entities by ManagedListId for quick lookup
                var mleByManagedList = managedListEntities
                    .Where(m => m.KTR_ManagedList != null)
                    .GroupBy(m => m.KTR_ManagedList.Id)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var createdCount = 0;

                var requests = new OrganizationRequestCollection();
                var stateUpdateRequests = new OrganizationRequestCollection();

                //5. For each managed list record, create Questionnaire Line Managed List Entity records for each Managed List Entity under its Managed List.
                foreach (var managedList in qlManagedLists)
                {
                    if (managedList.KTR_ManagedList == null)
                    {
                        continue;
                    }
                    var managedListId = managedList.KTR_ManagedList.Id;

                    if (!mleByManagedList.TryGetValue(managedListId, out var mleList) || mleList == null || mleList.Count == 0)
                    {
                        continue; // No MLE -> skip (require QL-ML-MLE chain)
                    }

                    foreach (var mle in mleList)
                    {

                        // Create record only when we have Managed List Entity (requirement) and Questionnaire Line reference
                        if (mle.Id == Guid.Empty || managedList.KTR_QuestionnaireLine == null)
                        {
                            continue;
                        }

                        var newEntity = new KTR_QuestionnaireLinemanAgedListEntity
                        {
                            Id = Guid.NewGuid(),
                            KTR_Name = $"{managedList.KTR_QuestionnaireLine.Name} - {mle.KTR_AnswerText}",
                            KTR_StudyId = new EntityReference(KT_Study.EntityLogicalName, currentStudyId),
                            KTR_ManagedListEntity = new EntityReference(KTR_ManagedListEntity.EntityLogicalName, mle.Id),
                            KTR_ManagedList = managedList.KTR_ManagedList,
                            KTR_QuestionnaireLine = managedList.KTR_QuestionnaireLine
                        };

                        requests.Add(new CreateRequest { Target = newEntity });

                        if (mle.StateCode == KTR_ManagedListEntity_StateCode.Inactive)
                        {
                            var setStateRequest = new SetStateRequest
                            {
                                EntityMoniker = new EntityReference(KTR_QuestionnaireLinemanAgedListEntity.EntityLogicalName, newEntity.Id),
                                State = new OptionSetValue((int)KTR_QuestionnaireLinemanAgedListEntity_StateCode.Inactive),
                                Status = new OptionSetValue((int)KTR_QuestionnaireLinemanAgedListEntity_StatusCode.Inactive)
                            };
                            stateUpdateRequests.Add(setStateRequest);

                            tracing?.Trace($"Inactivate KTR_QuestionnaireLinemanAgedListEntity: {newEntity.Id}");
                        }
                        createdCount++;
                    }
                }
                tracing.Trace($"Created {createdCount} Questionnaire Line Managed List Entity records (no parent study scenario).");
                ExecuteTransactionRequest(service, tracing, requests);
                ExecuteTransactionRequest(service, tracing, stateUpdateRequests);
                return;
            }
            else // Existing logic for when a parent study is present
            {
                var parentQLMLEntities = GetQuestionnaireLineManagedListEntitiesByStudy(service, parentStudyId.Value);
                tracing.Trace("GetQuestionnaireLineManagedListEntitiesByStudy executed.");

                if (parentQLMLEntities == null || parentQLMLEntities.Count == 0)
                {
                    tracing.Trace("Parent study has no Questionnaire Line Managed List Entity records. Nothing to copy.");
                    return;
                }

                tracing.Trace($"Found {parentQLMLEntities.Count} Questionnaire Line Managed List Entity records on parent study. Copying to current study.");

                var parentManagedListIds = parentQLMLEntities
                       .Where(x => x.KTR_ManagedList != null)
                       .Select(x => x.KTR_ManagedList.Id)
                       .Distinct()
                       .ToList();

                var parentManagedListEntities = GetManagedListEntities(service, parentManagedListIds);
                tracing.Trace("GetManagedListEntities executed.");

                var requests = new OrganizationRequestCollection();
                var stateUpdateRequests = new OrganizationRequestCollection();

                foreach (var parentQLMLEntity in parentQLMLEntities)
                {
                    var stateCode = parentQLMLEntity.StateCode == KTR_QuestionnaireLinemanAgedListEntity_StateCode.Active
                        ? KTR_QuestionnaireLinemanAgedListEntity_StateCode.Active
                        : KTR_QuestionnaireLinemanAgedListEntity_StateCode.Inactive;

                    var parentMLE = parentManagedListEntities.FirstOrDefault(mle => mle.Id.Equals(parentQLMLEntity.KTR_ManagedListEntity.Id));

                    if (parentMLE != null)
                    {
                        // Preserve state/status from Managed List Entity
                        stateCode = parentMLE.StateCode == KTR_ManagedListEntity_StateCode.Inactive
                            ? KTR_QuestionnaireLinemanAgedListEntity_StateCode.Inactive
                            : KTR_QuestionnaireLinemanAgedListEntity_StateCode.Active;
                    }

                    var newEntity = new KTR_QuestionnaireLinemanAgedListEntity
                    {
                        Id = Guid.NewGuid(),
                        KTR_Name = parentQLMLEntity.KTR_Name,
                        KTR_StudyId = new EntityReference(KT_Study.EntityLogicalName, currentStudyId),
                        KTR_ManagedListEntity = parentQLMLEntity.KTR_ManagedListEntity,
                        KTR_ManagedList = parentQLMLEntity.KTR_ManagedList,
                        KTR_QuestionnaireLine = parentQLMLEntity.KTR_QuestionnaireLine
                    };

                    requests.Add(new CreateRequest { Target = newEntity });

                    tracing?.Trace($"Created KTR_QuestionnaireLinemanAgedListEntity: {newEntity.Id}");

                    if (stateCode == KTR_QuestionnaireLinemanAgedListEntity_StateCode.Inactive)
                    {
                        var setStateRequest = new SetStateRequest
                        {
                            EntityMoniker = new EntityReference(KTR_QuestionnaireLinemanAgedListEntity.EntityLogicalName, newEntity.Id),
                            State = new OptionSetValue((int)KTR_QuestionnaireLinemanAgedListEntity_StateCode.Inactive),
                            Status = new OptionSetValue((int)KTR_QuestionnaireLinemanAgedListEntity_StatusCode.Inactive)
                        };
                        stateUpdateRequests.Add(setStateRequest);

                        tracing?.Trace($"Inactivate KTR_QuestionnaireLinemanAgedListEntity: {newEntity.Id}");
                    }
                }

                tracing.Trace($"Created {requests.Count} Questionnaire Line Managed List Entity records (with parent study scenario).");
                ExecuteTransactionRequest(service, tracing, requests);
                ExecuteTransactionRequest(service, tracing, stateUpdateRequests);
            }
        }

        // Retrieve Questionnaire Line Managed List Entity records for a given study.
        private List<KTR_QuestionnaireLinemanAgedListEntity> GetQuestionnaireLineManagedListEntitiesByStudy(IOrganizationService service, Guid studyId)
        {
            var query = new QueryExpression(KTR_QuestionnaireLinemanAgedListEntity.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_ManagedListEntity,
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_ManagedList,
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_QuestionnaireLine,
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_Name,
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.StateCode,
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.StatusCode
                ),
                Criteria = new FilterExpression
                {
                    Conditions = { new ConditionExpression(KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_StudyId, ConditionOperator.Equal, studyId) }
                },
                NoLock = true
            };
            var results = service.RetrieveMultiple(query);
            return results.Entities.Select(e => e.ToEntity<KTR_QuestionnaireLinemanAgedListEntity>()).ToList();
        }
        #endregion

        /// <summary>
        /// Executes a collection of organization requests in batches using transactions to optimize performance.
        /// Splits large request collections into smaller chunks and executes them sequentially.
        /// </summary>
        /// <param name="service">The organization service used to execute the requests.</param>
        /// <param name="tracing">The tracing service for logging batch execution progress.</param>
        /// <param name="requests">The collection of organization requests to execute.</param>
        /// <param name="chunkSize">The maximum number of requests per batch. Default is 100.</param>
        /// <remarks>
        /// This method improves performance for bulk operations by:
        /// - Reducing the number of round trips to Dataverse
        /// - Executing requests within transactions for data consistency
        /// - Processing requests in manageable chunks to avoid timeout issues
        /// Each batch is executed as a single transaction. If any request fails, the entire batch is rolled back.
        /// </remarks>
        private static void ExecuteTransactionRequest(
            IOrganizationService service,
            ITracingService tracing,
            IEnumerable<OrganizationRequest> requests,
            int chunkSize = 900)
        {
            var batch = new ExecuteTransactionRequest
            {
                ReturnResponses = false,
                Requests = new OrganizationRequestCollection()
            };

            var count = 0;
            foreach (var request in requests)
            {
                batch.Requests.Add(request);
                count++;

                if (batch.Requests.Count == chunkSize)
                {
                    tracing.Trace($"Executing batch of {batch.Requests.Count()} requests (total processed: {count})");
                    service.Execute(batch);

                    batch.Requests.Clear();
                }
            }

            if (batch.Requests.Count > 0)
            {
                tracing.Trace($"Executing final batch of {batch.Requests.Count()} requests (total processed: {count})");
                service.Execute(batch);
            }
        }
    }
}
