using Microsoft.Xrm.Sdk;
using System;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class StudyQuestionnaireLineAnswerSnapshotBuilder
    {
        private readonly KTR_StudyQuestionAnswerListSnapshot _entity;

        public StudyQuestionnaireLineAnswerSnapshotBuilder(KTR_StudyQuestionnaireLineSnapshot studyQlSnapshot, KTR_QuestionnaireLinesAnswerList qlAnswer)
        {
            _entity = new KTR_StudyQuestionAnswerListSnapshot
            {
                Id = Guid.NewGuid(),
                StateCode = KTR_StudyQuestionAnswerListSnapshot_StateCode.Active,
                StatusCode = KTR_StudyQuestionAnswerListSnapshot_StatusCode.Active,
                KTR_QuestionnaireLinesNaPsHot = new EntityReference(studyQlSnapshot.LogicalName, studyQlSnapshot.Id),
                KTR_QuestionnaireLinesAnswerList = new EntityReference(qlAnswer.LogicalName, qlAnswer.Id),
            };
        }

        public StudyQuestionnaireLineAnswerSnapshotBuilder WithAnswerId(string answerId)
        {
            _entity.KTR_AnswerId = answerId;
            return this;
        }

        public StudyQuestionnaireLineAnswerSnapshotBuilder WithAnswerLocation(string answerLocation)
        {
            _entity.KTR_AnswerLocation = answerLocation;
            return this;
        }

        public StudyQuestionnaireLineAnswerSnapshotBuilder WithAnswerText(string answerText)
        {
            _entity.KTR_AnswerText = answerText;
            return this;
        }

        public StudyQuestionnaireLineAnswerSnapshotBuilder WithCustomerProperty(string customerProperty)
        {
            _entity.KTR_CustomProperty = customerProperty;
            return this;
        }

        public StudyQuestionnaireLineAnswerSnapshotBuilder WithEffectiveDate(DateTime effectiveDate)
        {
            _entity.KTR_EffectiveDate = effectiveDate;
            return this;
        }

        public StudyQuestionnaireLineAnswerSnapshotBuilder WithEndDate(DateTime endDate)
        {
            _entity.KTR_EndDate = endDate;
            return this;
        }

        public StudyQuestionnaireLineAnswerSnapshotBuilder WithIsActive(string isActive)
        {
            _entity.KTR_IsActive = isActive;
            return this;
        }

        public StudyQuestionnaireLineAnswerSnapshotBuilder WithIsExclusive(string isExclusive)
        {
            _entity.KTR_IsExclusive = isExclusive;
            return this;
        }

        public StudyQuestionnaireLineAnswerSnapshotBuilder WithIsFixed(string isFixed)
        {
            _entity.KTR_IsFixed = isFixed;
            return this;
        }

        public StudyQuestionnaireLineAnswerSnapshotBuilder WithIsOpen(string isOpen)
        {
            _entity.KTR_IsOpen = isOpen;
            return this;
        }

        public StudyQuestionnaireLineAnswerSnapshotBuilder WithIsTranslatable(string isTranslatable)
        {
            _entity.KTR_IsTranslatable = isTranslatable;
            return this;
        }

        public StudyQuestionnaireLineAnswerSnapshotBuilder WithSourceId(string sourceId)
        {
            _entity.KTR_SourceId = sourceId;
            return this;
        }
        public StudyQuestionnaireLineAnswerSnapshotBuilder WithSourceName(string sourceName)
        {
            _entity.KTR_SourceName = sourceName;
            return this;
        }
        public StudyQuestionnaireLineAnswerSnapshotBuilder WithVersion(string version)
        {
            _entity.KTR_Version = version;
            return this;
        }

        public StudyQuestionnaireLineAnswerSnapshotBuilder WithDisplayOrder(int displayOrder)
        {
            _entity.KTR_DisplayOrder = displayOrder;
            return this;
        }

        public KTR_StudyQuestionAnswerListSnapshot Build()
        {
            return _entity;
        }
    }
}
