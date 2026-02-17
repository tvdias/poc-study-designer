using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class TagQuestionBankBuilder
    {
        private readonly KTR_Tag_KT_QuestionBank _entity;

        public TagQuestionBankBuilder()
        {
            _entity = new KTR_Tag_KT_QuestionBank
            {
                Id = Guid.NewGuid(),
            };
        }

        public TagQuestionBankBuilder WithTagAndQuestionBank(KTR_Tag tag, KT_QuestionBank questionBank)
        {
            _entity[KTR_Tag_KT_QuestionBank.Fields.KTR_TagId] = tag.Id;
            _entity[KTR_Tag_KT_QuestionBank.Fields.KT_QuestionBankId] = questionBank.Id;
            return this;
        }

        public KTR_Tag_KT_QuestionBank Build()
        {
            return _entity;
        }
    }
}
