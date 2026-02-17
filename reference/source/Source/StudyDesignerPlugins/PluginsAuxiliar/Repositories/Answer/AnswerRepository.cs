namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Answer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Kantar.StudyDesignerLite.Plugins;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    [ExcludeFromCodeCoverage]
    public class AnswerRepository : IAnswerRepository
    {
        private readonly IOrganizationService _service;

        public AnswerRepository(IOrganizationService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public IDictionary<Guid, IList<KTR_QuestionnaireLinesAnswerList>> GetAnswersByQuestionnaireLines(IEnumerable<Guid> questionnaireLineIds)
        {
            var result = new Dictionary<Guid, IList<KTR_QuestionnaireLinesAnswerList>>();

            if (questionnaireLineIds?.Any() != true)
            {
                return result;
            }

            var query = new QueryExpression
            {
                EntityName = KTR_QuestionnaireLinesAnswerList.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine,
                            ConditionOperator.In,
                            questionnaireLineIds.ToArray()
                        ),
                        new ConditionExpression(
                            KTR_QuestionnaireLinesAnswerList.Fields.StateCode,
                            ConditionOperator.Equal,
                            (int)KTR_QuestionnaireLinesAnswerList_StateCode.Active
                        )
                    }
                },
                Orders =
                {
                    new OrderExpression(KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine, OrderType.Ascending),
                    new OrderExpression(KTR_QuestionnaireLinesAnswerList.Fields.KTR_DisplayOrder, OrderType.Ascending)
                }
            };

            var results = _service.RetrieveMultiple(query);

            foreach (var entity in results.Entities)
            {
                var answer = entity.ToEntity<KTR_QuestionnaireLinesAnswerList>();
                var questionnaireLineId = answer.KTR_QuestionnaireLine?.Id;

                if (questionnaireLineId.HasValue)
                {
                    if (!result.ContainsKey(questionnaireLineId.Value))
                    {
                        result[questionnaireLineId.Value] = new List<KTR_QuestionnaireLinesAnswerList>();
                    }
                    result[questionnaireLineId.Value].Add(answer);
                }
            }

            return result;
        }
    }
}
