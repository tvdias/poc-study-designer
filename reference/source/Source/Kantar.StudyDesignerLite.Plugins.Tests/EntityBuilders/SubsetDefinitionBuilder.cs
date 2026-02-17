using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class SubsetDefinitionBuilder
    {
        private KTR_SubsetDefinition _entity;
        public SubsetDefinitionBuilder()
        {
            _entity = new KTR_SubsetDefinition()
            {
                Id = Guid.NewGuid(),
                StatusCode = KTR_SubsetDefinition_StatusCode.Active,
                StateCode = KTR_SubsetDefinition_StateCode.Active
            };
        }

        public SubsetDefinitionBuilder WithName(string name)
        {
            _entity[KTR_SubsetDefinition.Fields.KTR_Name] = name;
            return this;
        }

        public KTR_SubsetDefinition Build()
        {
            return _entity;
        }

    }
}
