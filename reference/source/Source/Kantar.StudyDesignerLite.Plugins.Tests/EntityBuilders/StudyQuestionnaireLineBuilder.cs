using Microsoft.Xrm.Sdk;
using System;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class StudyQuestionnaireLineBuilder
    {
        private readonly KTR_StudyQuestionnaireLine _entity;

        public StudyQuestionnaireLineBuilder(KT_Study study, KT_QuestionnaireLines questionnaireLine)
        {
            _entity = new KTR_StudyQuestionnaireLine
            {
                Id = Guid.NewGuid(),
                StateCode = KTR_StudyQuestionnaireLine_StateCode.Active,
                StatusCode = KTR_StudyQuestionnaireLine_StatusCode.Active,
                KTR_Study = new EntityReference(study.LogicalName, study.Id),
                KTR_QuestionnaireLine = new EntityReference(questionnaireLine.LogicalName, questionnaireLine.Id),
            };
        }

        public StudyQuestionnaireLineBuilder WithState(int state)
        {
            if (state == 0) // Active
            {
                _entity.StateCode = KTR_StudyQuestionnaireLine_StateCode.Active;
                _entity.StatusCode = KTR_StudyQuestionnaireLine_StatusCode.Active;
            }
            else if (state == 1) // Inactive
            {
                _entity.StateCode = KTR_StudyQuestionnaireLine_StateCode.Inactive;
                _entity.StatusCode = KTR_StudyQuestionnaireLine_StatusCode.Inactive;
            }
            return this;
        }

        public StudyQuestionnaireLineBuilder WithSortOrder(int sortOrder)
        {
            _entity.KTR_SortOrder = sortOrder;
            return this;
        }

        public KTR_StudyQuestionnaireLine Build()
        {
            return _entity;
        }
    }
}
