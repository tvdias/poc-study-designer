
using System;
using System.IdentityModel.Metadata;
using Microsoft.Xrm.Sdk;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class StudyManagedListEntityBuilder
    {
        private readonly KTR_StudyManagedListEntity _entity;

        public StudyManagedListEntityBuilder(KTR_ManagedListEntity mLE)
        {
            _entity = new KTR_StudyManagedListEntity
            {
                Id = Guid.NewGuid(),
                KTR_ManagedListEntity = new EntityReference(KTR_ManagedListEntity.EntityLogicalName, mLE.Id),
            };
        }

        public StudyManagedListEntityBuilder WithStudy(KT_Study study)
        {
            _entity.KTR_Study = new EntityReference(study.LogicalName, study.Id);
            return this;
        }

        public StudyManagedListEntityBuilder WithStatusCode(KTR_StudyManagedListEntity_StatusCode statusCode)
        {
            _entity.StatusCode = statusCode;
            return this;
        }

        public StudyManagedListEntityBuilder WithStateCode(KTR_StudyManagedListEntity_StateCode stateCode)
        {
            _entity.StateCode = stateCode;
            return this;
        }

        public KTR_StudyManagedListEntity Build()
        {
            return _entity;
        }
    }
}
