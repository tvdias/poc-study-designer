using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class ProductConfigQuestionDisplayRuleBuilder
    {
        private readonly KTR_ProductConfigQuestionDisplayRule _entity;

        public ProductConfigQuestionDisplayRuleBuilder(KTR_ProductConfigQuestion productConfigQuestion, KTR_ConfigurationQuestion configurationQuestion, KTR_ConfigurationAnswer configQuestionAnswer, KTR_ConfigurationQuestion impactedQuestion)
        {
            _entity = new KTR_ProductConfigQuestionDisplayRule
            {
                Id = Guid.NewGuid(),
                KTR_ProductConfigQuestion = new EntityReference(productConfigQuestion.LogicalName, productConfigQuestion.Id),
                KTR_RuleConfigQuestion = new EntityReference(configurationQuestion.LogicalName, productConfigQuestion.KTR_ConfigurationQuestion.Id),
                KTR_RuleConfigAnswer = new EntityReference(configQuestionAnswer.LogicalName, configQuestionAnswer.Id),
                KTR_ImpactedConfigQuestion = new EntityReference(impactedQuestion.LogicalName, impactedQuestion.Id),
            };
        }

        public ProductConfigQuestionDisplayRuleBuilder DisplayRuleWithHideQuestionSetting()
        {
            _entity.KTR_Type = KTR_DisplayRuleType.ConfigurationQuestion;
            _entity[KTR_ProductConfigQuestionDisplayRule.Fields.KTR_DisplaySettings] = new OptionSetValue((int)KTR_DisplayRuleSetting.Hide);
            return this;
        }
        public ProductConfigQuestionDisplayRuleBuilder WithImpactedAnswer(KTR_ConfigurationAnswer impactedAnswer)
        {
            _entity.KTR_ImpactedConfigAnswer = new EntityReference(impactedAnswer.LogicalName, impactedAnswer.Id);
            return this;
        }

        public ProductConfigQuestionDisplayRuleBuilder DisplayRuleWithDisplayQuestionSetting()
        {
            _entity.KTR_Type = KTR_DisplayRuleType.ConfigurationQuestion;
            _entity[KTR_ProductConfigQuestionDisplayRule.Fields.KTR_DisplaySettings] = new OptionSetValue((int)KTR_DisplayRuleSetting.Display);
            return this;
        }

        public ProductConfigQuestionDisplayRuleBuilder DisplayRuleWithDisplayAnswerSetting()
        {
            _entity.KTR_Type = KTR_DisplayRuleType.ConfigurationAnswer;
            _entity[KTR_ProductConfigQuestionDisplayRule.Fields.KTR_DisplaySettings] = new OptionSetValue((int)KTR_DisplayRuleSetting.Hide);
            return this;
        }

        public KTR_ProductConfigQuestionDisplayRule Build()
        {
            return _entity;
        }
    }
}
