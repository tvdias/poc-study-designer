namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLineSnapshot
{
    using System;
    using System.Collections.Generic;
    using Kantar.StudyDesignerLite.Plugins;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLineSnapshot;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using System.Linq;

    public class QuestionnaireLineSnapshotRepository : IQuestionnaireLineSnapshotRepository
    {
        private readonly IOrganizationService _service;

        public QuestionnaireLineSnapshotRepository(IOrganizationService service)
        {
            _service = service;
        }

        public IEnumerable<KTR_StudyQuestionnaireLineSnapshot> GetStudyQuestionnaireLineSnapshots(Guid studyId)
        {
            var query = new QueryExpression
            {
                EntityName = KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName,
                ColumnSet = new ColumnSet(true)
            };

            query.Criteria.AddCondition(
                KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_Study,
                ConditionOperator.Equal,
                studyId
            );

            query.Criteria.AddCondition(
                KTR_StudyQuestionnaireLineSnapshot.Fields.StateCode,
                ConditionOperator.Equal,
                (int)KTR_StudyQuestionnaireLinesNaPsHot_StateCode.Active
            );

            query.Orders.Add(new OrderExpression(
                KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_SortOrder,
                OrderType.Ascending
            ));

            var results = _service.RetrieveMultiple(query);
            return results.Entities
            .Select(e => e.ToEntity<KTR_StudyQuestionnaireLineSnapshot>())
            .ToList();
        }
    }
}
