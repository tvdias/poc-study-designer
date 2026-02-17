using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
   
    public class TagBuilder
    {
        private readonly KTR_Tag _entity;

        public TagBuilder()
        {
            _entity = new KTR_Tag
            {
                Id = Guid.NewGuid(),
            };
        }

        public TagBuilder WithName(string name)
        {
            _entity.KTR_Name = name;
            return this;
        }

        public KTR_Tag Build()
        {
            return _entity;
        }
    }
}
