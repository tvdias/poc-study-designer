using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class ModuleBuilder
    {
        private readonly KT_Module _entity;

        public ModuleBuilder()
        {
            _entity = new KT_Module
            {
                Id = Guid.NewGuid(),
                StateCode = KT_Module_StateCode.Active,
                StatusCode = KT_Module_StatusCode.Active,
            };
        }

        public ModuleBuilder WithName(string name)
        {
            _entity.KT_Name = name;
            return this;
        }

        public KT_Module Build()
        {
            return _entity;
        }
    }
}
