
using System;
using System.IdentityModel.Metadata;
using Microsoft.Xrm.Sdk;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class QuestionnaireLineManagedListEntityBuilder
    {
        private readonly KTR_QuestionnaireLinemanAgedListEntity _entity;

        public QuestionnaireLineManagedListEntityBuilder(KTR_ManagedListEntity mLE)
        {
            _entity = new KTR_QuestionnaireLinemanAgedListEntity
            {
                Id = Guid.NewGuid(),
                KTR_ManagedListEntity = new EntityReference(KTR_ManagedListEntity.EntityLogicalName, mLE.Id),
            };
        }

        public QuestionnaireLineManagedListEntityBuilder WithId(Guid id)
        {
            _entity.Id = id;
            return this;
        }

        public QuestionnaireLineManagedListEntityBuilder WithQuestionnaireLine(KT_QuestionnaireLines ql)
        {
            _entity[KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_QuestionnaireLine] =
                new EntityReference(KT_QuestionnaireLines.EntityLogicalName, ql.Id);
            return this;
        }

        public QuestionnaireLineManagedListEntityBuilder WithStudy(KT_Study study)
        {
            _entity.KTR_StudyId = new EntityReference(study.LogicalName, study.Id);
            return this;
        }

        public QuestionnaireLineManagedListEntityBuilder WithStatusCode(KTR_QuestionnaireLinemanAgedListEntity_StatusCode statusCode)
        {
            _entity.StatusCode = statusCode;
            return this;
        }

        public QuestionnaireLineManagedListEntityBuilder WithStateCode(KTR_QuestionnaireLinemanAgedListEntity_StateCode stateCode)
        {
            _entity.StateCode = stateCode;
            return this;
        }

        public KTR_QuestionnaireLinemanAgedListEntity Build()
        {
            return _entity;
        }
    }
}
