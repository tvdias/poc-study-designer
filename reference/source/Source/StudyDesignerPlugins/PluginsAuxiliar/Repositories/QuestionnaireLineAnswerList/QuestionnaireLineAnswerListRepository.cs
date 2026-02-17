namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLineAnswerList
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Kantar.StudyDesignerLite.Plugins;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    public class QuestionnaireLineAnswerListRepository
    {
        private readonly IOrganizationService _service;

        public QuestionnaireLineAnswerListRepository(IOrganizationService service)
        {
            _service = service;
        }

        public List<KTR_QuestionnaireLinesAnswerList> GetQuestionnaireLinesAnswerLists(IOrganizationService service, Guid questionnaireLineRefId)
        {
            var columns = new ColumnSet();
            columns.AllColumns = true;

            var answersQuery = new QueryExpression(KTR_QuestionnaireLinesAnswerList.EntityLogicalName)
            {
                ColumnSet = columns,
                Criteria =
                    {
                        Conditions =
                        {
                            new ConditionExpression(KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine, ConditionOperator.Equal, questionnaireLineRefId),
                            new ConditionExpression(KTR_QuestionnaireLinesAnswerList.Fields.StateCode, ConditionOperator.Equal, (int)KTR_QuestionnaireLinesAnswerList_StateCode.Active)
                        }
                    }
            };

            var answers = service.RetrieveMultiple(answersQuery).Entities.Select(e => e.ToEntity<KTR_QuestionnaireLinesAnswerList>()).ToList();
            return answers;
        }

        public List<KTR_QuestionnaireLinesAnswerList> GetCustomAnswerListsByQuestionnaireLine(Guid questionnaireLineId)
        {
            var query = new QueryExpression(KTR_QuestionnaireLinesAnswerList.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(
                    KTR_QuestionnaireLinesAnswerList.Fields.KTR_EnableCustomAnswerCodeEditing
                ),
                Criteria =
                    {
                        Conditions =
                        {
                            new ConditionExpression(
                                KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine,
                                ConditionOperator.Equal,
                                questionnaireLineId
                            ),

                            new ConditionExpression(
                                KTR_QuestionnaireLinesAnswerList.Fields.StatusCode,
                                ConditionOperator.Equal,
                                (int)KTR_QuestionnaireLinesAnswerList_StatusCode.Active
                            ),

                            new ConditionExpression(
                                KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionBank,
                                ConditionOperator.Null
                            )
                        }
                    }
            };

            return _service.RetrieveMultiple(query)
                           .Entities
                           .Select(e => e.ToEntity<KTR_QuestionnaireLinesAnswerList>())
                           .ToList();
        }
    }
}
