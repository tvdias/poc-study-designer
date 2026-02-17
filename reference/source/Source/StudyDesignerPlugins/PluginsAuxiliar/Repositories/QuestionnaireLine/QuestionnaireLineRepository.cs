namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLine
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Kantar.StudyDesignerLite.Plugins;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    [ExcludeFromCodeCoverage]
    public class QuestionnaireLineRepository : IQuestionnaireLineRepository
    {
        private readonly IOrganizationService _service;

        public QuestionnaireLineRepository(IOrganizationService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public IList<KT_QuestionnaireLines> GetQuestionnaireLinesByProjectId(
           Guid projectId,
           string[] columns = null)
        {
            if (columns == null || columns.Length == 0)
            {
                columns = new string[] { KT_QuestionnaireLines.Fields.Id };
            }

            var query = new QueryExpression()
            {
                EntityName = KT_QuestionnaireLines.EntityLogicalName,
                ColumnSet = new ColumnSet(columns),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KT_QuestionnaireLines.Fields.KTR_Project, ConditionOperator.Equal, projectId),
                        new ConditionExpression(KT_QuestionnaireLines.Fields.StatusCode, ConditionOperator.Equal, (int)KT_QuestionnaireLines_StatusCode.Active)
                    }
                }
            };

            var results = _service.RetrieveMultiple(query);

            return results == null ?
                new List<KT_QuestionnaireLines>() :
                results.Entities
                    .Select(e => e.ToEntity<KT_QuestionnaireLines>())
                    .ToList();
        }

        public IEnumerable<KT_QuestionnaireLines> GetStudyQuestionnaireLines(Guid studyId)
        {
            var query = new QueryExpression
            {
                EntityName = KT_QuestionnaireLines.EntityLogicalName,
                ColumnSet = new ColumnSet(true)
            };

            var studyQLink = query.AddLink(
                KTR_StudyQuestionnaireLine.EntityLogicalName,
                KT_QuestionnaireLines.Fields.Id,
                KTR_StudyQuestionnaireLine.Fields.KTR_QuestionnaireLine
            );
            studyQLink.JoinOperator = JoinOperator.Inner;

            var studyLink = studyQLink.AddLink(
                KT_Study.EntityLogicalName,
                KTR_StudyQuestionnaireLine.Fields.KTR_Study,
                KT_Study.Fields.Id
            );
            studyLink.JoinOperator = JoinOperator.Inner;

            studyLink.LinkCriteria.AddCondition(
                KT_Study.Fields.Id,
                ConditionOperator.Equal,
                studyId
            );
            studyQLink.LinkCriteria.AddCondition(
                KTR_StudyQuestionnaireLine.Fields.StateCode,
                ConditionOperator.Equal,
                (int)KTR_StudyQuestionnaireLine_StateCode.Active
            );

            studyQLink.Orders.Add(new OrderExpression(
                KTR_StudyQuestionnaireLine.Fields.KTR_SortOrder,
                OrderType.Ascending
            ));

            var results = _service.RetrieveMultiple(query);
            return results.Entities.Select(e => e.ToEntity<KT_QuestionnaireLines>()).ToList();
        }
    }
}
