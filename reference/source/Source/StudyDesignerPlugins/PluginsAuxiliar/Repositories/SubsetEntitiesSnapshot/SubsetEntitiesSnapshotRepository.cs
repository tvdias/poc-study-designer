namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.SubsetEntitiesSnapshot
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Kantar.StudyDesignerLite.Plugins;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Models;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    [ExcludeFromCodeCoverage]
    public class SubsetEntitiesSnapshotRepository : ISubsetEntitiesSnapshotRepository
    {
        private readonly IOrganizationService _service;

        public SubsetEntitiesSnapshotRepository(IOrganizationService service)
        {
            _service = service;
        }

        public IDictionary<Guid, IList<KTR_StudySubsetEntitiesSnapshot>> GetSubsetEntitiesBySubsetDefinitions(IList<Guid> subsetDefinitionSnapshotIds)
        {
            var result = new Dictionary<Guid, IList<KTR_StudySubsetEntitiesSnapshot>>();

            if (subsetDefinitionSnapshotIds?.Any() != true)
            {
                return result;
            }

            var query = new QueryExpression
            {
                EntityName = KTR_StudySubsetEntitiesSnapshot.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            KTR_StudySubsetEntitiesSnapshot.Fields.KTR_SubsetDefinitionSnapshot,
                            ConditionOperator.In,
                            subsetDefinitionSnapshotIds.ToArray()
                        ),
                        new ConditionExpression(
                            KTR_StudySubsetEntitiesSnapshot.Fields.StateCode,
                            ConditionOperator.Equal,
                            (int)KTR_StudySubsetEntitiesSnapshot_StateCode.Active
                        )
                    }
                }
            };

            var results = _service.RetrieveMultiple(query);

            foreach (var entity in results.Entities)
            {
                var subsetEntitySnapshot = entity.ToEntity<KTR_StudySubsetEntitiesSnapshot>();
                var subsetDefinitionSnapshotId = subsetEntitySnapshot.KTR_SubsetDefinitionSnapshot?.Id;

                if (subsetDefinitionSnapshotId.HasValue)
                {
                    if (!result.ContainsKey(subsetDefinitionSnapshotId.Value))
                    {
                        result[subsetDefinitionSnapshotId.Value] = new List<KTR_StudySubsetEntitiesSnapshot>();
                    }
                    result[subsetDefinitionSnapshotId.Value].Add(subsetEntitySnapshot);
                }
            }

            return result;
        }

        public IList<KTR_StudySubsetEntitiesSnapshot> GetSubsetEntitiesSnapshots(IList<Guid> subsetDefinitionSnapshotIds)
        {
            var result = new List<KTR_StudySubsetEntitiesSnapshot>();

            if (subsetDefinitionSnapshotIds == null || subsetDefinitionSnapshotIds.Count == 0)
            {
                return result;
            }

            var query = new QueryExpression
            {
                EntityName = KTR_StudySubsetEntitiesSnapshot.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            KTR_StudySubsetEntitiesSnapshot.Fields.KTR_SubsetDefinitionSnapshot,
                            ConditionOperator.In,
                            subsetDefinitionSnapshotIds.ToArray()
                        ),
                        new ConditionExpression(
                            KTR_StudySubsetEntitiesSnapshot.Fields.StateCode,
                            ConditionOperator.Equal,
                            (int)KTR_StudySubsetEntitiesSnapshot_StateCode.Active
                        )
                    }
                }
            };

            var results = _service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_StudySubsetEntitiesSnapshot>())
                .ToList();
        }

        /// <summary>
        /// Returns subset-entities snapshot rows for a study, including links to the subset definition snapshot and questionnaire line snapshot.
        /// </summary>
        /// <param name="studyId">The Study Id to filter by.</param>
        /// <param name="columns">Optional set of columns to return.</param>
        /// <returns>List of KTR_StudySubsetEntitiesSnapshot entities.</returns>
        public IList<KTR_StudySubsetEntitiesSnapshot> GetByStudyId(Guid studyId, string[] columns = null)
        {
            if (columns == null || columns.Length == 0)
            {
                columns = new string[]
                {
                    KTR_StudySubsetEntitiesSnapshot.Fields.Id,
                    KTR_StudySubsetEntitiesSnapshot.Fields.KTR_Study,
                    KTR_StudySubsetEntitiesSnapshot.Fields.KTR_SubsetDefinitionSnapshot,
                    KTR_StudySubsetEntitiesSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot,
                    KTR_StudySubsetEntitiesSnapshot.Fields.KTR_SubsetEntities,
                    KTR_StudySubsetEntitiesSnapshot.Fields.KTR_AnswerText,
                    KTR_StudySubsetEntitiesSnapshot.Fields.KTR_AnswerCode,
                    KTR_StudySubsetEntitiesSnapshot.Fields.KTR_DisplayOrder
                };
            }

            var query = new QueryExpression
            {
                EntityName = KTR_StudySubsetEntitiesSnapshot.EntityLogicalName,
                ColumnSet = new ColumnSet(columns),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_StudySubsetEntitiesSnapshot.Fields.KTR_Study, ConditionOperator.Equal, studyId),
                        new ConditionExpression(KTR_StudySubsetEntitiesSnapshot.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_StudySubsetEntitiesSnapshot_StatusCode.Active)
                    }
                }
            };

            // Order by Display Order ascending
            query.AddOrder(KTR_StudySubsetEntitiesSnapshot.Fields.KTR_DisplayOrder, OrderType.Ascending);

            var results = _service.RetrieveMultiple(query);

            return results == null
                ? new List<KTR_StudySubsetEntitiesSnapshot>()
                : results.Entities.Select(e => e.ToEntity<KTR_StudySubsetEntitiesSnapshot>()).ToList();
        }
    }
}
