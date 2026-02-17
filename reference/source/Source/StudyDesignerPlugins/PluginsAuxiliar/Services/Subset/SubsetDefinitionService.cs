namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Services.Subset
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Kantar.StudyDesignerLite.Plugins;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Subset;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.ManagedLists;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLine;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Study;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Subset;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.SubsetDefinitionSnapshot;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.SubsetEntitiesSnapshot;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    public class SubsetDefinitionService
    {
        private readonly ITracingService _tracing;
        private readonly ISubsetRepository _repository;
        private readonly IQuestionnaireLineManagedListEntityRepository _qLMLErepository;
        private readonly IStudyRepository _studyRepository;
        private readonly IManagedListEntityRepository _managedListEntityRepository;

        public SubsetDefinitionService(
            ITracingService tracing,
            ISubsetRepository repository,
            IQuestionnaireLineManagedListEntityRepository qLMLErepository,
            IStudyRepository studyRepository,
            IManagedListEntityRepository managedListEntityRepository)
        {
            _tracing = tracing;
            _repository = repository;
            _qLMLErepository = qLMLErepository;
            _studyRepository = studyRepository;
            _managedListEntityRepository = managedListEntityRepository;
        }

        public List<SubsetCreationResponse> ProcessSubsetLogic(Guid studyId)
        {
            _tracing.Trace("Processing subset logic...");

            var study = GetStudy(studyId);

            var subsetDefinitions = GetSubsetDefinitions(study);

            var subsetAssociations = GetSubsetStudyAssociationByStudyId(study.Id);

            var qLMlEntities = GetQuestionnaireLineManagedListEntities(study.Id);

            if (qLMlEntities.Count == 0)
            {
                _tracing.Trace("No QuestionnaireLineManagedListEntities found. Exiting subset processing.");
                Delete(subsetAssociations, subsetDefinitions, study);
                return new List<SubsetCreationResponse>();
            }

            var qlSubsetsByStudy = GetQLSubsetsByStudyId(study.Id);

            return ProcessQuestionnaireLineManagedListEntities(
                qLMlEntities,
                subsetAssociations,
                subsetDefinitions,
                qlSubsetsByStudy,
                study);
        }

        // ========================= Snapshot summary (for Subset HTML) =========================
        // Use this to build the "Sublist: <name>  Question Count: <n>" view with entity list.
        public IDictionary<string, SubsetSnapshotSummary> GetSubsetSnapshotSummary(
            IOrganizationService service,
            Guid studyId,
            ITracingService tracingService)
        {
            var subsetDefRepo = new SubsetDefinitionSnapshotRepository(service);
            var subsetEntRepo = new SubsetEntitiesSnapshotRepository(service);

            var defSnaps = subsetDefRepo.GetByStudyId(studyId) ?? new List<KTR_StudySubsetDefinitionSnapshot>();
            var entSnaps = subsetEntRepo.GetByStudyId(studyId) ?? new List<KTR_StudySubsetEntitiesSnapshot>();

            tracingService.Trace($"Found {defSnaps.Count} Subset Definition Snapshots and {entSnaps.Count} Subset Entity Snapshots for Study {studyId}.");

            if (defSnaps.Count == 0 && entSnaps.Count == 0)
            {
                return new Dictionary<string, SubsetSnapshotSummary>(StringComparer.OrdinalIgnoreCase);
            }

            // Map SubsetDefinitionSnapshotId -> SubsetDefinitionId
            var subsetSnapshotToSubsetId = defSnaps
                .Where(s => s.KTR_SubsetDefinition2 != null)
                .ToDictionary(s => s.Id, s => s.KTR_SubsetDefinition2.Id);

            // Count distinct Questionnaire Line Snapshots per SubsetDefinitionId
            var qlCountBySubsetId = defSnaps
                .Where(s => s.KTR_SubsetDefinition2 != null && s.KTR_QuestionnaireLinesNaPsHot != null)
                .GroupBy(s => s.KTR_SubsetDefinition2.Id)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.KTR_QuestionnaireLinesNaPsHot.Id).Distinct().Count());

            // Aggregate entities per SubsetDefinitionId
            var entityTuplesBySubsetId = new Dictionary<Guid, List<Tuple<string, string, int?>>>();
            foreach (var ses in entSnaps)
            {
                var subsetDefSnapRef = ses.KTR_SubsetDefinitionSnapshot;
                if (subsetDefSnapRef == null)
                {
                    continue;
                }

                Guid subsetDefId;
                if (!subsetSnapshotToSubsetId.TryGetValue(subsetDefSnapRef.Id, out subsetDefId))
                {
                    continue;
                }

                var name = ses.KTR_AnswerText;
                var code = ses.KTR_AnswerCode;
                var order = ses.KTR_DisplayOrder;

                List<Tuple<string, string, int?>> list;
                if (!entityTuplesBySubsetId.TryGetValue(subsetDefId, out list))
                {
                    list = new List<Tuple<string, string, int?>>();
                    entityTuplesBySubsetId[subsetDefId] = list;
                    tracingService.Trace($"Created entity list for SubsetDefinitionId {subsetDefId}.");
                }

                // De-duplicate by Name+Code
                if (!list.Any(x =>
                    string.Equals(x.Item1, name, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(x.Item2, code, StringComparison.OrdinalIgnoreCase)))
                {
                    list.Add(Tuple.Create(name, code, (int?)order));
                }
            }

            // Resolve Subset Definition names
            var subsetIds = subsetSnapshotToSubsetId.Values.Distinct().ToList();
            var namesById = GetSubsetNames(service, subsetIds);

            // Build final dictionary keyed by Subset Name
            var result = new Dictionary<string, SubsetSnapshotSummary>(StringComparer.OrdinalIgnoreCase);

            foreach (var subsetId in subsetIds)
            {
                string subsetName;
                if (!namesById.TryGetValue(subsetId, out subsetName) || string.IsNullOrWhiteSpace(subsetName))
                {
                    subsetName = subsetId.ToString();
                }

                var entities = entityTuplesBySubsetId.ContainsKey(subsetId)
                    ? entityTuplesBySubsetId[subsetId]
                        .OrderBy(x => x.Item3 ?? int.MaxValue)
                        .ThenBy(x => x.Item1)
                        .Select(x => new SubsetSnapshotEntity { Name = x.Item1, Code = x.Item2 })
                        .ToList()
                    : new List<SubsetSnapshotEntity>();

                result[subsetName] = new SubsetSnapshotSummary
                {
                    SubsetName = subsetName,
                    QuestionCount = qlCountBySubsetId.ContainsKey(subsetId) ? qlCountBySubsetId[subsetId] : 0,
                    Entities = entities
                };
            }

            // Print final result
            tracingService.Trace($"SubsetSnapshotSummary built. Total subsets: {result.Count}.");
            foreach (var kvp in result)
            {
                var subsetName = kvp.Key;
                var summary = kvp.Value;
            }

            return result;
        }

        private List<SubsetCreationResponse> ProcessQuestionnaireLineManagedListEntities(
            IList<KTR_QuestionnaireLinemanAgedListEntity> qLMlEntities,
            IList<KTR_StudySubsetDefinition> subsetAssociations,
            IList<KTR_SubsetDefinition> subsetDefinitions,
            IList<KTR_QuestionnaireLineSubset> qlSubsetsByStudy,
            KT_Study study)
        {
            var subsetEntities = GetSubsetEntitiesByMLEntityIds(qLMlEntities);

            var contexts = new SubsetsContextBuilder(qLMlEntities.ToList(), study);

            contexts.ProcessExistingSubsetDefinitions(subsetDefinitions);
            contexts.ProcessExistingStudySubsetDefinitions(subsetAssociations);
            contexts.ProcessExistingQuestionnaireLineSubset(qlSubsetsByStudy);
            contexts.ProcessExistingSubsetEntities(subsetEntities);
            ProcessManagedListEntities(qLMlEntities, contexts);

            var result = contexts.Build();

            Delete(result, subsetAssociations, subsetDefinitions, study);

            InsertSubsetDefinitions(result);

            InsertSubsetAssociations(result);

            InsertQLSubsets(result);

            InsertSubsetEntities(result);

            return result.Select(s => s.ToResponse()).ToList();
        }

        private void Delete(
            IList<KTR_StudySubsetDefinition> existentSubsetAssociations,
            IList<KTR_SubsetDefinition> existentSubsets,
            KT_Study study)
        {
            var subsetCanDelete = existentSubsets
                .Where(sd => sd.KTR_EverInSnapshot != true)
                .ToList();

            var existentAssociations = GetSubsetAssociationBySubsetIds(
                subsetCanDelete.Select(x => x.Id).ToArray());

            var subsetIdsToDelete = existentAssociations
                .GroupBy(s => s.KTR_SubsetDefinition.Id)
                .Where(g => g.Count() == 1 && g.First().KTR_Study.Id == study.Id)
                .Select(g => g.First().KTR_SubsetDefinition.Id)
                .ToList();

            DeleteSubsetStudyAssociation(existentSubsetAssociations.ToList());

            DeleteSubsetEntities(subsetIdsToDelete);

            DeleteQLSubsetsByStudyId(study.Id);

            if (subsetIdsToDelete.Count == 0)
            {
                _tracing.Trace("No subsets to delete.");
                return;
            }

            _repository.BulkDelete(subsetIdsToDelete);
            _tracing.Trace($"Deleted subset IDs");
        }

        private List<KTR_SubsetDefinition> InsertSubsetDefinitions(IList<SubsetCreationContext> contexts)
        {
            _tracing.Trace("Inserting new subsets...");
            var subsetsToInsert = contexts
                .Where(c => c.IsNewSubset)
                .Select(g => g.ToEntity())
                .ToList();

            if (subsetsToInsert.Count == 0)
            {
                _tracing.Trace("No new subsets to insert.");
                return subsetsToInsert;
            }

            _repository.BulkInsert(subsetsToInsert);

            _tracing.Trace($"Inserted {subsetsToInsert.Count} new subsets.");

            return subsetsToInsert;
        }

        private List<KTR_StudySubsetDefinition> InsertSubsetAssociations(IList<SubsetCreationContext> contexts)
        {
            _tracing.Trace("Inserting new subsets Associations...");

            var association = contexts
                .Where(c => c.IsNewStudySubsetDefinitionAssociation)
                .Select(g => g.StudySubsetDefinitionAssociation)
                .ToList();

            if (association.Count == 0)
            {
                _tracing.Trace("No new subsets Associations to insert.");
                return association;
            }

            _repository.BulkInsertSubsetStudyAssociation(association);

            _tracing.Trace($"Inserted {association.Count} new subset Associations.");

            return association;
        }

        private void InsertSubsetEntities(IList<SubsetCreationContext> contexts)
        {
            _tracing.Trace("Inserting new subsets Entities...");

            var subsetEntitiesToInsert = contexts
                .SelectMany(c => c.NewSubsetEntities)
                .ToList();

            if (subsetEntitiesToInsert.Count == 0)
            {
                _tracing.Trace("No new subsets Entities to insert.");
                return;
            }

            _repository.BulkInsertSubsetEntities(subsetEntitiesToInsert);

            _tracing.Trace($"Inserted {subsetEntitiesToInsert.Count} new subset Entities.");
        }

        private void InsertQLSubsets(IList<SubsetCreationContext> contexts)
        {
            _tracing.Trace("Inserting new Questionnaire Line subsets...");

            var qlSubsetEntitiesToInsert = contexts
                .SelectMany(c => c.NewQuestionnaireLineSubsets)
                .ToList();

            if (qlSubsetEntitiesToInsert.Count == 0)
            {
                _tracing.Trace("No new Questionnaire Line subsets to insert.");
                return;
            }

            _repository.BulkInsertQLSubsets(qlSubsetEntitiesToInsert);

            _tracing.Trace($"Inserted {qlSubsetEntitiesToInsert.Count} new Questionnaire Line subsets.");
        }

        private void Delete(
            IList<SubsetCreationContext> contexts,
            IList<KTR_StudySubsetDefinition> existentSubsetAssociations,
            IList<KTR_SubsetDefinition> existentSubsets,
            KT_Study study)
        {
            _tracing.Trace("Deleting unused subsets...");

            var subsetCanDelete = existentSubsets
                .Where(sd => sd.KTR_EverInSnapshot != true)
                .ToList();

            var subsetDefinitionIdsCandidateToDelete = subsetCanDelete
                .Where(es => !contexts.Any(c => !c.IsNewSubset && c.SubsetDefinitionId == es.Id))
                .Select(es => es.Id)
                .ToList();

            var associationsToDelete = existentSubsetAssociations
                .Where(es => !contexts.Any(c => c.SubsetDefinitionId == es.KTR_SubsetDefinition.Id))
                .ToList();

            var existentAssociations = GetSubsetAssociationBySubsetIds(subsetDefinitionIdsCandidateToDelete.ToArray());

            var subsetIdsToDelete = existentAssociations
                .GroupBy(s => s.KTR_SubsetDefinition.Id)
                .Where(g => g.Count() == 1 && g.First().KTR_Study.Id == study.Id)
                .Select(g => g.First().KTR_SubsetDefinition.Id)
                .ToList();

            DeleteSubsetStudyAssociation(associationsToDelete);

            DeleteQLSubsetes(contexts);

            DeleteSubsetEntities(subsetIdsToDelete);

            if (existentAssociations.Count == 0)
            {
                _tracing.Trace("No subsets to delete.");
                return;
            }

            if (subsetIdsToDelete != null && subsetIdsToDelete.Count > 0)
            {
                _repository.BulkDelete(subsetIdsToDelete);
                _tracing.Trace($"Deleted subsets Ids.");
            }
        }

        private void DeleteSubsetStudyAssociation(List<KTR_StudySubsetDefinition> associationsToDelete)
        {
            if (associationsToDelete == null || associationsToDelete.Count == 0)
            {
                return;
            }

            var deleteIds = associationsToDelete
                .Select(a => a.Id).ToList();
            _repository.BulkDeleteSubsetStudyAssociation(deleteIds);
            _tracing.Trace($"Deleted {associationsToDelete.Count} subset associations in bulk.");
        }

        private void DeleteSubsetEntities(List<Guid> subsetDefinitionsIds)
        {
            if (subsetDefinitionsIds.Count == 0)
            {
                _tracing.Trace("No subsets Entities to delete.");
                return;
            }

            _tracing.Trace("Deleting unused subsets Entities...");

            var subsetEntitiesToDelete = _repository.GetSubsetEntitiesByDefinitionIds(subsetDefinitionsIds.ToArray());

            if (subsetEntitiesToDelete != null && subsetEntitiesToDelete.Count > 0)
            {
                var deleteIds = subsetEntitiesToDelete
                    .Select(se => se.Id).ToList();
                _repository.BulkDeleteSubsetEntity(deleteIds);
            }
        }

        private void DeleteQLSubsetes(
            IList<SubsetCreationContext> contexts)
        {
            _tracing.Trace("Deleting unused Questionnaire Line subsets...");

            var existingQLSubsets = _repository
                .GetQLSubsetsByStudyId(contexts.First().Study.Id);

            var qlSubsetEntitiesToDelete = existingQLSubsets.Where(es => !contexts
                .SelectMany(c => c.ExistingQuestionnaireLineSubsets)
                .Any(c => c.Id == es.Id)).ToList();

            if (qlSubsetEntitiesToDelete != null && qlSubsetEntitiesToDelete.Count > 0)
            {
                var deleteIds = qlSubsetEntitiesToDelete
                .Select(x => x.Id)?.ToList();
                _repository.BulkDeleteQLSubset(deleteIds);
                _tracing.Trace($"BulkDeleteQLSubset executed.");
            }
        }

        private void DeleteQLSubsetsByStudyId(Guid studyId)
        {
            _tracing.Trace("Deleting unused Questionnaire Line subsets...");

            var existingQLSubsets = _repository
                .GetQLSubsetsByStudyId(studyId);

            var deleteIds = existingQLSubsets
                .Select(s => s.Id).ToList();
            _repository.BulkDeleteQLSubset(deleteIds);
            _tracing.Trace($"BulkDeleteQLSubset executed.");
        }

        private IList<KTR_QuestionnaireLinemanAgedListEntity> GetQuestionnaireLineManagedListEntities(Guid studyId)
        {
            _tracing.Trace($"Retrieving QuestionnaireLinemanAgedListEntities for study {studyId}...");

            var columns = new string[]
            {
                KTR_QuestionnaireLinemanAgedListEntity.Fields.Id,
                KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_ManagedListEntity,
                KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_ManagedList,
                KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_QuestionnaireLine
            };
            var entities = _qLMLErepository.GetByStudyId(studyId, columns) ?? new List<KTR_QuestionnaireLinemanAgedListEntity>();

            if (entities.Count == 0)
            {
                _tracing.Trace($"No QuestionnaireLinemanAgedListEntities found for the study {studyId}.");
            }

            return entities;
        }

        /// <summary>
        /// Get all Subset Definitions for a master study if applicable
        /// </summary>
        /// <param name="study"></param>
        /// <returns>List of Subset Definitions</returns>
        private IList<KTR_SubsetDefinition> GetSubsetDefinitions(KT_Study study)
        {
            var masterStudyId = study.KTR_MasterStudy == null ? study.Id : study.KTR_MasterStudy.Id;

            _tracing.Trace($"Retrieving SubsetDefinitions for study {masterStudyId}...");

            var columns = new string[]
            {
                KTR_SubsetDefinition.Fields.Id,
                KTR_SubsetDefinition.Fields.KTR_MasterStudyId,
                KTR_SubsetDefinition.Fields.KTR_Name,
                KTR_SubsetDefinition.Fields.KTR_FilterSignature,
                KTR_SubsetDefinition.Fields.KTR_UsageCount,
                KTR_SubsetDefinition.Fields.KTR_ManagedList,
                KTR_SubsetDefinition.Fields.KTR_EverInSnapshot,
            };
            return _repository.GetByMasterStudyId(masterStudyId, columns)
                ?? new List<KTR_SubsetDefinition>();
        }

        private IList<KTR_StudySubsetDefinition> GetSubsetStudyAssociationByStudyId(Guid studyId)
        {
            _tracing.Trace($"Retrieving SubsetDefinitions Associations for study {studyId}...");

            return _repository.GetSubsetStudyAssociationByStudyId(studyId)
                ?? new List<KTR_StudySubsetDefinition>();
        }

        private IList<KTR_StudySubsetDefinition> GetSubsetAssociationBySubsetIds(Guid[] subsetIds)
        {
            _tracing.Trace($"Retrieving SubsetDefinitions Associations...");

            return _repository.GetSubsetAssociationBySubsetIds(subsetIds)
                ?? new List<KTR_StudySubsetDefinition>();
        }

        private SubsetsContextBuilder ProcessManagedListEntities(
            IList<KTR_QuestionnaireLinemanAgedListEntity> entities,
            SubsetsContextBuilder contextBuilder)
        {
            _tracing.Trace($"Retrieving ManagedListEntities...");

            var managedListIds = entities.GroupBy(e => e.KTR_ManagedList.Id).Select(g => g.Key).ToList();

            foreach (var id in managedListIds)
            {
                var mlEntites = GetManagedListEntities(id);
                contextBuilder.ProcessManagedListEntities(mlEntites);
            }

            return contextBuilder;
        }

        private List<KTR_ManagedListEntity> GetManagedListEntities(Guid managedListId)
        {
            _tracing.Trace($"Retrieving ManagedListEntities for Managed List {managedListId}...");

            return _managedListEntityRepository.GetByManagedListId(managedListId)
                ?? new List<KTR_ManagedListEntity>();
        }

        /// <summary>
        /// Get Study by Id and validate existence
        /// </summary>
        /// <param name="studyId"></param>
        /// <returns>A Study</returns>
        /// <exception cref="ArgumentException"> Throw if study is not found</exception>
        private KT_Study GetStudy(Guid studyId)
        {
            _tracing.Trace($"Retrieving Study Id {studyId}...");

            var columns = new string[]
            {
                KT_Study.Fields.Id,
                KT_Study.Fields.KTR_MasterStudy,
                KT_Study.Fields.KT_Name,
                KT_Study.Fields.KT_Project,
            };

            var study = _studyRepository.Get(studyId, columns);

            if (study == null)
            {
                _tracing.Trace($"Study with ID {studyId} not found.");
                throw new ArgumentException($"Study with ID {studyId} not found.");
            }
            _tracing.Trace($"Study found: {study.Id} - {study.KT_Name}");
            return study;
        }

        private IList<KTR_QuestionnaireLineSubset> GetQLSubsetsByStudyId(
            Guid studyId)
        {
            _tracing.Trace($"Retrieving Questionnaire Line Subsets for questionnaire for study {studyId}...");

            return _repository.GetQLSubsetsByStudyId(studyId)
                ?? new List<KTR_QuestionnaireLineSubset>();
        }

        private IList<KTR_SubsetEntities> GetSubsetEntitiesByMLEntityIds(
            IList<KTR_QuestionnaireLinemanAgedListEntity> entities)
        {
            _tracing.Trace($"Retrieving Subset Entities...");

            if (entities.Count == 0)
            {
                return new List<KTR_SubsetEntities>();
            }
            var mlEntityIds = entities.Select(e => e.KTR_ManagedListEntity.Id).Distinct();

            return _repository.GetSubsetEntitiesByMLEntityIds(mlEntityIds.ToArray())
                ?? new List<KTR_SubsetEntities>();
        }

        private static IDictionary<Guid, string> GetSubsetNames(IOrganizationService service, IEnumerable<Guid> subsetIds)
        {
            var ids = (subsetIds ?? Enumerable.Empty<Guid>()).Distinct().ToList();
            var dict = new Dictionary<Guid, string>();
            if (ids.Count == 0)
            {
                return dict;
            }

            var query = new QueryExpression
            {
                EntityName = KTR_SubsetDefinition.EntityLogicalName,
                ColumnSet = new ColumnSet(KTR_SubsetDefinition.Fields.KTR_Name),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_SubsetDefinition.Fields.Id, ConditionOperator.In, ids.Cast<object>().ToArray()),
                        new ConditionExpression(KTR_SubsetDefinition.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_SubsetDefinition_StatusCode.Active)
                    }
                },
                NoLock = true
            };

            var rows = service.RetrieveMultiple(query).Entities
                .Select(e => e.ToEntity<KTR_SubsetDefinition>())
                .ToList();

            foreach (var r in rows)
            {
                dict[r.Id] = r.KTR_Name;
            }

            return dict;
        }
    }
}
