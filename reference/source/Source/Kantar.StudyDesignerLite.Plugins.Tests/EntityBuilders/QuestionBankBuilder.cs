using System;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class QuestionBankBuilder
    {
        private readonly KT_QuestionBank _entity;

        public QuestionBankBuilder()
        {
            _entity = new KT_QuestionBank
            {
                Id = Guid.NewGuid(),
                KT_StandardOrCustom = KT_QuestionBank_KT_StandardOrCustom.Standard,
                KT_QuestionType = KT_QuestionType.SingleChoice,
                StateCode = KT_QuestionBank_StateCode.Active,
                StatusCode = KT_QuestionBank_StatusCode.Active,
            };
        }

        public QuestionBankBuilder WithQuestionType(KT_QuestionType type)
        {
            _entity.KT_QuestionType = type;
            return this;
        }

        public QuestionBankBuilder WithStandardOrCustom(KT_QuestionBank_KT_StandardOrCustom standardOrCustom)
        {
            _entity.KT_StandardOrCustom = standardOrCustom;
            return this;
        }

        public QuestionBankBuilder WithName(string name)
        {
            _entity.KT_Name = name;
            return this;
        }
        public QuestionBankBuilder WithId(Guid id)
        {
            _entity.Id = id;
            return this;
        }

        public QuestionBankBuilder WithTitle(string questionTitle)
        {
            _entity.KT_QuestionTitle = questionTitle;
            return this;
        }

        public QuestionBankBuilder WithQuestionText(string questionText)
        {
            _entity.KT_DefaultQuestionText = questionText;
            return this;
        }

        public QuestionBankBuilder WithSingleOrMulticoded(KT_SingleOrMultiCode code)
        {
            _entity.KT_SingleOrMultiCode = code;
            return this;
        }
        public QuestionBankBuilder WithAnswerMin(int? answerMin)
        {
            _entity.KTR_AnswerMin = answerMin;
            return this;
        }
        public QuestionBankBuilder WithAnswerMax(int? answerMax)
        {
            _entity.KTR_AnswerMax = answerMax;
            return this;
        }
        public QuestionBankBuilder WithQuestionFormatDetails(string questionFormatDetails)
        {
            _entity.KTR_QuestionFormatDetails = questionFormatDetails;
            return this;
        }
        public QuestionBankBuilder WithCustomNotes(string customNotes)
        {
            _entity.KTR_CustomNotes = customNotes;
            return this;
        }
        public QuestionBankBuilder WithQuestionRationale(string questionRationale)
        {
            _entity.KT_QuestionRationale = questionRationale;
            return this;
        }

        public KT_QuestionBank Build()
        {
            return _entity;
        }
    }
}
