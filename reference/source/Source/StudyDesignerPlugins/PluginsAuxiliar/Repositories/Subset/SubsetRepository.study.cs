namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Subset
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Kantar.StudyDesignerLite.Plugins;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Models;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Partial class for Subset Repository - Study XML operations
    /// </summary>
    public partial class SubsetRepository : ISubsetRepository
    {
        public IEnumerable<KTR_SubsetDefinition> GetStudySubsets(Guid studyId)
        {
            var query = new QueryExpression
            {
                EntityName = KTR_SubsetDefinition.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    KTR_SubsetDefinition.Fields.Id,
                    KTR_SubsetDefinition.Fields.KTR_Name,
                    KTR_SubsetDefinition.Fields.KTR_ManagedList,
                    KTR_SubsetDefinition.Fields.KTR_EntityCount
                )
            };

            var studySubsetLink = query.AddLink(
                KTR_StudySubsetDefinition.EntityLogicalName,
                KTR_SubsetDefinition.Fields.Id,
                KTR_StudySubsetDefinition.Fields.KTR_SubsetDefinition
            );
            studySubsetLink.JoinOperator = JoinOperator.Inner;
            studySubsetLink.LinkCriteria.AddCondition(
                KTR_StudySubsetDefinition.Fields.KTR_Study,
                ConditionOperator.Equal,
                studyId
            );

            query.Criteria.AddCondition(
                KTR_SubsetDefinition.Fields.StateCode,
                ConditionOperator.Equal,
                (int)KTR_SubsetDefinition_StateCode.Active
            );

            query.Orders.Add(new OrderExpression(KTR_SubsetDefinition.Fields.KTR_Name, OrderType.Ascending));

            var results = _service.RetrieveMultiple(query);
            return results.Entities.Select(e => e.ToEntity<KTR_SubsetDefinition>()).ToList();
        }

        public IDictionary<Guid, IList<KTR_SubsetEntities>> GetSubsetEntitiesBySubsetDefinitions(IEnumerable<Guid> subsetDefinitionIds)
        {
            var result = new Dictionary<Guid, IList<KTR_SubsetEntities>>();

            if (subsetDefinitionIds?.Any() != true)
            {
                return result;
            }

            var query = new QueryExpression
            {
                EntityName = KTR_SubsetEntities.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            KTR_SubsetEntities.Fields.KTR_SubsetDeFinTion,
                            ConditionOperator.In,
                            subsetDefinitionIds.ToArray()
                        ),
                        new ConditionExpression(
                            KTR_SubsetEntities.Fields.StateCode,
                            ConditionOperator.Equal,
                            (int)KTR_SubsetEntities_StateCode.Active
                        )
                    }
                }
            };

            var results = _service.RetrieveMultiple(query);

            foreach (var entity in results.Entities)
            {
                var subsetEntity = entity.ToEntity<KTR_SubsetEntities>();
                var subsetDefinitionId = subsetEntity.KTR_SubsetDeFinTion?.Id;

                if (subsetDefinitionId.HasValue)
                {
                    if (!result.ContainsKey(subsetDefinitionId.Value))
                    {
                        result[subsetDefinitionId.Value] = new List<KTR_SubsetEntities>();
                    }
                    result[subsetDefinitionId.Value].Add(subsetEntity);
                }
            }

            return result;
        }

        public IDictionary<Guid, IList<QuestionnaireLineSubsetWithLocation>> GetQuestionnaireLineSubsetsWithLocation(Guid studyId)
        {
            var result = new Dictionary<Guid, IList<QuestionnaireLineSubsetWithLocation>>();

            var query = new QueryExpression
            {
                EntityName = KTR_QuestionnaireLineSubset.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    KTR_QuestionnaireLineSubset.Fields.Id,
                    KTR_QuestionnaireLineSubset.Fields.KTR_QuestionnaireLineId,
                    KTR_QuestionnaireLineSubset.Fields.KTR_SubsetDefinitionId,
                    KTR_QuestionnaireLineSubset.Fields.KTR_ManagedListId
                ),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            KTR_QuestionnaireLineSubset.Fields.KTR_Study,
                            ConditionOperator.Equal,
                            studyId
                        ),
                        new ConditionExpression(
                            KTR_QuestionnaireLineSubset.Fields.StateCode,
                            ConditionOperator.Equal,
                            (int)KTR_QuestionnaireLineSubset_StateCode.Active
                        )
                    }
                }
            };

            var questionnaireLineSubsets = _service.RetrieveMultiple(query).Entities
                .Select(e => e.ToEntity<KTR_QuestionnaireLineSubset>())
                .ToList();

            if (questionnaireLineSubsets.Count == 0)
            {
                return result;
            }

            var subsetDefinitionIds = questionnaireLineSubsets
                .Select(qls => qls.KTR_SubsetDefinitionId?.Id)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .Distinct()
                .ToList();

            var subsetDefinitions = RetrieveByIds<KTR_SubsetDefinition>(
                subsetDefinitionIds,
                KTR_SubsetDefinition.EntityLogicalName,
                KTR_SubsetDefinition.Fields.Id,
                new ColumnSet(KTR_SubsetDefinition.Fields.Id, KTR_SubsetDefinition.Fields.KTR_Name));

            var managedListIds = questionnaireLineSubsets
                .Select(qls => qls.KTR_ManagedListId?.Id)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .Distinct()
                .ToList();

            var managedLists = RetrieveByIds<KTR_ManagedList>(
                managedListIds,
                KTR_ManagedList.EntityLogicalName,
                KTR_ManagedList.Fields.Id,
                new ColumnSet(KTR_ManagedList.Fields.Id, KTR_ManagedList.Fields.KTR_Name));

            var questionnaireLineIds = questionnaireLineSubsets
                .Select(qls => qls.KTR_QuestionnaireLineId?.Id)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .Distinct()
                .ToList();

            // Locations keyed by (QuestionnaireLineId, ManagedListId)
            var locations = new Dictionary<(Guid qlId, Guid mlId), string>();

            if (questionnaireLineIds.Any() && managedListIds.Any())
            {
                var managedListQuery = new QueryExpression
                {
                    EntityName = KTR_QuestionnaireLinesHaRedList.EntityLogicalName,
                    ColumnSet = new ColumnSet(
                        KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLine,
                        KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList,
                        KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location
                    ),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression(
                                KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLine,
                                ConditionOperator.In,
                                questionnaireLineIds.ToArray()),
                            new ConditionExpression(
                                KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList,
                                ConditionOperator.In,
                                managedListIds.ToArray())
                        }
                    }
                };

                var sharedListResults = _service.RetrieveMultiple(managedListQuery).Entities;

                foreach (var sharedList in sharedListResults)
                {
                    var qlId = sharedList
                        .GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLine)
                        ?.Id;
                    var mlId = sharedList
                        .GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList)
                        ?.Id;
                    var locationValue = sharedList
                        .GetAttributeValue<OptionSetValue>(KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location);

                    if (qlId.HasValue && mlId.HasValue)
                    {
                        var isColumn = locationValue != null &&
                                       (KTR_Location)locationValue.Value == KTR_Location.Column;

                        locations[(qlId.Value, mlId.Value)] = isColumn ? "Column" : "Row";
                    }
                }
            }

            foreach (var qls in questionnaireLineSubsets)
            {
                var qlId = qls.KTR_QuestionnaireLineId?.Id;
                var subsetDefId = qls.KTR_SubsetDefinitionId?.Id;
                var managedListId = qls.KTR_ManagedListId?.Id;

                if (!qlId.HasValue || !subsetDefId.HasValue)
                {
                    continue;
                }

                subsetDefinitions.TryGetValue(subsetDefId.Value, out var subsetDefinition);
                var subsetName = subsetDefinition?.KTR_Name;

                KTR_ManagedList managedList = null;
                if (managedListId.HasValue)
                {
                    managedLists.TryGetValue(managedListId.Value, out managedList);
                }

                var location = "Row";
                if (managedListId.HasValue &&
                    locations.TryGetValue((qlId.Value, managedListId.Value), out var loc))
                {
                    location = loc;
                }

                var subsetWithLocation = new QuestionnaireLineSubsetWithLocation
                {
                    SubsetDefinitionId = subsetDefId.Value,
                    SubsetName = subsetName,
                    Location = location,
                    QuestionnaireLineId = qlId.Value,
                    ManagedListId = managedListId,
                    ManagedListName = managedList?.KTR_Name
                };

                if (!result.TryGetValue(qlId.Value, out var list))
                {
                    list = new List<QuestionnaireLineSubsetWithLocation>();
                    result[qlId.Value] = list;
                }

                list.Add(subsetWithLocation);
            }

            return result;
        }

        public IDictionary<Guid, IList<SubsetEntityWithManagedListEntity>> GetSubsetEntitiesWithManagedListInfo(
            IEnumerable<Guid> subsetDefinitionIds)
        {
            var result = new Dictionary<Guid, IList<SubsetEntityWithManagedListEntity>>();

            var subsetDefinitionIdList = subsetDefinitionIds?.ToList();
            if (subsetDefinitionIdList == null || subsetDefinitionIdList.Count == 0)
            {
                return result;
            }

            var subsetEntities = GetSubsetEntitiesBySubsetDefinitions(subsetDefinitionIdList);
            if (subsetEntities.Count == 0)
            {
                return result;
            }

            var managedListEntityIds = subsetEntities.Values
                .SelectMany(entities => entities)
                .Select(se => se.KTR_ManagedListEntity?.Id)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .Distinct()
                .ToList();

            if (managedListEntityIds.Count == 0)
            {
                return result;
            }

            var managedListEntityLookup = RetrieveByIds<KTR_ManagedListEntity>(
                managedListEntityIds,
                KTR_ManagedListEntity.EntityLogicalName,
                KTR_ManagedListEntity.Fields.Id,
                new ColumnSet(
                    KTR_ManagedListEntity.Fields.Id,
                    KTR_ManagedListEntity.Fields.KTR_AnswerCode,
                    KTR_ManagedListEntity.Fields.KTR_AnswerText,
                    KTR_ManagedListEntity.Fields.KTR_DisplayOrder,
                    KTR_ManagedListEntity.Fields.CreatedOn
                ));

            foreach (var subsetGroup in subsetEntities)
            {
                var subsetDefinitionId = subsetGroup.Key;
                var entities = new List<SubsetEntityWithManagedListEntity>();

                foreach (var subsetEntity in subsetGroup.Value)
                {
                    var managedListEntityId = subsetEntity.KTR_ManagedListEntity?.Id;

                    if (managedListEntityId is Guid mleId &&
                        managedListEntityLookup.TryGetValue(mleId, out var managedListEntity))
                    {
                        entities.Add(new SubsetEntityWithManagedListEntity
                        {
                            SubsetEntityId = subsetEntity.Id,
                            SubsetDefinitionId = subsetDefinitionId,
                            ManagedListEntityId = managedListEntityId,
                            EntityCode = managedListEntity.KTR_AnswerCode,
                            EntityName = managedListEntity.KTR_AnswerText,
                            DisplayOrder = managedListEntity.KTR_DisplayOrder,
                            CreatedOn = managedListEntity.CreatedOn.GetValueOrDefault(DateTime.UtcNow) // Shouldn't be empty at all
                        });
                    }
                }

                if (entities.Count > 0)
                {
                    result[subsetDefinitionId] = entities;
                }
            }

            return result;
        }

        private Dictionary<Guid, T> RetrieveByIds<T>(
            IEnumerable<Guid> ids,
            string entityLogicalName,
            string idAttribute,
            ColumnSet columns)
            where T : Entity
        {
            var idArray = ids?.Distinct().ToArray() ?? Array.Empty<Guid>();
            if (idArray.Length == 0)
            {
                return new Dictionary<Guid, T>();
            }

            var query = new QueryExpression
            {
                EntityName = entityLogicalName,
                ColumnSet = columns,
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(idAttribute, ConditionOperator.In, idArray)
                    }
                }
            };

            return _service.RetrieveMultiple(query).Entities
                .Select(e => e.ToEntity<T>())
                .ToDictionary(e => e.Id, e => e);
        }
    }
}
