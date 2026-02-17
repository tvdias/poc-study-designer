using Microsoft.Xrm.Sdk;
using System;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class StudyBuilder
    {
        private readonly KT_Study _entity;

        public StudyBuilder(KT_Project project)
        {
            _entity = new KT_Study
            {
                Id = Guid.NewGuid(),
                StateCode = KT_Study_StateCode.Active,
                StatusCode = KT_Study_StatusCode.Draft,
                KT_Project = new EntityReference(project.LogicalName, project.Id),
            };
        }

        public StudyBuilder WithName(string name)
        {
            _entity.KT_Name = name;
            return this;
        }

        public StudyBuilder WithStatusCode(KT_Study_StatusCode statusCode)
        {
            _entity.StatusCode = statusCode;
            return this;
        }

        public StudyBuilder WithStateCode(KT_Study_StateCode stateCode)
        {
            _entity.StateCode = stateCode;
            return this;
        }

        public StudyBuilder WithIsSnapshotCreated(bool isSnapshotCreated)
        {
            _entity.KTR_IsSnapshotCreated = isSnapshotCreated;
            return this;
        }

        public StudyBuilder WithParentStudy(KT_Study study)
        {
            _entity.KTR_ParentStudy = new EntityReference(study.LogicalName, study.Id);
            return this;
        }
        public StudyBuilder WithMasterStudy(KT_Study study)
        {
            _entity.KTR_MasterStudy = new EntityReference(study.LogicalName, study.Id);
            return this;
        }
        public StudyBuilder WithVersion(int version)
        {
            _entity.KTR_VersionNumber = version;
            return this;
        }

        public StudyBuilder WithFieldworkMarket(Guid marketId)
        {
            _entity.KTR_StudyFieldworkMarket = new EntityReference("ktr_studyfieldworkmarket", marketId);
            return this;
        }

        public KT_Study Build()
        {
            return _entity;
        }
    }
}
