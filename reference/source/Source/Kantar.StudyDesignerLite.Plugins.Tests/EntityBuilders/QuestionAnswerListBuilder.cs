using System;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class QuestionAnswerListBuilder
    {
        private readonly KTR_QuestionAnswerList _entity;

        public QuestionAnswerListBuilder(KT_QuestionBank question)
        {
            _entity = new KTR_QuestionAnswerList
            {
                Id = Guid.NewGuid(),
                StateCode = KTR_QuestionAnswerList_StateCode.Active,
                StatusCode = KTR_QuestionAnswerList_StatusCode.Active,
                KTR_KT_QuestionBank = new Microsoft.Xrm.Sdk.EntityReference(question.LogicalName, question.Id)
            };
        }

        public QuestionAnswerListBuilder()
        {
            _entity = new KTR_QuestionAnswerList
            {
                Id = Guid.NewGuid(),
                StateCode = KTR_QuestionAnswerList_StateCode.Active,
                StatusCode = KTR_QuestionAnswerList_StatusCode.Active,
            };
        }

        public QuestionAnswerListBuilder WithText(string text)
        {
            _entity.KTR_AnswerText = text;
            return this;
        }

        public QuestionAnswerListBuilder WithSortOrder(int sortOrder)
        {
            _entity.KTR_DisplayOrder = sortOrder;
            return this;
        }

        public QuestionAnswerListBuilder WithState(int state)
        {
            if (state == 0) // Active
            {
                _entity.StateCode = KTR_QuestionAnswerList_StateCode.Active;
                _entity.StatusCode = KTR_QuestionAnswerList_StatusCode.Active;
            }
            else if (state == 1) // Inactive
            {
                _entity.StateCode = KTR_QuestionAnswerList_StateCode.Inactive;
                _entity.StatusCode = KTR_QuestionAnswerList_StatusCode.Inactive;
            }
            return this;
        }

        public QuestionAnswerListBuilder WithId(Guid id)
        {
            _entity.Id = id;
            return this;
        }

        public KTR_QuestionAnswerList Build()
        {
            return _entity;
        }
    }
}
