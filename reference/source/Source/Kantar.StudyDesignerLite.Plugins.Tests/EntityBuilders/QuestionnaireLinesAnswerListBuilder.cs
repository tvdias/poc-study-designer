using Microsoft.Xrm.Sdk;
using System;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class QuestionnaireLinesAnswerListBuilder
    {
        private readonly KTR_QuestionnaireLinesAnswerList _entity;

        public QuestionnaireLinesAnswerListBuilder(KT_QuestionnaireLines questionnaireLine)
        {
            _entity = new KTR_QuestionnaireLinesAnswerList
            {
                Id = Guid.NewGuid(),
                StateCode = KTR_QuestionnaireLinesAnswerList_StateCode.Active,
                StatusCode = KTR_QuestionnaireLinesAnswerList_StatusCode.Active,
                KTR_QuestionnaireLine = new EntityReference(questionnaireLine.LogicalName, questionnaireLine.Id),
            };
        }

        public QuestionnaireLinesAnswerListBuilder()
        {
            _entity = new KTR_QuestionnaireLinesAnswerList
            {
                Id = Guid.NewGuid(),
                StateCode = KTR_QuestionnaireLinesAnswerList_StateCode.Active,
                StatusCode = KTR_QuestionnaireLinesAnswerList_StatusCode.Active,
            };
        }

        public QuestionnaireLinesAnswerListBuilder WithAnswerId(string answerId)
        {
            _entity.KTR_AnswerId = answerId;
            return this;
        }

        public QuestionnaireLinesAnswerListBuilder WithCustomProperty(string customProperty)
        {
            _entity.KTR_CustomProperty = customProperty;
            return this;
        }

        public QuestionnaireLinesAnswerListBuilder WithEffectiveDate(DateTime effectiveDate)
        {
            _entity.KTR_EffectiveDate = effectiveDate;
            return this;
        }

        public QuestionnaireLinesAnswerListBuilder WithEndDate(DateTime endDate)
        {
            _entity.KTR_EndDate = endDate;
            return this;
        }

        public QuestionnaireLinesAnswerListBuilder WithIsActive(bool isActive)
        {
            _entity.KTR_IsActive = isActive;
            return this;
        }

        public QuestionnaireLinesAnswerListBuilder WithIsExclusive(bool isExclusive)
        {
            _entity.KTR_IsExclusive = isExclusive;
            return this;
        }

        public QuestionnaireLinesAnswerListBuilder WithIsFixed(bool isFixed)
        {
            _entity.KTR_IsFixed = isFixed;
            return this;
        }

        public QuestionnaireLinesAnswerListBuilder WithIsOpen(bool isOpen)
        {
            _entity.KTR_IsOpen = isOpen;
            return this;
        }

        public QuestionnaireLinesAnswerListBuilder WithIsTranslatable(bool isTranslatable)
        {
            _entity.KTR_IsTranslatable = isTranslatable;
            return this;
        }

        public QuestionnaireLinesAnswerListBuilder WithSourceId(string sourceId)
        {
            _entity.KTR_SourceId = sourceId;
            return this;
        }

        public QuestionnaireLinesAnswerListBuilder WithSourceName(string sourceName)
        {
            _entity.KTR_SourceName = sourceName;
            return this;
        }

        public QuestionnaireLinesAnswerListBuilder WithVersion(string version)
        {
            _entity.KTR_Version = version;
            return this;
        }

        public QuestionnaireLinesAnswerListBuilder WithAnswerText(string answerText)
        {
            _entity.KTR_AnswerText = answerText;
            return this;
        }
        public QuestionnaireLinesAnswerListBuilder WithAnswerCode(string answerCode)
        {
            _entity.KTR_AnswerCode = answerCode;
            return this;
        }

        public QuestionnaireLinesAnswerListBuilder WithAnswerName(string answerCode)
        {
            _entity.KTR_Name = answerCode;
            return this;
        }

        public QuestionnaireLinesAnswerListBuilder WithDisplayOrder(int displayOrder)
        {
            _entity.KTR_DisplayOrder = displayOrder;
            return this;
        }

        public QuestionnaireLinesAnswerListBuilder WithAnswerType(KTR_AnswerType answerType)
        {
            _entity.KTR_AnswerType = answerType;
            return this;
        }

        public QuestionnaireLinesAnswerListBuilder WithState(int state)
        {
            if (state == 0) // Active
            {
                _entity.StateCode = KTR_QuestionnaireLinesAnswerList_StateCode.Active;
                _entity.StatusCode = KTR_QuestionnaireLinesAnswerList_StatusCode.Active;
            }
            else if (state == 1) // Inactive
            {
                _entity.StateCode = KTR_QuestionnaireLinesAnswerList_StateCode.Inactive;
                _entity.StatusCode = KTR_QuestionnaireLinesAnswerList_StatusCode.Inactive;
            }
            return this;
        }

        public QuestionnaireLinesAnswerListBuilder WithCustomAnswerCodeEditToggle(bool CustomAnswerEditingToggle)
        {
            _entity.KTR_EnableCustomAnswerCodeEditing = CustomAnswerEditingToggle;
            return this;
        }

        public QuestionnaireLinesAnswerListBuilder WithQuestionBank(KT_QuestionBank questionBank)
        {
            _entity.KTR_QuestionBank = new EntityReference(questionBank.LogicalName, questionBank.Id);
            return this;
        }

        public QuestionnaireLinesAnswerListBuilder WithId(Guid id)
        {
            _entity.Id = id;
            return this;
        }

        public KTR_QuestionnaireLinesAnswerList Build()
        {
            return _entity;
        }
    }
}
