using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class ConfigurationQuestionBuilder
    {
        private readonly KTR_ConfigurationQuestion _entity;

        public ConfigurationQuestionBuilder()
        {
            _entity = new KTR_ConfigurationQuestion
            {
                Id = Guid.NewGuid(),
                KTR_Rule = KTR_Rule.SingleCoded,
                StateCode = KTR_ConfigurationQuestion_StateCode.Active,
                StatusCode = KTR_ConfigurationQuestion_StatusCode.Active,
            };
        }

        public ConfigurationQuestionBuilder WithName(string name)
        {
            _entity.KTR_Name = name;
            return this;
        }

        public ConfigurationQuestionBuilder WithRule(KTR_Rule rule)
        {
            _entity.KTR_Rule = rule;
            return this;
        }

        public KTR_ConfigurationQuestion Build()
        {
            return _entity;
        }
    }
}
