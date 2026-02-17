namespace Kantar.StudyDesignerLite.Plugins.Study
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Mappers;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    public class CreateStudyChangelogPostOperation : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.CreateStudyChangelogPostOperation";

        public CreateStudyChangelogPostOperation() : base(typeof(CreateStudyChangelogPostOperation))
        {
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            var tracingService = localPluginContext.TracingService;
            var context = localPluginContext.PluginExecutionContext;
            var service = localPluginContext.SystemUserService;

            context.InputParameters.TryGetValue("Target", out Entity target);

            tracingService.Trace($"{PluginName} {target.LogicalName}");

            if (target.LogicalName == KT_Study.EntityLogicalName)
            {
                var study = target.ToEntity<KT_Study>();

                if (context.MessageName == nameof(ContextMessageEnum.Update))
                {
                    CreateStudyChangelog(service, tracingService, study);
                }
            }
        }

        #region Create Study Changelog
        private void CreateStudyChangelog(IOrganizationService service, ITracingService tracingService, KT_Study target)
        {
            var study = GetStudy(service, target.Id);

            if (study.KTR_IsSnapshotCreated != true)
            {
                tracingService.Trace("Changelog not created: Snapshot creation flag is false. Exiting.");
                return;
            }

            var parentStudyId = GetParentStudyIdForChangelog(study, service);

            if (parentStudyId == null)
            {
                tracingService.Trace("Changelog not created: no parent study found. Exiting.");
                return;
            }

            var studyIds = new List<Guid> { study.Id, parentStudyId.Value };

            var questionnaireLineSnapshots = GetStudyQuestionnaireLineSnapshots(service, studyIds);
            tracingService.Trace("GetStudyQuestionnaireLineSnapshots executed.");
            var questionnaireLineSnapshotIds = EntityHelpers.JoinIds(questionnaireLineSnapshots, KTR_StudyQuestionnaireLineSnapshot.Fields.Id);

            IList<KTR_StudyQuestionAnswerListSnapshot> questionnaireLineAnswerSnapshots = null;
            IList<KTR_StudyQuestionManagedListSnapshot> questionnaireLineManagedListSnapshots = null;

            System.Threading.Tasks.Parallel.Invoke(
                () => questionnaireLineAnswerSnapshots = GetStudyQuestionAnswerListSnapshots(service, questionnaireLineSnapshotIds),
                () => questionnaireLineManagedListSnapshots = GetStudyQuestionManagedListSnapshots(service, questionnaireLineSnapshotIds)
            );

            tracingService.Trace("Parallel queries completed. (GetStudyQuestionAnswerListSnapshots and GetStudyQuestionManagedListSnapshots)");

            var managedListSnapshotIds = EntityHelpers.JoinIds(questionnaireLineManagedListSnapshots, KTR_StudyQuestionManagedListSnapshot.Fields.Id);
            var managedListEntitySnapshots = GetStudyManagedListEntitySnapshots(service, managedListSnapshotIds);
            tracingService.Trace("GetStudyManagedListEntitySnapshots executed.");

            // Compare Questions
            var questionChangeLogs = CreateQuestionChangeLogRows(study.Id, parentStudyId.Value, questionnaireLineSnapshots);
            tracingService.Trace("CreateQuestionChangeLogRows executed.");

            // Compare Answers
            var answerChangeLogs = CreateAnswersChangeLogRows(questionChangeLogs, study.Id, parentStudyId.Value, questionnaireLineSnapshots, questionnaireLineAnswerSnapshots);
            tracingService.Trace("CreateAnswersChangeLogRows executed.");

            // Compare Managed Lists
            var managedListChangeLogs = CreateManagedListChangeLogRows(questionChangeLogs, study.Id, parentStudyId.Value, questionnaireLineSnapshots, questionnaireLineManagedListSnapshots);
            tracingService.Trace("CreateManagedListChangeLogRows executed.");

            //Compare Managed List Entities
            var managedListEntityChangeLogs = CreateManagedListEntityChangeLogRows(questionChangeLogs, study.Id, parentStudyId.Value, questionnaireLineSnapshots, managedListEntitySnapshots);
            tracingService.Trace("CreateManagedListEntityChangeLogRows executed.");

            // Insert ChangeLog rows
            var studyChangelogsToInsert = new List<KTR_StudySnapshotLineChangelog>();
            studyChangelogsToInsert.AddRange(questionChangeLogs);
            studyChangelogsToInsert.AddRange(answerChangeLogs);
            studyChangelogsToInsert.AddRange(managedListChangeLogs);
            studyChangelogsToInsert.AddRange(managedListEntityChangeLogs);
            InsertStudySnapshotLineChangeLog(service, studyChangelogsToInsert);
            tracingService.Trace("InsertStudySnapshotLineChangeLog executed.");
        }

        #endregion

        #region Version Comparisons - Questions
        private IList<KTR_StudySnapshotLineChangelog> CreateQuestionChangeLogRows(
            Guid currentStudyId,
            Guid parentStudyId,
            IList<KTR_StudyQuestionnaireLineSnapshot> questionnaireLineSnapshots)
        {
            var changelogs = new List<KTR_StudySnapshotLineChangelog>();

            if (questionnaireLineSnapshots != null && questionnaireLineSnapshots.Count > 0)
            {
                var currentQuestionSnapshots = questionnaireLineSnapshots
                    .Where(x => x.KTR_Study.Id == currentStudyId)
                    .ToList();
                var parentQuestionSnapshots = questionnaireLineSnapshots
                    .Where(x => x.KTR_Study.Id == parentStudyId)
                    .ToList();

                var currentDict = currentQuestionSnapshots
                    .Where(l => l.KTR_QuestionnaireLine != null)
                    .ToDictionary(l => l.KTR_QuestionnaireLine.Id, l => (Entity)l);
                var parentDict = parentQuestionSnapshots
                    .Where(l => l.KTR_QuestionnaireLine != null)
                    .ToDictionary(l => l.KTR_QuestionnaireLine.Id, l => (Entity)l);

                var questionCompareResult = VersionCompareHelpers.CompareEntities(
                    currentDict,
                    parentDict,
                    KTR_ChangelogRelatedObject.Question);

                // Added Questions
                foreach (var addedId in questionCompareResult.AddedEntityIds)
                {
                    var questionnaireSnapshot = currentQuestionSnapshots.FirstOrDefault(x => x.KTR_QuestionnaireLine.Id == addedId);

                    if (questionnaireSnapshot.KTR_Module2 != null && questionnaireSnapshot.KTR_Module2.Id != Guid.Empty)
                    {
                        var log = StudySnapshotLineChangelogMapper.MapModuleAdded(currentStudyId, parentStudyId, questionnaireSnapshot, questionnaireSnapshot.KTR_Module2.Id);
                        changelogs.Add(log);
                    }
                    else
                    {
                        var log = StudySnapshotLineChangelogMapper.MapQuestionAdded(currentStudyId, parentStudyId, questionnaireSnapshot);
                        changelogs.Add(log);
                    }

                }

                // Removed Questions
                foreach (var removedId in questionCompareResult.RemovedEntityIds)
                {
                    var parentQuestionnaireSnapshot = parentQuestionSnapshots.FirstOrDefault(x => x.KTR_QuestionnaireLine.Id == removedId);

                    if (parentQuestionnaireSnapshot?.KTR_Module2 == null || parentQuestionnaireSnapshot.KTR_Module2.Id == Guid.Empty)
                    {
                        // No module info – treat as question removal
                        var log = StudySnapshotLineChangelogMapper.MapQuestionRemoved(currentStudyId, parentStudyId, parentQuestionnaireSnapshot);
                        changelogs.Add(log);
                        continue;
                    }

                    var removedModuleId = parentQuestionnaireSnapshot.KTR_Module2.Id;

                    // Check if any snapshot in the *current* study still uses this module
                    var isModuleStillInUse = currentQuestionSnapshots
                        .Any(s => s.KTR_Module2?.Id == removedModuleId);

                    if (isModuleStillInUse)
                    {
                        // Module still exists in current study – just a question removed
                        var log = StudySnapshotLineChangelogMapper.MapQuestionRemoved(currentStudyId, parentStudyId, parentQuestionnaireSnapshot);
                        changelogs.Add(log);
                    }
                    else
                    {
                        // Module no longer used in current study – entire module removed
                        var log = StudySnapshotLineChangelogMapper.MapModuleRemoved(currentStudyId, parentStudyId, parentQuestionnaireSnapshot, parentQuestionnaireSnapshot.KTR_Module2.Id);
                        changelogs.Add(log);
                    }
                }

                // Modified Questions
                foreach (var commonId in questionCompareResult.CommonEntityIds)
                {
                    var questionnaireSnapshot = (KTR_StudyQuestionnaireLineSnapshot)currentDict[commonId];
                    var parentQuestionnaireSnapshot = (KTR_StudyQuestionnaireLineSnapshot)parentDict[commonId];

                    var fieldChangedList = questionCompareResult.FieldsChangedResults
                        .Where(x => x.Entity.Id == questionnaireSnapshot.Id);

                    foreach (var fieldChange in fieldChangedList)
                    {
                        var log = StudySnapshotLineChangelogMapper.MapModifiedQuestion(currentStudyId, parentStudyId, questionnaireSnapshot, parentQuestionnaireSnapshot, fieldChange);

                        changelogs.Add(log);
                    }
                }

                //Modified Question Order
                var orderedCurrentQuestions = questionCompareResult.CommonEntityIds.OrderBy(
                    id => ((KTR_StudyQuestionnaireLineSnapshot)currentDict[id]).KTR_SortOrder).ToArray();

                var orderedParentQuestions = questionCompareResult.CommonEntityIds.OrderBy(
                    id => ((KTR_StudyQuestionnaireLineSnapshot)parentDict[id]).KTR_SortOrder).ToArray();

                var diffResult = VersionCompareHelpers.MyersDiffAlgorithm(orderedParentQuestions, orderedCurrentQuestions);

                changelogs.AddRange(StudySnapshotLineChangelogMapper.MapModifiedQuestionOrder(currentStudyId, parentStudyId, currentDict, parentDict, diffResult));

            }
            return changelogs;
        }
        #endregion

        #region Version Comparisons - Answers
        private IList<KTR_StudySnapshotLineChangelog> CreateAnswersChangeLogRows(
            IList<KTR_StudySnapshotLineChangelog> questionChangeLogs,
            Guid currentStudyId,
            Guid parentStudyId,
            IList<KTR_StudyQuestionnaireLineSnapshot> qlSnapshots,
            IList<KTR_StudyQuestionAnswerListSnapshot> qlAnswersSnapshots)
        {
            var changelogs = new List<KTR_StudySnapshotLineChangelog>();

            if (qlAnswersSnapshots != null && qlAnswersSnapshots.Count > 0)
            {
                var currentQlSnapshots = qlSnapshots
                    .Where(x => x.KTR_Study.Id == currentStudyId)
                    .ToDictionary(x => x.Id, x => x);
                var parentQlSnapshots = qlSnapshots
                    .Where(x => x.KTR_Study.Id == parentStudyId)
                    .ToDictionary(x => x.Id, x => x);

                var currentAnswersSnapshots = qlAnswersSnapshots
                    .Where(x => currentQlSnapshots.ContainsKey(x.KTR_QuestionnaireLinesNaPsHot.Id))
                    .ToList();
                var parentAnswersSnapshots = qlAnswersSnapshots
                    .Where(x => parentQlSnapshots.ContainsKey(x.KTR_QuestionnaireLinesNaPsHot.Id))
                    .ToList();

                var currentDict = currentAnswersSnapshots
                    .Where(l => l.KTR_QuestionnaireLinesAnswerList != null)
                    .ToDictionary(l => l.KTR_QuestionnaireLinesAnswerList.Id, l => (Entity)l);

                var parentDict = parentAnswersSnapshots
                    .Where(l => l.KTR_QuestionnaireLinesAnswerList != null)
                    .ToDictionary(l => l.KTR_QuestionnaireLinesAnswerList.Id, l => (Entity)l);

                var answersCompareResult = VersionCompareHelpers.CompareEntities(
                   currentDict,
                   parentDict,
                   KTR_ChangelogRelatedObject.Answer);

                // Added Answers
                foreach (var addedId in answersCompareResult.AddedEntityIds)
                {
                    var qlAnswerSnapshot = currentAnswersSnapshots.FirstOrDefault(x => x.KTR_QuestionnaireLinesAnswerList.Id == addedId);

                    var log = StudySnapshotLineChangelogMapper.MapAnswerAdded(currentStudyId, parentStudyId, qlAnswerSnapshot.KTR_QuestionnaireLinesNaPsHot, qlAnswerSnapshot);

                    var questionWasJustAdded = CheckIfQuestionWasJustAddedOrRemoved(questionChangeLogs, log, KTR_ChangelogType.QuestionAdded);

                    if (!questionWasJustAdded)
                    {
                        changelogs.Add(log);
                    }
                }

                // Removed Answers
                foreach (var removedId in answersCompareResult.RemovedEntityIds)
                {
                    var parentQlAnswerSnapshot = parentAnswersSnapshots.FirstOrDefault(x => x.KTR_QuestionnaireLinesAnswerList.Id == removedId);

                    var log = StudySnapshotLineChangelogMapper
                        .MapAnswerRemoved(currentStudyId, parentStudyId, parentQlAnswerSnapshot.KTR_QuestionnaireLinesNaPsHot, parentQlAnswerSnapshot);

                    var questionWasJustRemoved = CheckIfQuestionWasJustAddedOrRemoved(questionChangeLogs, log, KTR_ChangelogType.QuestionRemoved);

                    if (!questionWasJustRemoved)
                    {
                        changelogs.Add(log);
                    }
                }

                // Modified Answers
                foreach (var commonId in answersCompareResult.CommonEntityIds)
                {
                    var qlAnswerSnapshot = currentAnswersSnapshots.FirstOrDefault(x => x.KTR_QuestionnaireLinesAnswerList.Id == commonId);
                    var parentQlAnswerSnapshot = parentAnswersSnapshots.FirstOrDefault(x => x.KTR_QuestionnaireLinesAnswerList.Id == commonId);

                    var fieldChangedList = answersCompareResult.FieldsChangedResults
                        .Where(x => x.Entity.Id == qlAnswerSnapshot.Id);

                    foreach (var fieldChange in fieldChangedList)
                    {
                        var log = StudySnapshotLineChangelogMapper.MapModifiedAnswer(currentStudyId, parentStudyId, qlAnswerSnapshot.KTR_QuestionnaireLinesNaPsHot, qlAnswerSnapshot, fieldChange);

                        changelogs.Add(log);
                    }
                }

                //Modified Answer Order
                var currentQuestionToAnswers = new Dictionary<Guid, List<KTR_StudyQuestionAnswerListSnapshot>>();
                var parentQuestionToAnswers = new Dictionary<Guid, List<KTR_StudyQuestionAnswerListSnapshot>>();

                foreach (var commonAnswerId in answersCompareResult.CommonEntityIds)
                {
                    var currentAnswer = (KTR_StudyQuestionAnswerListSnapshot)currentDict[commonAnswerId];
                    var currentQuestionSnapshotId = currentAnswer.KTR_QuestionnaireLinesNaPsHot.Id;
                    var currentQuestionId = currentQlSnapshots[currentQuestionSnapshotId].KTR_QuestionnaireLine.Id;
                    if (!currentQuestionToAnswers.ContainsKey(currentQuestionId))
                    {
                        currentQuestionToAnswers[currentQuestionId] = new List<KTR_StudyQuestionAnswerListSnapshot>();
                    }
                    currentQuestionToAnswers[currentQuestionId].Add(currentAnswer);

                    var parentAnswer = (KTR_StudyQuestionAnswerListSnapshot)parentDict[commonAnswerId];
                    var parentQuestionSnapshotId = parentAnswer.KTR_QuestionnaireLinesNaPsHot.Id;
                    var parentQuestionId = parentQlSnapshots[parentQuestionSnapshotId].KTR_QuestionnaireLine.Id;
                    if (!parentQuestionToAnswers.ContainsKey(parentQuestionId))
                    {
                        parentQuestionToAnswers[parentQuestionId] = new List<KTR_StudyQuestionAnswerListSnapshot>();
                    }
                    parentQuestionToAnswers[parentQuestionId].Add(parentAnswer);
                }

                foreach (var currentKeyVal in currentQuestionToAnswers)
                {
                    bool hasVal = parentQuestionToAnswers.TryGetValue(currentKeyVal.Key, out var parentAnswers);
                    if (!hasVal)
                    {
                        continue;
                    }
                    var orderedCurrentAnswers = currentKeyVal.Value.OrderBy(val => val.KTR_DisplayOrder)
                        .Select(x => x.KTR_QuestionnaireLinesAnswerList.Id).ToArray();
                    var orderedParentAnswers = parentAnswers.OrderBy(val => val.KTR_DisplayOrder)
                        .Select(x => x.KTR_QuestionnaireLinesAnswerList.Id).ToArray();

                    var diffResult = VersionCompareHelpers.MyersDiffAlgorithm(orderedParentAnswers, orderedCurrentAnswers);

                    changelogs.AddRange(StudySnapshotLineChangelogMapper.MapModifiedAnswerOrder(currentStudyId, parentStudyId, currentDict, parentDict, diffResult));
                }
            }
            return changelogs;
        }
        #endregion

        #region Version Comparisons - Managed List
        private IList<KTR_StudySnapshotLineChangelog> CreateManagedListChangeLogRows(
            IList<KTR_StudySnapshotLineChangelog> questionChangeLogs,
            Guid currentStudyId,
            Guid parentStudyId,
            IList<KTR_StudyQuestionnaireLineSnapshot> qlSnapshots,
            IList<KTR_StudyQuestionManagedListSnapshot> qlManagedListSnapshots)
        {
            var changelogs = new List<KTR_StudySnapshotLineChangelog>();

            if (qlManagedListSnapshots != null && qlManagedListSnapshots.Count > 0)
            {
                var currentQlSnapshots = qlSnapshots
                    .Where(x => x.KTR_Study.Id == currentStudyId)
                    .ToDictionary(x => x.Id, x => x);
                var parentQlSnapshots = qlSnapshots
                    .Where(x => x.KTR_Study.Id == parentStudyId)
                    .ToDictionary(x => x.Id, x => x);

                var currentManagedListSnapshots = qlManagedListSnapshots
                    .Where(x => currentQlSnapshots.ContainsKey(x.KTR_QuestionnaireLinesNaPsHot.Id))
                    .ToList();
                var parentManagedListSnapshots = qlManagedListSnapshots
                    .Where(x => parentQlSnapshots.ContainsKey(x.KTR_QuestionnaireLinesNaPsHot.Id))
                    .ToList();

                var currentDict = currentManagedListSnapshots
                    .Where(l => l.KTR_QuestionnaireLineManagedList != null)
                    .ToDictionary(l => l.KTR_QuestionnaireLineManagedList.Id, l => (Entity)l);

                var parentDict = parentManagedListSnapshots
                    .Where(l => l.KTR_QuestionnaireLineManagedList != null)
                    .ToDictionary(l => l.KTR_QuestionnaireLineManagedList.Id, l => (Entity)l);

                var managedListsCompareResult = VersionCompareHelpers.CompareEntities(
                   currentDict,
                   parentDict,
                   KTR_ChangelogRelatedObject.ManagedList);

                // Added Managed Lists
                foreach (var addedId in managedListsCompareResult.AddedEntityIds)
                {
                    var qlManagedListSnapshot = currentManagedListSnapshots.FirstOrDefault(x => x.KTR_QuestionnaireLineManagedList.Id == addedId);

                    var log = StudySnapshotLineChangelogMapper.MapManagedListAdded(currentStudyId, parentStudyId, qlManagedListSnapshot.KTR_QuestionnaireLinesNaPsHot, qlManagedListSnapshot);

                    var questionWasJustAdded = CheckIfQuestionWasJustAddedOrRemoved(questionChangeLogs, log, KTR_ChangelogType.QuestionAdded);

                    if (!questionWasJustAdded)
                    {
                        changelogs.Add(log);
                    }
                }

                // Removed Managed List
                foreach (var removedId in managedListsCompareResult.RemovedEntityIds)
                {
                    var parentQlManagedSnapshot = parentManagedListSnapshots.FirstOrDefault(x => x.KTR_QuestionnaireLineManagedList.Id == removedId);

                    var log = StudySnapshotLineChangelogMapper
                        .MapManagedListRemoved(currentStudyId, parentStudyId, parentQlManagedSnapshot.KTR_QuestionnaireLineManagedList, parentQlManagedSnapshot);

                    var questionWasJustRemoved = CheckIfQuestionWasJustAddedOrRemoved(questionChangeLogs, log, KTR_ChangelogType.QuestionRemoved);

                    if (!questionWasJustRemoved)
                    {
                        changelogs.Add(log);
                    }
                }

                // Modified Managed List
                foreach (var commonId in managedListsCompareResult.CommonEntityIds)
                {
                    var qlManagedListSnapshot = currentManagedListSnapshots.FirstOrDefault(x => x.KTR_QuestionnaireLineManagedList.Id == commonId);
                    var parentQlManagedListSnapshot = parentManagedListSnapshots.FirstOrDefault(x => x.KTR_QuestionnaireLineManagedList.Id == commonId);

                    var fieldChangedList = managedListsCompareResult.FieldsChangedResults
                        .Where(x => x.Entity.Id == qlManagedListSnapshot.Id);

                    foreach (var fieldChange in fieldChangedList)
                    {
                        var log = StudySnapshotLineChangelogMapper.MapModifiedManagedList(currentStudyId, parentStudyId, qlManagedListSnapshot.KTR_QuestionnaireLinesNaPsHot, qlManagedListSnapshot, fieldChange);

                        changelogs.Add(log);
                    }
                }
            }
            return changelogs;
        }
        #endregion

        #region Version Comparisons - Managed List Entity
        private IList<KTR_StudySnapshotLineChangelog> CreateManagedListEntityChangeLogRows(
            IList<KTR_StudySnapshotLineChangelog> questionChangeLogs,
            Guid currentStudyId,
            Guid parentStudyId,
            IList<KTR_StudyQuestionnaireLineSnapshot> qlSnapshots,
            IList<KTR_StudyManagedListEntitiesSnapshot> qlManagedListEntitySnapshots)
        {
            var changelogs = new List<KTR_StudySnapshotLineChangelog>();

            if (qlManagedListEntitySnapshots != null && qlManagedListEntitySnapshots.Count > 0)
            {
                var currentQlSnapshots = qlSnapshots
                    .Where(x => x.KTR_Study.Id == currentStudyId)
                    .ToDictionary(x => x.Id, x => x);
                var parentQlSnapshots = qlSnapshots
                    .Where(x => x.KTR_Study.Id == parentStudyId)
                    .ToDictionary(x => x.Id, x => x);

                var currentManagedListEntitySnapshots = qlManagedListEntitySnapshots
                    .Where(x => currentQlSnapshots.ContainsKey(x.KTR_QuestionnaireLinesNaPsHot.Id))
                    .ToList();
                var parentManagedListEntitySnapshots = qlManagedListEntitySnapshots
                    .Where(x => parentQlSnapshots.ContainsKey(x.KTR_QuestionnaireLinesNaPsHot.Id))
                    .ToList();

                // Build lookup of snapshotId -> stable question line id for current and parent
                var currentQlBySnapshot = qlSnapshots
                    .Where(x => x.KTR_Study.Id == currentStudyId)
                    .ToDictionary(x => x.Id, x => x.KTR_QuestionnaireLine?.Id ?? Guid.Empty);

                var parentQlBySnapshot = qlSnapshots
                    .Where(x => x.KTR_Study.Id == parentStudyId)
                    .ToDictionary(x => x.Id, x => x.KTR_QuestionnaireLine?.Id ?? Guid.Empty);

                // Group by stable question line id; within each question, key by stable managed list entity id
                var currentByQuestionLine = currentManagedListEntitySnapshots
                    .Where(s => s.KTR_QuestionnaireLinesNaPsHot != null && s.KTR_ManagedListEntity != null)
                    .Select(s => new { Snapshot = s, QuestionLineId = currentQlBySnapshot.TryGetValue(s.KTR_QuestionnaireLinesNaPsHot.Id, out var qlId) ? qlId : Guid.Empty })
                    .Where(x => x.QuestionLineId != Guid.Empty)
                    .GroupBy(x => x.QuestionLineId)
                    .ToDictionary(g => g.Key, g => g
                        .GroupBy(x => x.Snapshot.KTR_ManagedListEntity.Id)
                        .Select(gg => gg.First().Snapshot)
                        .ToDictionary(s => s.KTR_ManagedListEntity.Id, s => (Entity)s));

                var parentByQuestionLine = parentManagedListEntitySnapshots
                    .Where(s => s.KTR_QuestionnaireLinesNaPsHot != null && s.KTR_ManagedListEntity != null)
                    .Select(s => new { Snapshot = s, QuestionLineId = parentQlBySnapshot.TryGetValue(s.KTR_QuestionnaireLinesNaPsHot.Id, out var qlId) ? qlId : Guid.Empty })
                    .Where(x => x.QuestionLineId != Guid.Empty)
                    .GroupBy(x => x.QuestionLineId)
                    .ToDictionary(g => g.Key, g => g
                        .GroupBy(x => x.Snapshot.KTR_ManagedListEntity.Id)
                        .Select(gg => gg.First().Snapshot)
                        .ToDictionary(s => s.KTR_ManagedListEntity.Id, s => (Entity)s));

                // For each stable question line id, run comparison
                foreach (var questionLineId in currentByQuestionLine.Keys.Union(parentByQuestionLine.Keys))
                {
                    var currentDict = currentByQuestionLine.TryGetValue(questionLineId, out var cdict) ? cdict : new Dictionary<Guid, Entity>();
                    var parentDict = parentByQuestionLine.TryGetValue(questionLineId, out var pdict) ? pdict : new Dictionary<Guid, Entity>();

                    var managedListsEntityCompareResult = VersionCompareHelpers.CompareEntities(
                        currentDict,
                        parentDict,
                        KTR_ChangelogRelatedObject.ManagedListEntity);

                    // Added
                    foreach (var addedId in managedListsEntityCompareResult.AddedEntityIds)
                    {
                        var qlManagedListEntitySnapshot = currentManagedListEntitySnapshots
                            .FirstOrDefault(x =>
                                x.KTR_QuestionnaireLinesNaPsHot != null &&
                                x.KTR_ManagedListEntity != null &&
                                currentQlBySnapshot.TryGetValue(x.KTR_QuestionnaireLinesNaPsHot.Id, out var qLineId) &&
                                qLineId == questionLineId &&
                                x.KTR_ManagedListEntity.Id == addedId);

                        if (qlManagedListEntitySnapshot == null)
                        {
                            continue;
                        }

                        var log = StudySnapshotLineChangelogMapper.MapManagedListEntityAdded(
                            currentStudyId,
                            parentStudyId,
                            qlManagedListEntitySnapshot.KTR_QuestionnaireLinesNaPsHot,
                            qlManagedListEntitySnapshot);

                        var questionWasJustAdded = CheckIfQuestionWasJustAddedOrRemoved(questionChangeLogs, log, KTR_ChangelogType.QuestionAdded);
                        if (!questionWasJustAdded)
                        {
                            changelogs.Add(log);
                        }
                    }

                    // Removed
                    foreach (var removedId in managedListsEntityCompareResult.RemovedEntityIds)
                    {
                        var parentQlManagedListEntitySnapshot = parentManagedListEntitySnapshots
                            .FirstOrDefault(x =>
                                x.KTR_QuestionnaireLinesNaPsHot != null &&
                                x.KTR_ManagedListEntity != null &&
                                parentQlBySnapshot.TryGetValue(x.KTR_QuestionnaireLinesNaPsHot.Id, out var qLineId) &&
                                qLineId == questionLineId &&
                                x.KTR_ManagedListEntity.Id == removedId);

                        if (parentQlManagedListEntitySnapshot == null)
                        {
                            continue;
                        }

                        var log = StudySnapshotLineChangelogMapper.MapManagedListEntityRemoved(
                            currentStudyId,
                            parentStudyId,
                            parentQlManagedListEntitySnapshot.KTR_QuestionnaireLinesNaPsHot,
                            parentQlManagedListEntitySnapshot);

                        var questionWasJustRemoved = CheckIfQuestionWasJustAddedOrRemoved(questionChangeLogs, log, KTR_ChangelogType.QuestionRemoved);
                        if (!questionWasJustRemoved)
                        {
                            changelogs.Add(log);
                        }
                    }

                    // Modified
                    foreach (var commonId in managedListsEntityCompareResult.CommonEntityIds)
                    {
                        var qlManagedListEntitySnapshot = currentManagedListEntitySnapshots
                            .FirstOrDefault(x =>
                                x.KTR_QuestionnaireLinesNaPsHot != null &&
                                x.KTR_ManagedListEntity != null &&
                                currentQlBySnapshot.TryGetValue(x.KTR_QuestionnaireLinesNaPsHot.Id, out var qLineId) &&
                                qLineId == questionLineId &&
                                x.KTR_ManagedListEntity.Id == commonId);

                        var parentQlManagedListSnapshot = parentManagedListEntitySnapshots
                            .FirstOrDefault(x =>
                                x.KTR_QuestionnaireLinesNaPsHot != null &&
                                x.KTR_ManagedListEntity != null &&
                                parentQlBySnapshot.TryGetValue(x.KTR_QuestionnaireLinesNaPsHot.Id, out var qLineId) &&
                                qLineId == questionLineId &&
                                x.KTR_ManagedListEntity.Id == commonId);

                        if (qlManagedListEntitySnapshot == null || parentQlManagedListSnapshot == null)
                        {
                            continue;
                        }

                        var fieldChangedList = managedListsEntityCompareResult.FieldsChangedResults
                            .Where(x => x.Entity.Id == qlManagedListEntitySnapshot.Id);

                        foreach (var fieldChange in fieldChangedList)
                        {
                            var log = StudySnapshotLineChangelogMapper.MapModifiedManagedListEntity(
                                currentStudyId,
                                parentStudyId,
                                qlManagedListEntitySnapshot.KTR_QuestionnaireLinesNaPsHot,
                                qlManagedListEntitySnapshot,
                                fieldChange);

                            changelogs.Add(log);
                        }
                    }
                }
            }
            return changelogs;
        }
        #endregion

        #region Queries to Dataverse - Study
        private KT_Study GetStudy(IOrganizationService service, Guid studyId)
        {
            var study = service.Retrieve(
                KT_Study.EntityLogicalName,
                studyId,
                new ColumnSet(
                    KT_Study.Fields.KTR_IsSnapshotCreated,
                    KT_Study.Fields.KT_Project,
                    KT_Study.Fields.KTR_VersionNumber,
                    KT_Study.Fields.KTR_MasterStudy));

            return study
                .ToEntity<KT_Study>();
        }

        private Guid? GetParentStudyIdForChangelog(KT_Study study, IOrganizationService service)
        {
            var fetchQuery = new QueryExpression(KT_Study.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KT_Study.Fields.KT_StudyId, KT_Study.Fields.KTR_VersionNumber),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(KT_Study.Fields.KT_Project, ConditionOperator.Equal, study.KT_Project.Id),
                        new ConditionExpression(KT_Study.Fields.KTR_MasterStudy, ConditionOperator.Equal, study.KTR_MasterStudy?.Id ?? Guid.Empty),
                        new ConditionExpression(KT_Study.Fields.StatusCode, ConditionOperator.In, new int[] {
                            (int)KT_Study_StatusCode.ReadyForScripting,
                            (int)KT_Study_StatusCode.ApprovedForLaunch,
                            (int)KT_Study_StatusCode.Completed
                        }),
                        new ConditionExpression(KT_Study.Fields.KTR_VersionNumber, ConditionOperator.LessThan, study.KTR_VersionNumber)
                    }
                },
                Orders =
                {
                    new OrderExpression(KT_Study.Fields.KTR_VersionNumber, OrderType.Descending) // get latest version
                },
                TopCount = 1 // only fetch one
            };

            var parent = service.RetrieveMultiple(fetchQuery).Entities.FirstOrDefault();

            if (parent == null || !parent.Contains(KT_Study.Fields.KTR_VersionNumber))
            {
                return null;
            }

            return parent.GetAttributeValue<Guid>(KT_Study.Fields.KT_StudyId);
        }
        #endregion

        #region Queries to Dataverse - StudyQuestionnaireLineSnapshots
        private List<KTR_StudyQuestionnaireLineSnapshot> GetStudyQuestionnaireLineSnapshots(
            IOrganizationService service,
            IList<Guid> studyIds)
        {
            if (studyIds == null || studyIds.Count == 0)
            {
                return new List<KTR_StudyQuestionnaireLineSnapshot>();
            }

            var query = new QueryExpression()
            {
                EntityName = KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    KTR_StudyQuestionnaireLineSnapshot.Fields.Id,
                    KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_Study,
                    KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_QuestionnaireLine,
                    KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_Module2,
                    KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_SortOrder,
                    KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_Id,
                    KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_QuestionRationale,
                    KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_QuestionText,
                    KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_QuestionTitle,
                    KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_QuestionType,
                    KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_Name,
                    KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_QuestionVersion2,
                    KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_ScripterNotes,
                    KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_StandardOrCustom,
                    KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_IsDummyQuestion,
                    KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_Scriptlets),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_Study, ConditionOperator.In, studyIds.Cast<object>().ToArray()),
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
        #endregion

        #region Queries to Dataverse - StudyQuestionAnswerListSnapshot
        private List<KTR_StudyQuestionAnswerListSnapshot> GetStudyQuestionAnswerListSnapshots(
            IOrganizationService service,
            IList<Guid> questionnaireLineSnapshotIds)
        {
            if (questionnaireLineSnapshotIds == null || questionnaireLineSnapshotIds.Count == 0)
            {
                return new List<KTR_StudyQuestionAnswerListSnapshot>();
            }

            var query = new QueryExpression()
            {
                EntityName = KTR_StudyQuestionAnswerListSnapshot.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    KTR_StudyQuestionAnswerListSnapshot.Fields.Id,
                    KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot,
                    KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_QuestionnaireLinesAnswerList,
                    KTR_StudyQuestionAnswerListSnapshot.Fields.StatusCode,
                    KTR_StudyQuestionAnswerListSnapshot.Fields.StateCode,
                    KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_DisplayOrder,
                    KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_AnswerId,
                    KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_AnswerLocation,
                    KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_AnswerText,
                    KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_CustomProperty,
                    KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_EffectiveDate,
                    KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_EndDate,
                    KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_IsActive,
                    KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_IsExclusive,
                    KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_IsFixed,
                    KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_IsOpen,
                    KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_IsTranslatable,
                    KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_SourceId,
                    KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_SourceName,
                    KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_Version),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot, ConditionOperator.In, questionnaireLineSnapshotIds.Cast<object>().ToArray()),
                        new ConditionExpression(KTR_StudyQuestionAnswerListSnapshot.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_StudyQuestionAnswerListSnapshot_StatusCode.Active),
                    }
                },
                NoLock = true,
            };

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_StudyQuestionAnswerListSnapshot>())
                .ToList();
        }
        #endregion

        #region Queries to Dataverse - StudyQuestionManagedListSnapshot
        private List<KTR_StudyQuestionManagedListSnapshot> GetStudyQuestionManagedListSnapshots(
            IOrganizationService service,
            IList<Guid> questionnaireLineSnapshotIds)
        {
            if (questionnaireLineSnapshotIds == null || questionnaireLineSnapshotIds.Count == 0)
            {
                return new List<KTR_StudyQuestionManagedListSnapshot>();
            }

            var query = new QueryExpression()
            {
                EntityName = KTR_StudyQuestionManagedListSnapshot.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    KTR_StudyQuestionManagedListSnapshot.Fields.Id,
                    KTR_StudyQuestionManagedListSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot,
                    KTR_StudyQuestionManagedListSnapshot.Fields.KTR_QuestionnaireLinemanAgedList,
                    KTR_StudyQuestionManagedListSnapshot.Fields.KTR_Location,
                    KTR_StudyQuestionManagedListSnapshot.Fields.KTR_DisplayOrder,
                    KTR_StudyQuestionManagedListSnapshot.Fields.KTR_Name),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_StudyQuestionManagedListSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot, ConditionOperator.In, questionnaireLineSnapshotIds.Cast<object>().ToArray()),
                        new ConditionExpression(KTR_StudyQuestionManagedListSnapshot.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_StudyQuestionManagedListSnapshot_StatusCode.Active),
                    }
                },
                NoLock = true,
            };

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_StudyQuestionManagedListSnapshot>())
                .ToList();
        }
        #endregion

        #region Queries to Dataverse - StudyManagedListEntitySnapshots
        private List<KTR_StudyManagedListEntitiesSnapshot> GetStudyManagedListEntitySnapshots(
            IOrganizationService service,
            IList<Guid> managedListSnapshotIds)
        {
            if (managedListSnapshotIds == null || managedListSnapshotIds.Count == 0)
            {
                return new List<KTR_StudyManagedListEntitiesSnapshot>();
            }

            var query = new QueryExpression()
            {
                EntityName = KTR_StudyManagedListEntitiesSnapshot.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    KTR_StudyManagedListEntitiesSnapshot.Fields.Id,
                    KTR_StudyManagedListEntitiesSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot,
                    KTR_StudyManagedListEntitiesSnapshot.Fields.KTR_ManagedListEntity,
                    KTR_StudyManagedListEntitiesSnapshot.Fields.KTR_StudyQuestionManagedListSnapshot,
                    KTR_StudyManagedListEntitiesSnapshot.Fields.KTR_DisplayOrder,
                    KTR_StudyManagedListEntitiesSnapshot.Fields.KTR_Name),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            KTR_StudyManagedListEntitiesSnapshot.Fields.KTR_StudyQuestionManagedListSnapshot,
                            ConditionOperator.In,
                            managedListSnapshotIds.Cast<object>().ToArray()
                        ),
                        new ConditionExpression(
                            KTR_StudyManagedListEntitiesSnapshot.Fields.StatusCode,
                            ConditionOperator.Equal,
                            (int)KTR_StudyManagedListEntitiesSnapshot_StatusCode.Active
                        )
                    }
                },
                NoLock = true,
            };

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_StudyManagedListEntitiesSnapshot>())
                .ToList();
        }
        #endregion

        #region Queries to Dataverse - StudySnapshotLineChangeLog
        private void InsertStudySnapshotLineChangeLog(
            IOrganizationService service,
            IList<KTR_StudySnapshotLineChangelog> studyChangelogs)
        {
            if (studyChangelogs == null || studyChangelogs.Count == 0)
            {
                return;
            }

            var requestCollection = new OrganizationRequestCollection();

            foreach (var changelog in studyChangelogs)
            {
                requestCollection.Add(new CreateRequest { Target = changelog });
            }

            var executeMultiple = new ExecuteMultipleRequest
            {
                Requests = requestCollection,
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                    ReturnResponses = true
                }
            };

            var response = (ExecuteMultipleResponse)service.Execute(executeMultiple);

            if (response.IsFaulted)
            {
                throw new InvalidPluginExecutionException($"Error while inserting StudySnapshotLineChangeLog: {response.Responses.First().Fault.Message}");
            }
        }
        #endregion

        #region Check if Question was just added/removed
        private bool CheckIfQuestionWasJustAddedOrRemoved(
            IList<KTR_StudySnapshotLineChangelog> questionChangeLogs,
            KTR_StudySnapshotLineChangelog log,
            KTR_ChangelogType changeLogType = KTR_ChangelogType.QuestionAdded)
        {
            return questionChangeLogs.Any(
                            x => (x.KTR_RelatedObject == KTR_ChangelogRelatedObject.Question)
                            && (x.KTR_Change == changeLogType)
                            && ChangelogTypeBasedCompare(changeLogType, x, log));

        }

        private bool ChangelogTypeBasedCompare(KTR_ChangelogType changeLogType, KTR_StudySnapshotLineChangelog logQuestion,
            KTR_StudySnapshotLineChangelog logAnswer)
        {
            switch (changeLogType)
            {
                case KTR_ChangelogType.QuestionAdded:
                    return logQuestion.KTR_CurrentStudyQuestionnaireSnapshotLine?.Id == logAnswer.KTR_CurrentStudyQuestionnaireSnapshotLine?.Id;
                case KTR_ChangelogType.QuestionRemoved:
                    return logQuestion.KTR_FormerStudyQuestionnaireSnapshotLine?.Id == logAnswer.KTR_FormerStudyQuestionnaireSnapshotLine?.Id;
                default:
                    return false;
            }
        }

        #endregion
    }
}
