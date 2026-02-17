namespace Kantar.StudyDesignerLite.Plugins.Study
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.ManagedLists;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLine;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Study;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Subset;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Services;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Services.Subset;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    public class CreateStudySnapshotsCustomAPI : PluginBase
    {
        public CreateStudySnapshotsCustomAPI()
            : base(typeof(CreateStudySnapshotsCustomAPI))
        {
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            ITracingService tracingService = localPluginContext.TracingService;
            IPluginExecutionContext context = localPluginContext.PluginExecutionContext;
            IOrganizationService service = localPluginContext.SystemUserService;

            var studyIdParam = context.GetInputParameter<Guid>("studyId");

            var studyRepos = new StudyRepository(service);
            var study = studyRepos.Get(studyIdParam, new string[]
            {
                KT_Study.Fields.Id,
                KT_Study.Fields.KT_Name,
                KT_Study.Fields.KTR_IsSnapshotCreated,
                KT_Study.Fields.StatusCode,
            });

            var errorMessage = string.Empty;
            if (study == null)
            {
                errorMessage = $"Study with Id {studyIdParam} not found.";
                tracingService.Trace(errorMessage);
                throw new InvalidPluginExecutionException(errorMessage);
            }

            if (study.KTR_IsSnapshotCreated == true)
            {
                errorMessage = $"Study with Id {studyIdParam} has already been snapshotted.";
                tracingService.Trace(errorMessage);
                throw new InvalidPluginExecutionException(errorMessage);
            }

            if (study.StatusCode != KT_Study_StatusCode.ReadyForScripting)
            {
                errorMessage = $"Study with Id {studyIdParam} is not ready for scripting can't be snapshotted.";
                tracingService.Trace(errorMessage);
                throw new InvalidPluginExecutionException(errorMessage);
            }

            CreateStudySnapshots(service, tracingService, study);

            tracingService.Trace("Success!");
        }

        #region Study Operations based on Status Transitions
        private void CreateStudySnapshots(IOrganizationService service, ITracingService tracingService, KT_Study study)
        {
            // Get study questionnaire lines including subset html
            var questionnaireLines = GetStudyQuestionnaireLines(service, study.Id);
            var questionnaireLineIds = JoinStudyQuestionnaireLineIds(questionnaireLines);

            var questionnaireLinesMetadata = GetQuestionnaireLines(service, questionnaireLineIds);

            // Create snapshots (copy subset html from study questionnaire line)
            InsertStudyQuestionnaireLinesSnapshot(service, questionnaireLinesMetadata, questionnaireLines, study.Id);
            tracingService.Trace("InsertStudyQuestionnaireLinesSnapshot executed.");

            var studyQuestionnaireLineSnapshots = GetStudyQuestionnaireLineSnapshots(service, study.Id);

            var questionnaireLineAnswersMetadata = GetQuestionnaireLinesAnswers(service, questionnaireLineIds);

            var questionnaireLinesManagedListMetadata = GetQuestionnaireLinesManagedList(service, questionnaireLineIds);

            InsertStudyQuestionnaireLineAnswersSnapshot(service, studyQuestionnaireLineSnapshots, questionnaireLineAnswersMetadata);
            tracingService.Trace($"InsertStudyQuestionnaireLineAnswersSnapshot executed.");

            InsertSnapshotManagedLists(service, studyQuestionnaireLineSnapshots, questionnaireLinesManagedListMetadata, tracingService);
            tracingService.Trace($"InsertSnapshotManagedLists executed.");

            // Fetch back the created StudyQuestionManagedListSnapshots
            var studyQuestionManagedListSnapshots = GetStudyQuestionManagedListSnapshotsForStudy(service, study.Id);

            var qlMles = GetQLManagedListEntities(service, study.Id);
            // Insert Managed List Entity Snapshots
            InsertStudyManagedListEntitiesSnapshots(service, studyQuestionManagedListSnapshots, qlMles, tracingService);
            tracingService.Trace($"InsertStudyManagedListEntitiesSnapshots executed.");

            UpdateManagedListsEverInSnapshot(service, questionnaireLineIds, tracingService);
            tracingService.Trace($"UpdateManagedListsEverInSnapshot executed.");

            // Insert QLs-Subset Snapshot
            var studyQlsSubsets = GetStudyQuestionnaireLinesSubsets(service, study.Id, questionnaireLineIds);
            tracingService.Trace($"GetStudyQuestionnaireLinesSubsets executed.");
            var subsetIds = studyQlsSubsets != null ?
                studyQlsSubsets.Select(s => s.KTR_SubsetDefinitionId.Id).Distinct().ToList() :
                new List<Guid>();
            var subsets = GetSubsetsBySubsetIds(service, subsetIds);
            tracingService.Trace($"GetSubsetsBySubsetIds executed. Subsets found: {subsetIds.Count}");

            InsertStudyQlsSubsetSnapshots(service, tracingService, subsets, studyQuestionnaireLineSnapshots, studyQuestionManagedListSnapshots, studyQlsSubsets);
            tracingService.Trace($"InsertStudyQlsSubsetSnapshots executed.");

            UpdateBulkSubsets(service, tracingService, subsetIds);
            tracingService.Trace($"UpdateBulkSubsets executed.");

            // Insert Qls-Subset Entities Snapshots
            var studySubsetSnapshotsCreated = GetStudyQlsSubsetSnapshots(service, study.Id);
            var subsetEntities = GetSubsetEntitiesBySubsetIds(service, subsetIds);
            InsertStudyQlsSubsetEntitiesSnapshots(service, tracingService, study.Id, subsetEntities, studyQuestionnaireLineSnapshots, studySubsetSnapshotsCreated);
            tracingService.Trace($"InsertStudyQlsSubsetEntitiesSnapshots executed.");

            // Build a view model for Subset Snapshot HTML (Subset Name, Question Count, Entities)
            var subsetSvc = new SubsetDefinitionService(
                tracingService,
                new SubsetRepository(service),
                new QuestionnaireLineManagedListEntityRepository(service, tracingService),
                new StudyRepository(service),
                new ManagedListEntityRepository(service));
            var subsetSnapshotView = subsetSvc.GetSubsetSnapshotSummary(service, study.Id, tracingService);
            tracingService.Trace($"Subset snapshot view prepared for {subsetSnapshotView.Count} subsets.");

            var subsetHtml = HtmlGenerationHelper.RenderSubsetSnapshotView(subsetSnapshotView);
            tracingService.Trace($"subsetHtml generated.");
            study.KTR_SubsetSnapshotHtml = subsetHtml;

            // Update Study
            study.KTR_IsSnapshotCreated = true;
            UpdateStudy(service, study);

            tracingService.Trace($"UpdateStudy executed.");
        }
        #endregion

        private void UpdateManagedListsEverInSnapshot(
            IOrganizationService service,
            IList<Guid> questionnaireLineIds,
            ITracingService tracingService)
        {
            tracingService.Trace("UpdateManagedListsEverInSnapshot started.");

            // Get all Questionnaire Line Managed List records
            var qlManagedLists = GetQuestionnaireLinesManagedList(service, questionnaireLineIds);

            if (qlManagedLists == null || qlManagedLists.Count == 0)
            {
                tracingService.Trace("No Questionnaire Line Managed List records found.");
                return;
            }

            var managedListIds = qlManagedLists
                .Where(x => x.Contains(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList))
                .Select(x => ((EntityReference)x[KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList]).Id)
                .Distinct()
                .ToList();

            if (managedListIds.Count == 0)
            {
                tracingService.Trace("No Managed List references found.");
                return;
            }

            // Fetch Managed Lists
            var managedLists = GetManagedLists(service, managedListIds);
            // Fetch Managed List Entities
            var managedListEntities = GetManagedListEntities(service, managedListIds);

            UpdateBulkManagedLists(service, tracingService, managedLists);
            UpdateBulkManagedListEntities(service, tracingService, managedListEntities);
        }

        #region Queries to Dataverse - StudyQuestionnaireLineSnapshots

        private List<KTR_StudyQuestionnaireLineSnapshot> GetStudyQuestionnaireLineSnapshots(IOrganizationService service, Guid studyId)
        {
            var query = new QueryExpression()
            {
                EntityName = KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_Study,
                    KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_QuestionnaireLine),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_Study, ConditionOperator.Equal, studyId),
                        new ConditionExpression(KTR_StudyQuestionnaireLineSnapshot.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_StudyQuestionnaireLinesNaPsHot_StatusCode.Active),
                    }
                },
                NoLock = true,
            };

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_StudyQuestionnaireLineSnapshot>())
                .ToList();
        }

        private void InsertStudyQuestionnaireLinesSnapshot(
          IOrganizationService service,
          IList<KT_QuestionnaireLines> questionnaireLinesMetadata,
          IList<KTR_StudyQuestionnaireLine> studyQuestionnaireLines,
          Guid studyId)
        {
            if (questionnaireLinesMetadata == null || questionnaireLinesMetadata.Count == 0)
            {
                return;
            }

            // Build lookup from QuestionnaireLineId to subset html
            var subsetHtmlByQuestionnaireLine = new Dictionary<Guid, string>();
            if (studyQuestionnaireLines != null)
            {
                foreach (var sl in studyQuestionnaireLines)
                {
                    var qlRef = sl.KTR_QuestionnaireLine;
                    if (qlRef != null)
                    {
                        var html = sl.GetAttributeValue<string>(KTR_StudyQuestionnaireLine.Fields.KTR_SubsetHtml);
                        if (!subsetHtmlByQuestionnaireLine.ContainsKey(qlRef.Id))
                        {
                            subsetHtmlByQuestionnaireLine.Add(qlRef.Id, html);
                        }
                        else
                        {
                            subsetHtmlByQuestionnaireLine[qlRef.Id] = html; // last wins
                        }
                    }
                }
            }

            var requestCollection = new OrganizationRequestCollection();

            foreach (var ql in questionnaireLinesMetadata)
            {
                var snapshot = new KTR_StudyQuestionnaireLineSnapshot
                {
                    KTR_Study = new EntityReference(KT_Study.EntityLogicalName, studyId),
                    KTR_QuestionnaireLine = new EntityReference(KT_QuestionnaireLines.EntityLogicalName, ql.Id),
                    KTR_Id = ql.KT_Name,
                    KTR_QuestionRationale = ql.KTR_QuestionRationale,
                    KTR_QuestionText = ql.KT_QuestionText2,
                    KTR_QuestionTitle = ql.KT_QuestionTitle,
                    KTR_QuestionType = ql.KT_QuestionTypeName,
                    KTR_Name = ql.KT_QuestionVariableName,
                    KTR_QuestionVersion2 = ql.KTR_QuestionVersion,
                    KTR_ScripterNotes = ql.KTR_ScripterNotes,
                    KTR_SortOrder = ql.KT_QuestionSortOrder,
                    KTR_StandardOrCustom = ql.KT_StandardOrCustomName,
                    KTR_AnswerList = ql.KTR_AnswerList,
                    KTR_ScripterNotesOutput = ql.KTR_ScripterNotesOutput,
                    KTR_IsDummyQuestion = ql.KTR_IsDummyQuestion,
                    KTR_Scriptlets = ql.KTR_Scriptlets,
                    KTR_RowSortOrder = ql.FormattedValues.TryGetValue(KT_QuestionnaireLines.Fields.KTR_RowSortOrder, out var rowLabel) ? rowLabel : null,
                    KTR_ColumnSortOrder = ql.FormattedValues.TryGetValue(KT_QuestionnaireLines.Fields.KTR_ColumnSortOrder, out var columnLabel) ? columnLabel : null,
                    KTR_AnswerMin = ql.KTR_AnswerMin,
                    KTR_AnswerMax = ql.KTR_AnswerMax,
                    KTR_QuestionFormatDetails = ql.KTR_QuestionFormatDetails,
                    KTR_CustomNotes = ql.KTR_CustomNotes
                };

                if (ql.KTR_Module != null)
                {
                    snapshot[KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_Module2] = new EntityReference(KT_Module.EntityLogicalName, ql.KTR_Module.Id);
                }

                // Copy subset html
                string subsetHtml;
                if (subsetHtmlByQuestionnaireLine.TryGetValue(ql.Id, out subsetHtml) && !string.IsNullOrEmpty(subsetHtml))
                {
                    snapshot[KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_SubsetHtml] = subsetHtml;
                }

                requestCollection.Add(new CreateRequest { Target = snapshot });
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
                throw new InvalidPluginExecutionException($"Some error happening while inserting study questionnaire snapshot (see plugin trace for details)");
            }
        }

        #endregion

        #region Queries to Dataverse - StudyQuestionAnswerListSnapshot
        private void InsertStudyQuestionnaireLineAnswersSnapshot(
            IOrganizationService service,
            IList<KTR_StudyQuestionnaireLineSnapshot> questionnaireLineSnapshots,
            IList<KTR_QuestionnaireLinesAnswerList> questionnaireLineAnswersMetadata)
        {
            if (questionnaireLineSnapshots != null && questionnaireLineSnapshots.Count > 0
                && questionnaireLineAnswersMetadata != null && questionnaireLineAnswersMetadata.Count > 0)
            {
                var requestCollection = new OrganizationRequestCollection();

                foreach (var qlAnswer in questionnaireLineAnswersMetadata)
                {
                    var snapshotAnswer = new Entity(KTR_StudyQuestionAnswerListSnapshot.EntityLogicalName);

                    var questionnaireLineSnapshot = questionnaireLineSnapshots.FirstOrDefault(x => x.KTR_QuestionnaireLine.Id == qlAnswer.KTR_QuestionnaireLine.Id);

                    if (questionnaireLineSnapshot != null)
                    {
                        snapshotAnswer[KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot] = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, questionnaireLineSnapshot.Id);
                    }
                    snapshotAnswer[KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_QuestionnaireLinesAnswerList] = new EntityReference(KTR_QuestionnaireLinesAnswerList.EntityLogicalName, qlAnswer.Id);
                    snapshotAnswer[KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_Name] = qlAnswer.KTR_Name;
                    snapshotAnswer[KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_AnswerId] = qlAnswer.KTR_AnswerId;
                    snapshotAnswer[KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_AnswerLocation] = qlAnswer.KTR_AnswerTypeName;
                    snapshotAnswer[KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_CustomProperty] = qlAnswer.KTR_CustomProperty;
                    snapshotAnswer[KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_EffectiveDate] = qlAnswer.KTR_EffectiveDate;
                    snapshotAnswer[KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_EndDate] = qlAnswer.KTR_EndDate;
                    snapshotAnswer[KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_IsActive] = qlAnswer.KTR_IsActiveName;
                    snapshotAnswer[KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_IsExclusive] = qlAnswer.KTR_IsExclusiveName;
                    snapshotAnswer[KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_IsFixed] = qlAnswer.KTR_IsFixedName;
                    snapshotAnswer[KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_IsOpen] = qlAnswer.KTR_IsoPennaMe;
                    snapshotAnswer[KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_IsTranslatable] = qlAnswer.KTR_IsTranslatableName;
                    snapshotAnswer[KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_SourceId] = qlAnswer.KTR_SourceId;
                    snapshotAnswer[KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_SourceName] = qlAnswer.KTR_SourceName;
                    snapshotAnswer[KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_Version] = qlAnswer.KTR_Version;
                    snapshotAnswer[KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_AnswerText] = qlAnswer.KTR_AnswerText;
                    snapshotAnswer[KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_DisplayOrder] = qlAnswer.KTR_DisplayOrder;

                    requestCollection.Add(new CreateRequest { Target = snapshotAnswer });
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
                    throw new InvalidPluginExecutionException($"Some error happening while inserting study questionnaire answer snapshot (see plugin trace for details)");
                }
            }
        }
        #endregion

        #region Queries to Dataverse - StudyQuestionManagedListSnapshots
        private void InsertSnapshotManagedLists(
            IOrganizationService service,
            IList<KTR_StudyQuestionnaireLineSnapshot> questionnaireLineSnapshots,
            IList<KTR_QuestionnaireLinesHaRedList> questionnaireLinesManagedListMetadata,
            ITracingService tracingService)
        {
            tracingService.Trace($"InsertSnapshotManagedLists started with {questionnaireLineSnapshots.Count} questionnaire line snapshots and {questionnaireLinesManagedListMetadata.Count} managed list metadata.");

            if (questionnaireLineSnapshots != null && questionnaireLineSnapshots.Count > 0
                && questionnaireLinesManagedListMetadata != null && questionnaireLinesManagedListMetadata.Count > 0)
            {
                var requestCollection = new OrganizationRequestCollection();

                foreach (var qlManagedList in questionnaireLinesManagedListMetadata)
                {
                    var snapshotManagedList = new KTR_StudyQuestionManagedListSnapshot();

                    var questionnaireLineSnapshot = questionnaireLineSnapshots.FirstOrDefault(x => x.KTR_QuestionnaireLine.Id == qlManagedList.KTR_QuestionnaireLine.Id);

                    if (questionnaireLineSnapshot != null)
                    {
                        snapshotManagedList.KTR_QuestionnaireLinesNaPsHot = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, questionnaireLineSnapshot.Id);
                    }
                    snapshotManagedList.KTR_Name = qlManagedList.KTR_Name;
                    snapshotManagedList.KTR_Location = qlManagedList.KTR_Location != null ? qlManagedList.KTR_Location : null;
                    snapshotManagedList.KTR_DisplayOrder = qlManagedList.KTR_DisplayOrder;
                    snapshotManagedList.KTR_QuestionnaireLineManagedList = new EntityReference(KTR_QuestionnaireLinesHaRedList.EntityLogicalName, qlManagedList.Id);
                    snapshotManagedList.KTR_ManagedList = new EntityReference(KTR_ManagedList.EntityLogicalName, qlManagedList.KTR_ManagedList.Id);

                    requestCollection.Add(new CreateRequest { Target = snapshotManagedList });
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
                    throw new InvalidPluginExecutionException($"Some error happening while inserting Study - Question Managed List Snapshots (see plugin trace for details)");
                }
            }
        }

        // Fetch created StudyQuestionManagedListSnapshots for Managed List Entity snapshots
        private List<KTR_StudyQuestionManagedListSnapshot> GetStudyQuestionManagedListSnapshotsForStudy(
            IOrganizationService service, Guid studyId)
        {
            // Get all StudyQuestionnaireLineSnapshots for this study
            var questionnaireLineSnapshots = GetStudyQuestionnaireLineSnapshots(service, studyId);
            if (questionnaireLineSnapshots == null || questionnaireLineSnapshots.Count == 0)
            { return new List<KTR_StudyQuestionManagedListSnapshot>(); }

            var snapshotIds = questionnaireLineSnapshots.Select(x => x.Id).ToList();

            // Get StudyQuestionManagedListSnapshots where QuestionnaireLinesSnapshot is in these IDs
            var query = new QueryExpression
            {
                EntityName = KTR_StudyQuestionManagedListSnapshot.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    true
                ),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            KTR_StudyQuestionManagedListSnapshot.Fields.StatusCode,
                            ConditionOperator.Equal,
                            (int)KTR_StudyQuestionManagedListSnapshot_StatusCode.Active),
                        new ConditionExpression(
                            KTR_StudyQuestionManagedListSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot,
                            ConditionOperator.In,
                            snapshotIds.Cast<object>().ToArray())
                    }
                },
                NoLock = true
            };

            return service.RetrieveMultiple(query).Entities
                .Select(e => e.ToEntity<KTR_StudyQuestionManagedListSnapshot>())
                .ToList();
        }
        #endregion

        #region Queries to Dataverse - StudyManagedListEntitiesSnapshots
        private void InsertStudyManagedListEntitiesSnapshots(
            IOrganizationService service,
            IList<KTR_StudyQuestionManagedListSnapshot> createdQmlSnapshots,
            IList<KTR_QuestionnaireLinemanAgedListEntity> qlMles,
            ITracingService tracingService)
        {
            tracingService.Trace($"InsertStudyManagedListEntitiesSnapshots started with {createdQmlSnapshots.Count} StudyQuestionManagedListSnapshots.");

            if (createdQmlSnapshots == null || createdQmlSnapshots.Count == 0)
            {
                tracingService.Trace("No StudyQuestionManagedListSnapshots provided.");
                return;
            }

            if (qlMles == null || qlMles.Count == 0)
            {
                tracingService.Trace("No QuestionnaireLineManagedListEntities provided.");
                return;
            }

            var requestCollection = new OrganizationRequestCollection();

            foreach (var qmlSnapshot in createdQmlSnapshots)
            {
                if (qmlSnapshot.KTR_ManagedList == null)
                {
                    tracingService.Trace($"Skipping QML snapshot {qmlSnapshot.Id} because KTR_ManagedList is null.");
                    continue;
                }

                // Fetch Managed List Entities for this Managed List
                var managedListEntities = GetManagedListEntities(service, new List<Guid> { qmlSnapshot.KTR_ManagedList.Id });

                if (managedListEntities == null || managedListEntities.Count == 0)
                {
                    tracingService.Trace($"No ManagedListEntities found for ManagedList {qmlSnapshot.KTR_ManagedList.Id}.");
                    continue;
                }

                // Build dictionary for quick lookup of MLE name/display order
                var mleDict = managedListEntities.ToDictionary(
                    x => x.Id,
                    x => new
                    {
                        Name = x.Contains(KTR_ManagedListEntity.Fields.KTR_AnswerText) ? x[KTR_ManagedListEntity.Fields.KTR_AnswerText] : null,
                        DisplayOrder = x.Contains(KTR_ManagedListEntity.Fields.KTR_DisplayOrder) ? x[KTR_ManagedListEntity.Fields.KTR_DisplayOrder] : null
                    });

                // Retrieve the QuestionnaireLinesSnapshot reference from the StudyQuestionManagedListSnapshot
                var questionnaireLineSnapshotRef = qmlSnapshot.GetAttributeValue<EntityReference>(KTR_StudyQuestionManagedListSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot);

                // Determine the original QuestionnaireLine Id (so we can filter QL-MLE rows)
                Guid? questionnaireLineId = null;
                tracingService.Trace($"questionnaireLineSnapshotRef: {questionnaireLineSnapshotRef.Id}");
                if (questionnaireLineSnapshotRef != null)
                {
                    // Retrieve the StudyQuestionnaireLineSnapshot record to get the original KTR_QuestionnaireLine ref
                    var studyQlSnapshotEntity = service.Retrieve(
                        KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName,
                        questionnaireLineSnapshotRef.Id,
                        new ColumnSet(KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_QuestionnaireLine));

                    var sourceQlRef = studyQlSnapshotEntity.GetAttributeValue<EntityReference>(KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_QuestionnaireLine);
                    if (sourceQlRef != null)
                    {
                        questionnaireLineId = sourceQlRef.Id;
                        tracingService.Trace($"For QML snapshot {qmlSnapshot.Id}, source questionnaire line id is {questionnaireLineId.Value}.");
                    }
                    else
                    {
                        tracingService.Trace($"StudyQuestionnaireLineSnapshot {questionnaireLineSnapshotRef.Id} does not contain KTR_QuestionnaireLine.");
                    }
                }
                else
                {
                    tracingService.Trace($"QML snapshot {qmlSnapshot.Id} does not reference a StudyQuestionnaireLineSnapshot.");
                }

                // If we couldn't resolve a source questionnaire line id, fall back to using the provided qlManagedListEntities as-is.
                var qlMleToProcess = questionnaireLineId.HasValue ?
                    qlMles
                        .Where(x => x.KTR_QuestionnaireLine.Id == questionnaireLineId.Value && x.KTR_ManagedList.Id == qmlSnapshot.KTR_ManagedList.Id)
                        .ToList() :
                    qlMles;

                tracingService.Trace($"QL-MLE count to process: {qlMleToProcess.Count} for ML: {qmlSnapshot.KTR_ManagedList.Id}");

                foreach (var qlmle in qlMleToProcess)
                {
                    // Get the related ManagedListEntity info
                    var relatedMleRef = qlmle.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_ManagedListEntity);
                    string name = null;
                    object displayOrder = null;

                    if (relatedMleRef != null && mleDict.ContainsKey(relatedMleRef.Id))
                    {
                        name = mleDict[relatedMleRef.Id].Name?.ToString();
                        displayOrder = mleDict[relatedMleRef.Id].DisplayOrder;
                    }

                    var snapshotEntity = new KTR_StudyManagedListEntitiesSnapshot
                    {
                        [KTR_StudyManagedListEntitiesSnapshot.Fields.KTR_StudyQuestionManagedListSnapshot] =
                            new EntityReference(KTR_StudyQuestionManagedListSnapshot.EntityLogicalName, qmlSnapshot.Id),

                        [KTR_StudyManagedListEntitiesSnapshot.Fields.KTR_Name] = name,

                        [KTR_StudyManagedListEntitiesSnapshot.Fields.KTR_DisplayOrder] = displayOrder,

                        [KTR_StudyManagedListEntitiesSnapshot.Fields.KTR_ManagedListEntity] = relatedMleRef,

                        [KTR_StudyManagedListEntitiesSnapshot.Fields.KTR_QuestionnaireLinemanAgedListEntity] =
                            new EntityReference(KTR_QuestionnaireLinemanAgedListEntity.EntityLogicalName, qlmle.Id),
                    };

                    // Set QL snapshot reference
                    if (questionnaireLineSnapshotRef != null)
                    {
                        snapshotEntity[KTR_StudyManagedListEntitiesSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot] =
                            questionnaireLineSnapshotRef;
                    }

                    requestCollection.Add(new CreateRequest { Target = snapshotEntity });
                }
            }

            if (requestCollection.Count > 0)
            {
                tracingService.Trace($"Executing ExecuteMultipleRequest for {requestCollection.Count} StudyManagedListEntitiesSnapshots...");
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
                    throw new InvalidPluginExecutionException($"Some error happening while inserting Study Managed List Entities Snapshots (see plugin trace for details)");
                }

                tracingService.Trace($"Inserted {requestCollection.Count} Study Managed List Entities Snapshots successfully.");
            }
            else
            {
                tracingService.Trace("No StudyManagedListEntitiesSnapshots to insert.");
            }
        }
        #endregion

        #region Queries to Dataverse - Study Subset Snapshots
        private void InsertStudyQlsSubsetSnapshots(
            IOrganizationService service,
            ITracingService tracingService,
            IList<KTR_SubsetDefinition> subsets,
            IList<KTR_StudyQuestionnaireLineSnapshot> questionnaireLineSnapshots,
            IList<KTR_StudyQuestionManagedListSnapshot> mlSnapshots,
            IList<KTR_QuestionnaireLineSubset> studyQlsSubsets)
        {
            if (questionnaireLineSnapshots != null && questionnaireLineSnapshots.Count > 0
                && studyQlsSubsets != null && studyQlsSubsets.Count > 0
                && subsets != null && subsets.Count > 0)
            {
                var requestCollection = new OrganizationRequestCollection();

                foreach (var qlSubset in studyQlsSubsets)
                {
                    var snapshotSubset = new Entity(KTR_StudySubsetDefinitionSnapshot.EntityLogicalName);

                    var questionnaireLineSnapshot = questionnaireLineSnapshots.FirstOrDefault(x => x.KTR_QuestionnaireLine.Id == qlSubset.KTR_QuestionnaireLineId.Id);

                    if (questionnaireLineSnapshot != null)
                    {
                        snapshotSubset[KTR_StudySubsetDefinitionSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot] = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, questionnaireLineSnapshot.Id);
                    }
                    snapshotSubset[KTR_StudySubsetDefinitionSnapshot.Fields.KTR_SubsetDefinition2] = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, qlSubset.KTR_SubsetDefinitionId.Id);

                    snapshotSubset[KTR_StudySubsetDefinitionSnapshot.Fields.KTR_Study] = new EntityReference(KT_Study.EntityLogicalName, qlSubset.KTR_Study.Id);

                    snapshotSubset[KTR_StudySubsetDefinitionSnapshot.Fields.KTR_Name] = qlSubset.KTR_SubsetDefinitionId.Name;

                    var subset = subsets.FirstOrDefault(x => x.Id == qlSubset.KTR_SubsetDefinitionId.Id);
                    if (subset != null)
                    {
                        snapshotSubset[KTR_StudySubsetDefinitionSnapshot.Fields.KTR_ManagedListNameLabel] = subset.KTR_ManagedListName;
                    }

                    if (mlSnapshots != null && mlSnapshots.Count > 0 && questionnaireLineSnapshot != null)
                    {
                        var mlSnapshot = mlSnapshots.FirstOrDefault(x => x.KTR_QuestionnaireLinesNaPsHot.Id == questionnaireLineSnapshot.Id && x.KTR_ManagedList.Id == subset.KTR_ManagedList.Id);

                        if (mlSnapshot != null)
                        {
                            snapshotSubset[KTR_StudySubsetDefinitionSnapshot.Fields.KTR_ManagedListLocation] = mlSnapshot.KTR_LocationName;
                        }
                    }

                    requestCollection.Add(new CreateRequest { Target = snapshotSubset });
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
                    throw new InvalidPluginExecutionException($"Some error happening while inserting Study Subset Snapshots (see plugin trace for details)");
                }
            }
            else
            {
                tracingService.Trace($"No StudyQlsSubsetDefinitionSnapshots were inserted.");
            }
        }

        private List<KTR_StudySubsetDefinitionSnapshot> GetStudyQlsSubsetSnapshots(
            IOrganizationService service,
            Guid studyId)
        {
            var query = new QueryExpression()
            {
                EntityName = KTR_StudySubsetDefinitionSnapshot.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    KTR_StudySubsetDefinitionSnapshot.Fields.KTR_SubsetDefinition2,
                    KTR_StudySubsetDefinitionSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_StudySubsetDefinitionSnapshot.Fields.KTR_Study, ConditionOperator.Equal, studyId),
                        new ConditionExpression(KTR_StudySubsetDefinitionSnapshot.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_StudySubsetDefinitionSnapshot_StatusCode.Active),
                    }
                },
                NoLock = true,
            };

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_StudySubsetDefinitionSnapshot>())
                .ToList();
        }
        #endregion

        #region Queries to Dataverse - Study Subset Entities Snapshots
        private void InsertStudyQlsSubsetEntitiesSnapshots(
            IOrganizationService service,
            ITracingService tracingService,
            Guid studyId,
            IList<KTR_SubsetEntities> subsetEntities,
            IList<KTR_StudyQuestionnaireLineSnapshot> questionnaireLineSnapshots,
            IList<KTR_StudySubsetDefinitionSnapshot> studySubsetSnapshots)
        {
            tracingService.Trace($"------> questionnaireLineSnapshots COUNT: {questionnaireLineSnapshots.Count}.");
            tracingService.Trace($"------> studySubsetSnapshots COUNT: {studySubsetSnapshots.Count}.");
            tracingService.Trace($"------> subsetEntities COUNT: {subsetEntities.Count}.");

            if (questionnaireLineSnapshots != null && questionnaireLineSnapshots.Count > 0
                && studySubsetSnapshots != null && studySubsetSnapshots.Count > 0
                && subsetEntities != null && subsetEntities.Count > 0)
            {
                var requestCollection = new OrganizationRequestCollection();

                var mlEntitiesIds = subsetEntities
                    .Select(x => x.KTR_ManagedListEntity.Id)
                    .Where(x => x != null)
                    .ToList();

                var mlEntities = GetManagedListEntitiesByIds(service, mlEntitiesIds);

                if (mlEntities == null || mlEntities.Count == 0)
                {
                    tracingService.Trace($"No Managed List Entities found for Subset Entities of Study: {studyId}");
                    return;
                }

                foreach (var qlSnapshot in questionnaireLineSnapshots)
                {
                    var subsetSnapshots = studySubsetSnapshots
                        .Where(x => x.KTR_QuestionnaireLinesNaPsHot.Id == qlSnapshot.Id)
                        .ToList();

                    foreach (var subsetSnapshot in subsetSnapshots)
                    {
                        var entities = subsetEntities
                            .Where(x => x.KTR_SubsetDeFinTion.Id == subsetSnapshot.KTR_SubsetDefinition2.Id)
                            .ToList();

                        foreach (var entity in entities)
                        {
                            var snapshotSubsetEntity = new Entity(KTR_StudySubsetEntitiesSnapshot.EntityLogicalName);
                            snapshotSubsetEntity[KTR_StudySubsetEntitiesSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot] = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, qlSnapshot.Id);
                            snapshotSubsetEntity[KTR_StudySubsetEntitiesSnapshot.Fields.KTR_SubsetDefinitionSnapshot] = new EntityReference(KTR_StudySubsetDefinitionSnapshot.EntityLogicalName, subsetSnapshot.Id);
                            snapshotSubsetEntity[KTR_StudySubsetEntitiesSnapshot.Fields.KTR_SubsetEntities] = new EntityReference(KTR_SubsetEntities.EntityLogicalName, entity.Id);
                            snapshotSubsetEntity[KTR_StudySubsetEntitiesSnapshot.Fields.KTR_Study] = new EntityReference(KT_Study.EntityLogicalName, studyId);
                            snapshotSubsetEntity[KTR_StudySubsetEntitiesSnapshot.Fields.KTR_Name] = entity.KTR_Name;

                            var mlEntity = mlEntities.FirstOrDefault(x => x.Id == entity.KTR_ManagedListEntity.Id);
                            if (mlEntity != null)
                            {
                                snapshotSubsetEntity[KTR_StudySubsetEntitiesSnapshot.Fields.KTR_AnswerCode] = mlEntity.KTR_AnswerCode;
                                snapshotSubsetEntity[KTR_StudySubsetEntitiesSnapshot.Fields.KTR_AnswerText] = mlEntity.KTR_AnswerText;
                                snapshotSubsetEntity[KTR_StudySubsetEntitiesSnapshot.Fields.KTR_DisplayOrder] = mlEntity.KTR_DisplayOrder;
                            }

                            requestCollection.Add(new CreateRequest { Target = snapshotSubsetEntity });
                        }
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
                    throw new InvalidPluginExecutionException($"Some error happening while inserting Study Subset Entities Snapshots (see plugin trace for details)");
                }
            }
            else
            {
                tracingService.Trace($"No StudyQlsSubsetEntitiesSnapshots were insereted.");
            }
        }
        #endregion

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

        #region Queries to Dataverse - Study QuestionnaireLines Subsets
        private List<KTR_QuestionnaireLineSubset> GetStudyQuestionnaireLinesSubsets(
            IOrganizationService service,
            Guid studyId,
            IList<Guid> questionnaireLineIds)
        {
            if (questionnaireLineIds == null || questionnaireLineIds.Count == 0)
            {
                return new List<KTR_QuestionnaireLineSubset>();
            }

            var query = new QueryExpression()
            {
                EntityName = KTR_QuestionnaireLineSubset.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_QuestionnaireLineSubset.Fields.KTR_QuestionnaireLineId, ConditionOperator.In, questionnaireLineIds.Cast<object>().ToArray()),
                        new ConditionExpression(KTR_QuestionnaireLineSubset.Fields.KTR_Study, ConditionOperator.Equal, studyId),
                        new ConditionExpression(KTR_QuestionnaireLineSubset.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_QuestionnaireLineSubset_StatusCode.Active),
                    }
                },
                NoLock = true,
            };

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_QuestionnaireLineSubset>())
                .ToList();
        }
        #endregion

        #region Queries to Dataverse - QuestionnaireLineManagedList
        private List<KTR_QuestionnaireLinesHaRedList> GetQuestionnaireLinesManagedList(IOrganizationService service, IList<Guid> questionnaireLineIds)
        {
            var query = new QueryExpression()
            {
                EntityName = KTR_QuestionnaireLinesHaRedList.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLine, ConditionOperator.In, questionnaireLineIds.Cast<object>().ToArray()),
                        new ConditionExpression(KTR_QuestionnaireLinesHaRedList.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_QuestionnaireLinesHaRedList_StatusCode.Active),
                    }
                },
                NoLock = true,
            };

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_QuestionnaireLinesHaRedList>())
                .ToList();
        }
        #endregion

        #region Queries to Dataverse - Study
        private void UpdateStudy(IOrganizationService service, Entity study)
        {
            service.Update(study);
        }

        #endregion

        #region Queries to Dataverse - ManagedList
        private List<KTR_ManagedList> GetManagedLists(IOrganizationService service, IList<Guid> managedListIds)
        {
            if (managedListIds == null || managedListIds.Count == 0)
            {
                return new List<KTR_ManagedList>();
            }

            var query = new QueryExpression
            {
                EntityName = KTR_ManagedList.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    KTR_ManagedList.Fields.Id,
                    KTR_ManagedList.Fields.KTR_EverInSnapshot),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_ManagedList.Fields.Id, ConditionOperator.In, managedListIds.Cast<object>().ToArray())
                    }
                },
                NoLock = true
            };

            return service.RetrieveMultiple(query).Entities
                .Select(x => x.ToEntity<KTR_ManagedList>())
                .ToList();
        }

        private void UpdateBulkManagedLists(
            IOrganizationService service,
            ITracingService tracingService,
            List<KTR_ManagedList> managedLists)
        {
            if (managedLists == null || managedLists.Count == 0)
            {
                tracingService.Trace($"No Managed Lists found to update.");
                return;
            }

            var requestCollection = new OrganizationRequestCollection();

            var currentTime = DateTime.UtcNow;
            foreach (var ml in managedLists)
            {
                var update = new Entity(ml.LogicalName, ml.Id);
                update[KTR_ManagedList.Fields.KTR_EverInSnapshot] = true;
                update[KTR_ManagedList.Fields.KTR_FirstSnapshotDate] = currentTime;
                requestCollection.Add(new UpdateRequest { Target = update });
            }

            if (requestCollection.Count > 0)
            {
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
                    throw new InvalidPluginExecutionException(
                        $"Some error happened while updating Managed List records (see plugin trace for details)");
                }
            }
        }
        #endregion

        #region Queries to Dataverse - ManagedListEntities
        private List<KTR_ManagedListEntity> GetManagedListEntities(IOrganizationService service, IList<Guid> managedListIds)
        {
            if (managedListIds == null || managedListIds.Count == 0)
            {
                return new List<KTR_ManagedListEntity>();
            }

            var query = new QueryExpression
            {
                EntityName = KTR_ManagedListEntity.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    KTR_ManagedListEntity.Fields.KTR_ManagedList,
                    KTR_ManagedListEntity.Fields.KTR_EverInSnapshot,
                    KTR_ManagedListEntity.Fields.KTR_AnswerText,
                    KTR_ManagedListEntity.Fields.KTR_DisplayOrder,
                    KTR_ManagedListEntity.Fields.CreatedOn),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_ManagedListEntity.Fields.KTR_ManagedList, ConditionOperator.In, managedListIds.Cast<object>().ToArray()),
                        new ConditionExpression(KTR_ManagedListEntity.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ManagedListEntity_StatusCode.Active)

                    }
                },
                NoLock = true
            };

            return service.RetrieveMultiple(query).Entities
                .Select(x => x.ToEntity<KTR_ManagedListEntity>())
                .ToList();
        }

        private List<KTR_ManagedListEntity> GetManagedListEntitiesByIds(IOrganizationService service, IList<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return new List<KTR_ManagedListEntity>();
            }

            var query = new QueryExpression
            {
                EntityName = KTR_ManagedListEntity.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    KTR_ManagedListEntity.Fields.KTR_ManagedListEntityId,
                    KTR_ManagedListEntity.Fields.KTR_ManagedList,
                    KTR_ManagedListEntity.Fields.KTR_AnswerCode,
                    KTR_ManagedListEntity.Fields.KTR_AnswerText,
                    KTR_ManagedListEntity.Fields.KTR_DisplayOrder),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_ManagedListEntity.Fields.KTR_ManagedListEntityId, ConditionOperator.In, ids.Cast<object>().ToArray()),
                        new ConditionExpression(KTR_ManagedListEntity.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ManagedListEntity_StatusCode.Active)
                    }
                },
                NoLock = true
            };

            return service.RetrieveMultiple(query).Entities
                .Select(x => x.ToEntity<KTR_ManagedListEntity>())
                .ToList();
        }

        private void UpdateBulkManagedListEntities(
            IOrganizationService service,
            ITracingService tracingService,
            List<KTR_ManagedListEntity> managedListEntities)
        {
            if (managedListEntities == null || managedListEntities.Count == 0)
            {
                tracingService.Trace($"No Managed List Entities found to update.");
                return;
            }

            var requestCollection = new OrganizationRequestCollection();

            foreach (var mle in managedListEntities)
            {
                var update = new Entity(mle.LogicalName, mle.Id);
                update[KTR_ManagedListEntity.Fields.KTR_EverInSnapshot] = true;
                requestCollection.Add(new UpdateRequest { Target = update });
            }

            if (requestCollection.Count > 0)
            {
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
                    throw new InvalidPluginExecutionException(
                        $"Some error happened while updating Managed List Entity records (see plugin trace for details)");
                }
            }
        }
        #endregion

        #region Queries to Dataverse - QuestionnaireLineManagedListEntity

        private List<KTR_QuestionnaireLinemanAgedListEntity> GetQLManagedListEntities(
            IOrganizationService service,
            Guid studyId)
        {
            var query = new QueryExpression
            {
                EntityName = KTR_QuestionnaireLinemanAgedListEntity.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.Id,
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_QuestionnaireLine,
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_ManagedList,
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_ManagedListEntity),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_StudyId,
                            ConditionOperator.Equal,
                            studyId
                        ),
                        new ConditionExpression(
                            KTR_QuestionnaireLinemanAgedListEntity.Fields.StatusCode,
                            ConditionOperator.Equal,
                            (int)KTR_QuestionnaireLinemanAgedListEntity_StatusCode.Active
                        )
                    }
                },
                NoLock = true
            };

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_QuestionnaireLinemanAgedListEntity>())
                .ToList();
        }

        #endregion

        #region Queries to Dataverse - Subset Definition
        private void UpdateBulkSubsets(
            IOrganizationService service,
            ITracingService tracingService,
            List<Guid> subsetIds)
        {
            if (subsetIds == null || subsetIds.Count == 0)
            {
                tracingService.Trace($"No Subsets found to update.");
                return;
            }

            var requestCollection = new OrganizationRequestCollection();

            var currentTime = DateTime.UtcNow;
            foreach (var subsetId in subsetIds)
            {
                var update = new Entity(KTR_SubsetDefinition.EntityLogicalName, subsetId);
                update[KTR_SubsetDefinition.Fields.KTR_EverInSnapshot] = true;
                update[KTR_SubsetDefinition.Fields.KTR_FirstSnapshotDate] = currentTime;
                requestCollection.Add(new UpdateRequest { Target = update });
            }

            if (requestCollection.Count > 0)
            {
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
                    throw new InvalidPluginExecutionException(
                        $"Some error happened while updating Subset Definition records (see plugin trace for details)");
                }
            }
        }

        private List<KTR_SubsetDefinition> GetSubsetsBySubsetIds(IOrganizationService service, IList<Guid> subsetIds)
        {
            if (subsetIds == null || subsetIds.Count == 0)
            {
                return new List<KTR_SubsetDefinition>();
            }

            var query = new QueryExpression()
            {
                EntityName = KTR_SubsetDefinition.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_SubsetDefinition.Fields.Id, ConditionOperator.In, subsetIds.Cast<object>().ToArray()),
                        new ConditionExpression(KTR_SubsetDefinition.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_SubsetDefinition_StatusCode.Active),
                    }
                },
                NoLock = true,
            };

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_SubsetDefinition>())
                .ToList();
        }

        #endregion

        #region Queries to Dataverse - Subset Entities
        private List<KTR_SubsetEntities> GetSubsetEntitiesBySubsetIds(IOrganizationService service, IList<Guid> subsetIds)
        {
            if (subsetIds == null || subsetIds.Count == 0)
            {
                return new List<KTR_SubsetEntities>();
            }

            var query = new QueryExpression()
            {
                EntityName = KTR_SubsetEntities.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_SubsetEntities.Fields.KTR_SubsetDeFinTion, ConditionOperator.In, subsetIds.Cast<object>().ToArray()),
                        new ConditionExpression(KTR_SubsetEntities.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_SubsetEntities_StatusCode.Active),
                    }
                },
                NoLock = true,
            };

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_SubsetEntities>())
                .ToList();
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
