using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class ProjectProductConfigQuestionAnswerBuilder
    {
        private readonly KTR_ProjectProductConfigQuestionAnswer _entity;

        public ProjectProductConfigQuestionAnswerBuilder(
            KTR_ProjectProductConfig projectProductConfigQuestion,
            KTR_ConfigurationQuestion configQuestion,
            KTR_ConfigurationAnswer configAnswer)
        {
            _entity = new KTR_ProjectProductConfigQuestionAnswer
            {
                Id = Guid.NewGuid(),
                StateCode = KTR_ProjectProductConfigQuestionAnswer_StateCode.Active,
                StatusCode = KTR_ProjectProductConfigQuestionAnswer_StatusCode.Active,
                KTR_ProjectProductConfigQuestion = new EntityReference(projectProductConfigQuestion.LogicalName, projectProductConfigQuestion.Id),
                KTR_ConfigurationQuestion = new EntityReference(configQuestion.LogicalName, configQuestion.Id),
                KTR_ConfigurationAnswer = new EntityReference(configAnswer.LogicalName, configAnswer.Id),
                KTR_IsSelected = true,
            };
        }

        public ProjectProductConfigQuestionAnswerBuilder WithIsSelected(bool isSelected)
        {
            _entity.KTR_IsSelected = isSelected;
            return this;
        }

        public ProjectProductConfigQuestionAnswerBuilder WithIsSelectedAsFalse()
        {
            _entity.KTR_IsSelected = false;
            return this;
        }

        public ProjectProductConfigQuestionAnswerBuilder WithIsSelectedAsTrue(KTR_ProjectProductConfig ProjectProductConfig)
        {
            _entity[KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_ProjectProductConfigQuestion] = new EntityReference(ProjectProductConfig.LogicalName, ProjectProductConfig.Id);
            _entity.KTR_IsSelected = true;
            return this;
        }

        public KTR_ProjectProductConfigQuestionAnswer Build()
        {
            return _entity;
        }
    }
}
