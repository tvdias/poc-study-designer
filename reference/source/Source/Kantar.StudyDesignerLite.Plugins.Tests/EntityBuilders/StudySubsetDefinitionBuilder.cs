using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class StudySubsetDefinitionBuilder
    {
        private KTR_StudySubsetDefinition _entity;
        public StudySubsetDefinitionBuilder()
        {
            _entity = new KTR_StudySubsetDefinition()
            {
                Id = Guid.NewGuid(),
                StatusCode = KTR_StudySubsetDefinition_StatusCode.Active,
                StateCode = KTR_StudySubsetDefinition_StateCode.Active
            };
        }

        public StudySubsetDefinitionBuilder WithStudyName(string name)
        {
            _entity[KTR_StudySubsetDefinition.Fields.KTR_StudyName] = name;
            return this;
        }

        public StudySubsetDefinitionBuilder WithStudy(KT_Study study)
        {
            _entity.KTR_Study = new EntityReference(study.LogicalName, study.Id);
            return this;
        }

        public StudySubsetDefinitionBuilder WithSubsetDefinition(KTR_SubsetDefinition subsetDefinition)
        {
            _entity.KTR_SubsetDefinition = new EntityReference(subsetDefinition.LogicalName, subsetDefinition.Id);
            return this;
        }

        public KTR_StudySubsetDefinition Build()
        {
            return _entity;
        }

    }
}
