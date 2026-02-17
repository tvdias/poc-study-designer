using System;
using Microsoft.Xrm.Sdk;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class UrlBuilder
    {
        private readonly Entity _entity;

        public UrlBuilder(KT_Study study)
        {
            _entity = new Entity(KTR_Url.EntityLogicalName)
            {
                Id = Guid.NewGuid()
            };

            if (study != null)
            {
                _entity[KTR_Url.Fields.KTR_Study] = study.ToEntityReference();
            }
        }

        public UrlBuilder WithName(string name)
        {
            _entity[KTR_Url.Fields.KTR_Url] = name;
            return this;
        }

        public Entity Build()
        {
            return _entity;
        }
    }
}
