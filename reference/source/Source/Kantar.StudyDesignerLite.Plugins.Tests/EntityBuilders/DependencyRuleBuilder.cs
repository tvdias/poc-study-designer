using Microsoft.Xrm.Sdk;
using System;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class DependencyRuleBuilder
    {
        private readonly KTR_DependencyRule _entity;

        public DependencyRuleBuilder(
            KTR_ConfigurationQuestion configQuestion)
        {
            _entity = new KTR_DependencyRule
            {
                Id = Guid.NewGuid(),
                KTR_Name = configQuestion.Id.ToString(),
                KTR_ConfigurationQuestion = new EntityReference(configQuestion.LogicalName, configQuestion.Id),
                StateCode = KTR_DependencyRule_StateCode.Active,
                StatusCode = KTR_DependencyRule_StatusCode.Active,
            };
        }

        public DependencyRuleBuilder WithType(KTR_DependencyRule_KTR_Type type)
        {
            _entity.KTR_Type = type;
            return this;
        }

        public DependencyRuleBuilder WithContentType(KTR_DependencyRule_KTR_ContentType contentType)
        {
            _entity.KTR_ContentType = contentType;
            return this;
        }

        public DependencyRuleBuilder WithQuestionBank(KT_QuestionBank questionBank)
        {
            _entity.KTR_KT_QuestionBank = new EntityReference(questionBank.LogicalName, questionBank.Id);
            return this;
        }

        public DependencyRuleBuilder WithModule(KT_Module module)
        {
            _entity.KTR_KT_Module = new EntityReference(module.LogicalName, module.Id);
            return this;
        }

        public DependencyRuleBuilder WithTag(KTR_Tag tag)
        {
            _entity.KTR_Tag = new EntityReference(tag.LogicalName, tag.Id);
            return this;
        }

        public DependencyRuleBuilder WithTriggeringAnswerIfSingle(KTR_ConfigurationAnswer configAnswer)
        {
            _entity.KTR_TriggeringAnswer = new EntityReference(configAnswer.LogicalName, configAnswer.Id);
            return this;
        }

        public DependencyRuleBuilder WithClassification(KTR_DependencyRule_KTR_Classification classification)
        {
            _entity.KTR_Classification = classification;
            return this;
        }

        public KTR_DependencyRule Build()
        {
            return _entity;
        }
    }
}
