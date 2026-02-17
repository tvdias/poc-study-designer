using Microsoft.Xrm.Sdk;
using System;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class DependencyRuleAnswerBuilder
    {
        private readonly KTR_DependencyRuleAnswer _entity;

        public DependencyRuleAnswerBuilder(
            KTR_DependencyRule dependencyRule,
            KTR_ConfigurationAnswer configAnswer)
        {
            _entity = new KTR_DependencyRuleAnswer
            {
                Id = Guid.NewGuid(),
                KTR_DependencyRule = new EntityReference(dependencyRule.LogicalName, dependencyRule.Id),
                KTR_ConfigurationAnswer = new EntityReference(configAnswer.LogicalName, configAnswer.Id),
                StateCode = KTR_DependencyRuleAnswer_StateCode.Active,
                StatusCode = KTR_DependencyRuleAnswer_StatusCode.Active,
            };
        }

        public KTR_DependencyRuleAnswer Build()
        {
            return _entity;
        }
    }
}
