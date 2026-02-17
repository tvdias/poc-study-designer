using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class StudyQuestionnaireLineSnapshotBuilder
    {
        private readonly KTR_StudyQuestionnaireLineSnapshot _entity;

        public StudyQuestionnaireLineSnapshotBuilder(KT_Study study, KT_QuestionnaireLines questionnaireLine)
        {
            _entity = new KTR_StudyQuestionnaireLineSnapshot
            {
                Id = Guid.NewGuid(),
                StateCode = KTR_StudyQuestionnaireLinesNaPsHot_StateCode.Active,
                StatusCode = KTR_StudyQuestionnaireLinesNaPsHot_StatusCode.Active,
                KTR_Study = new EntityReference(study.LogicalName, study.Id),
                KTR_QuestionnaireLine = new EntityReference(questionnaireLine.LogicalName, questionnaireLine.Id),
            };
        }
        public StudyQuestionnaireLineSnapshotBuilder WithId(Guid id)
        {
            _entity.Id = id;
            return this;
        }

        public StudyQuestionnaireLineSnapshotBuilder WithQuestionnaireLine(Guid questionnaireLineId)
        {
            _entity.KTR_QuestionnaireLine = new EntityReference("kt_questionnairelines", questionnaireLineId);
            return this;
        }

        public StudyQuestionnaireLineSnapshotBuilder WithStudy(Guid studyId)
        {
            _entity.KTR_Study = new EntityReference("kt_study", studyId);
            return this;
        }

        public StudyQuestionnaireLineSnapshotBuilder WithStatusCode(KTR_StudyQuestionnaireLinesNaPsHot_StatusCode statusCode)
        {
            _entity.StatusCode = statusCode;
            return this;
        }

        public StudyQuestionnaireLineSnapshotBuilder WithScripterNotes(string scripterNotes)
        {
            _entity.KTR_ScripterNotes = scripterNotes;
            return this;
        }

        public StudyQuestionnaireLineSnapshotBuilder WithFieldValue(string fieldName, object value)
        {
            _entity.Attributes[fieldName] = value;
            return this;
        }
        public StudyQuestionnaireLineSnapshotBuilder WithModule(KT_Module module)
        {
            _entity.KTR_Module2 = new EntityReference(module.LogicalName, module.Id);
            return this;
        }

        public StudyQuestionnaireLineSnapshotBuilder WithSortOrder(int sortOrder)
        {
            _entity.KTR_SortOrder = sortOrder;
            return this;
        }

        public KTR_StudyQuestionnaireLineSnapshot Build()
        {
            return _entity;
        }
    }
}
