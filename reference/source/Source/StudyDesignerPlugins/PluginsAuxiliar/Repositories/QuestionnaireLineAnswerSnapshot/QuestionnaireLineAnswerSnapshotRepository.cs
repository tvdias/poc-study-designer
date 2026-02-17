using System;
using System.Collections.Generic;
using System.Linq;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLineAnswerSnapshot
{
    public class QuestionnaireLineAnswerSnapshotRepository : IQuestionnaireLineAnswerSnapshotRepository
    {
        private readonly IOrganizationService _service;

        public QuestionnaireLineAnswerSnapshotRepository(IOrganizationService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public IDictionary<Guid, IList<KTR_StudyQuestionAnswerListSnapshot>> GetAnswersBySnapshotIds(IEnumerable<Guid> snapshotIds)
        {
            if (snapshotIds == null || !snapshotIds.Any())
            { return new Dictionary<Guid, IList<KTR_StudyQuestionAnswerListSnapshot>>(); }

            var query = new QueryExpression(KTR_StudyQuestionAnswerListSnapshot.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(true)
            };

            query.Criteria.AddCondition(
                KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot,
                ConditionOperator.In,
                snapshotIds.Cast<object>().ToArray()
            );

            var results = _service.RetrieveMultiple(query);

            var typedResults = results.Entities
                .Select(e => e.ToEntity<KTR_StudyQuestionAnswerListSnapshot>())
                .Where(a => a.KTR_QuestionnaireLinesNaPsHot != null)
                .ToList();

            // Group by snapshot ID
            var grouped = typedResults
                .GroupBy(a => a.KTR_QuestionnaireLinesNaPsHot.Id)
                .ToDictionary(
                    g => g.Key,
                    g => (IList<KTR_StudyQuestionAnswerListSnapshot>)g.ToList()
                );

            // Ensure all requested snapshot IDs exist in the dictionary, even if empty (to avoid errors)
            foreach (var id in snapshotIds)
            {
                if (!grouped.ContainsKey(id))
                {
                    grouped[id] = new List<KTR_StudyQuestionAnswerListSnapshot>();
                }
            }

            return grouped;
        }
    }
}
