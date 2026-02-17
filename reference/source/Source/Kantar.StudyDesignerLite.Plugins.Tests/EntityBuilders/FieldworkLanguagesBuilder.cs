using Microsoft.Xrm.Sdk;
using System;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class FieldworkLanguagesBuilder
    {
        private readonly Entity _entity;

        public FieldworkLanguagesBuilder(Guid studyId)
        {
            _entity = new Entity("ktr_fieldworklanguages")
            {
                Id = Guid.NewGuid()
            };
            _entity["ktr_study"] = new EntityReference("kt_study", studyId);
        }

        public FieldworkLanguagesBuilder WithName(string name)
        {
            _entity["ktr_name"] = name;
            return this;
        }

        public FieldworkLanguagesBuilder WithCode(string code)
        {
            _entity["ktr_code"] = code;
            return this;
        }

        public Entity Build()
        {
            return _entity;
        }
    }
}
