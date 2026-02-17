namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.SubsetDefinitionSnapshot
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Kantar.StudyDesignerLite.Plugins;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    [ExcludeFromCodeCoverage]
    public class SubsetDefinitionSnapshotRepository : ISubsetDefinitionSnapshotRepository
    {
        private readonly IOrganizationService _service;

        public SubsetDefinitionSnapshotRepository(IOrganizationService service)
        {
            _service = service;
        }

        public IList<KTR_StudySubsetDefinitionSnapshot> GetStudySubsetSnapshots(Guid studyId)
        {
            var query = new QueryExpression
            {
                EntityName = KTR_StudySubsetDefinitionSnapshot.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_StudySubsetDefinitionSnapshot.Fields.KTR_Study, ConditionOperator.Equal, studyId),
                        new ConditionExpression(KTR_StudySubsetDefinitionSnapshot.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_StudySubsetDefinitionSnapshot_StatusCode.Active)
                    }
                }
            };

            var results = _service.RetrieveMultiple(query);
            return results.Entities.Select(e => e.ToEntity<KTR_StudySubsetDefinitionSnapshot>()).ToList();
        }

        /// <summary>
        /// Returns snapshot rows containing Study, Subset Definition and Questionnaire Line for a study.
        /// </summary>
        /// <param name="studyId">The Study Id to filter by.</param>
        /// <param name="columns">Optional set of columns to return.</param>
        /// <returns>List of KTR_StudySubsetDefinitionSnapshot entities.</returns>
        public IList<KTR_StudySubsetDefinitionSnapshot> GetByStudyId(Guid studyId, string[] columns = null)
        {
            if (columns == null || columns.Length == 0)
            {
                columns = new string[]
                {
                    KTR_StudySubsetDefinitionSnapshot.Fields.Id,
                    KTR_StudySubsetDefinitionSnapshot.Fields.KTR_Study,
                    KTR_StudySubsetDefinitionSnapshot.Fields.KTR_SubsetDefinition2,
                    KTR_StudySubsetDefinitionSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot
                };
            }

            var query = new QueryExpression
            {
                EntityName = KTR_StudySubsetDefinitionSnapshot.EntityLogicalName,
                ColumnSet = new ColumnSet(columns),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_StudySubsetDefinitionSnapshot.Fields.KTR_Study, ConditionOperator.Equal, studyId),
                        new ConditionExpression(KTR_StudySubsetDefinitionSnapshot.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_StudySubsetDefinitionSnapshot_StatusCode.Active)
                    }
                }
            };

            var results = _service.RetrieveMultiple(query);

            return results == null
                ? new List<KTR_StudySubsetDefinitionSnapshot>()
                : results.Entities.Select(e => e.ToEntity<KTR_StudySubsetDefinitionSnapshot>()).ToList();
        }
    }
}
