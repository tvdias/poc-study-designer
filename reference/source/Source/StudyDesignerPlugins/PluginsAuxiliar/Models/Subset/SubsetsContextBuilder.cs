namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Subset
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Kantar.StudyDesignerLite.Plugins;
    using Microsoft.Xrm.Sdk;

    public class SubsetsContextBuilder
    {
        private readonly List<SubsetCreationContext> _contexts = new List<SubsetCreationContext>();

        private List<KTR_SubsetDefinition> _subsetDefinitions = new List<KTR_SubsetDefinition>();

        public SubsetsContextBuilder(
            List<KTR_QuestionnaireLinemanAgedListEntity> entityByQL,
            KT_Study study)
        {
            CreateContexts(entityByQL, study);
        }

        public List<SubsetCreationContext> Build()
        {
            var managedListGroups = _contexts.GroupBy(c => c.KTR_ManagedList.Id).ToList();

            var result = new List<SubsetCreationContext>();

            foreach (var context in managedListGroups)
            {
                var subsetDefinitionsInGroup = _subsetDefinitions
                    .Where(sd => sd.KTR_ManagedList.Id == context.First().KTR_ManagedList.Id)
                    .ToList();

                var countName = GetInitialCountName(subsetDefinitionsInGroup);

                foreach (var c1 in context)
                {
                    var c = new SubsetCreationContext()
                    {
                        SubsetDefinitionId = c1.SubsetDefinitionId,
                        Hash = c1.Hash,
                        SubsetName = c1.SubsetName,
                        KTR_ManagedList = c1.KTR_ManagedList,
                        MasterStudyId = c1.MasterStudyId,
                        EntityCount = c1.EntityCount,
                        IsFullList = c1.IsFullList,
                        IsReused = c1.IsReused,
                        IsNewSubset = c1.IsNewSubset,
                        Study = c1.Study,
                        StudySubsetDefinitionAssociation = c1.StudySubsetDefinitionAssociation,
                        IsNewStudySubsetDefinitionAssociation = c1.IsNewStudySubsetDefinitionAssociation,
                        NewSubsetEntities = c1.NewSubsetEntities,
                        ExistingSubsetEntities = c1.ExistingSubsetEntities,
                        NewQuestionnaireLineSubsets = c1.NewQuestionnaireLineSubsets,
                        ExistingQuestionnaireLineSubsets = c1.ExistingQuestionnaireLineSubsets,
                    };

                    c.SubsetName = !c.IsNewSubset ? c.SubsetName : $"{c.KTR_ManagedList.Name}SUB{countName++}";

                    c.StudySubsetDefinitionAssociation.KTR_Id = $"{c.Study.KT_Name}-{c.SubsetName}";

                    c.IsReused = c1.IsReused ? c1.IsReused : c.NewQuestionnaireLineSubsets.Count > 0;

                    result.Add(c);
                }
            }

            return result;
        }

        public List<SubsetCreationContext> ProcessExistingSubsetDefinitions(IList<KTR_SubsetDefinition> existent)
        {
            _subsetDefinitions = existent.ToList();
            foreach (var context in _contexts)
            {
                var existingSubset = existent
                    .FirstOrDefault(s => s.KTR_FilterSignature == context.Hash);

                if (existingSubset != null)
                {
                    context.SubsetDefinitionId = existingSubset.Id;
                    context.SubsetName = existingSubset.KTR_Name;
                    context.IsNewSubset = false;
                    context.IsReused = true;
                    continue;
                }
                context.IsNewSubset = true;
            }

            return _contexts;
        }

        public List<SubsetCreationContext> ProcessExistingStudySubsetDefinitions(IList<KTR_StudySubsetDefinition> existent)
        {
            foreach (var context in _contexts)
            {
                var association = existent.FirstOrDefault(sa => sa.KTR_SubsetDefinition.Id == context.SubsetDefinitionId);

                context.StudySubsetDefinitionAssociation = association is null ? new KTR_StudySubsetDefinition
                {
                    Id = Guid.NewGuid(),
                    KTR_Study = new EntityReference(KT_Study.EntityLogicalName, context.Study.Id),
                    KTR_SubsetDefinition = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, context.SubsetDefinitionId)
                } : association;

                context.IsNewStudySubsetDefinitionAssociation = association == null;
            }

            return _contexts;
        }

        public List<SubsetCreationContext> ProcessExistingQuestionnaireLineSubset(IList<KTR_QuestionnaireLineSubset> existent)
        {
            foreach (var subsetContext in _contexts)
            {
                foreach (var ql in subsetContext.KTR_QuestionnaireLines)
                {
                    var existingQLSubset = existent
                        ?.FirstOrDefault(q => q.KTR_Study.Id == subsetContext.Study.Id
                        && q.KTR_SubsetDefinitionId.Id == subsetContext.SubsetDefinitionId
                        && q.KTR_QuestionnaireLineId.Id == ql.Id);

                    if (existingQLSubset == null)
                    {
                        var newOne = new KTR_QuestionnaireLineSubset
                        {
                            Id = Guid.NewGuid(),
                            KTR_QuestionnaireLineId = ql,
                            KTR_ManagedListId = subsetContext.KTR_ManagedList,
                            KTR_Name = $"{ql.Name} - {subsetContext.SubsetDefinitionId}",
                            KTR_SubsetDefinitionId = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetContext.SubsetDefinitionId),
                            StatusCode = KTR_QuestionnaireLineSubset_StatusCode.Active,
                            KTR_StudyMaster = new EntityReference(KT_Study.EntityLogicalName, subsetContext.MasterStudyId),
                            KTR_Study = new EntityReference(KT_Study.EntityLogicalName, subsetContext.Study.Id)
                        };
                        subsetContext.NewQuestionnaireLineSubsets.Add(newOne);
                        continue;
                    }
                    subsetContext.ExistingQuestionnaireLineSubsets.Add(existingQLSubset);
                }
            }

            return _contexts;
        }

        public List<SubsetCreationContext> ProcessExistingSubsetEntities(IList<KTR_SubsetEntities> existent)
        {
            foreach (var context in _contexts)
            {
                foreach (var entityRef in context.KTR_ManagedListEntities)
                {
                    var existingEntities = existent
                        .FirstOrDefault(se => se.KTR_ManagedListEntity.Id == entityRef.Id
                                && se.KTR_SubsetDeFinTion.Id == context.SubsetDefinitionId);

                    if (existingEntities == null)
                    {
                        var subsetEntity = new KTR_SubsetEntities
                        {
                            Id = Guid.NewGuid(),
                            KTR_SubsetDeFinTion = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, context.SubsetDefinitionId),
                            KTR_ManagedListEntity = entityRef,
                            KTR_Name = $"{entityRef.Name}",
                            StatusCode = KTR_SubsetEntities_StatusCode.Active
                        };
                        context.NewSubsetEntities.Add(subsetEntity);
                        continue;
                    }
                    context.ExistingSubsetEntities.Add(existingEntities);
                }
            }

            return _contexts;
        }

        public List<SubsetCreationContext> ProcessManagedListEntities(IList<KTR_ManagedListEntity> smlEntities)
        {
            foreach (var context in _contexts.Where(x => x.KTR_ManagedList.Id == smlEntities.First().KTR_ManagedList.Id))
            {
                context.IsFullList = context.QuestionnaireLinemanAgedListEntities.Count == smlEntities.Count();
            }

            return _contexts;
        }

        private void CreateContexts(List<KTR_QuestionnaireLinemanAgedListEntity> entityByQL, KT_Study study)
        {
            var contextsGroups = entityByQL
                .GroupBy(e => e.KTR_ManagedList.Id)
                .Select(managedListGroup => managedListGroup
                    .GroupBy(x => x.KTR_QuestionnaireLine.Id)
                    .Select(qlGroup =>
                    {
                        return new SubsetCreationContext(qlGroup.ToList(), study);
                    }))
                .SelectMany(g => g).ToList();

            var sanitizedContexts = contextsGroups.GroupBy(c => c.Hash)
                .Select(g =>
                {
                    var entity = g.First();

                    entity.KTR_QuestionnaireLines = g
                        .SelectMany(c => c.QuestionnaireLinemanAgedListEntities
                            .Select(qlle => qlle.KTR_QuestionnaireLine))
                            .GroupBy(ql => ql.Id)
                            .Select(qlg => qlg.First())
                        .ToList();

                    return entity;
                })
                .ToList();

            _contexts.AddRange(sanitizedContexts);
        }

        private static int GetInitialCountName(IList<KTR_SubsetDefinition> subsets)
        {
            if (subsets == null || subsets.Count == 0)
            {
                return 1;
            }

            var subset = subsets.OrderBy(s => s.KTR_Name).Last();

            var regex = new Regex($@"{Regex.Escape("SUB")}(\d+)$");

            var match = regex.Match(subset.KTR_Name);

            if (!match.Success)
            {
                throw new Exception("No match found for subset name pattern.");
            }

            int.TryParse(match.Groups[1].Value, out var parsedCount);

            return parsedCount + 1;
        }
    }
}
