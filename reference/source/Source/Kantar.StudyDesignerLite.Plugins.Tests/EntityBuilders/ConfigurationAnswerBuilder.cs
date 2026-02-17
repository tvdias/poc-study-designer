using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class ConfigurationAnswerBuilder
    {
        private readonly KTR_ConfigurationAnswer _entity;

        public ConfigurationAnswerBuilder(KTR_ConfigurationQuestion configQuestion)
        {
            _entity = new KTR_ConfigurationAnswer
            {
                Id = Guid.NewGuid(),
                KTR_ConfigurationQuestion = new EntityReference(configQuestion.LogicalName, configQuestion.Id),
                StateCode = KTR_ConfigurationAnswer_StateCode.Active,
                StatusCode = KTR_ConfigurationAnswer_StatusCode.Active,
            };
        }

        public ConfigurationAnswerBuilder WithName(string name)
        {
            _entity.KTR_Name = name;
            return this;
        }

        public KTR_ConfigurationAnswer Build()
        {
            return _entity;
        }
    }
}
