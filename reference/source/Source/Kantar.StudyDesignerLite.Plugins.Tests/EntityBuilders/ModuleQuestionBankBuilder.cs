using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class ModuleQuestionBankBuilder
    {
        private readonly KTR_ModuleQuestionBank _entity;

        public ModuleQuestionBankBuilder(
            KT_Module module,
            KT_QuestionBank questionBank)
        {
            _entity = new KTR_ModuleQuestionBank
            {
                Id = Guid.NewGuid(),
                KTR_Module = new EntityReference(module.LogicalName, module.Id),
                KTR_QuestionBank = new EntityReference(questionBank.LogicalName, questionBank.Id),
                StateCode = KTR_ModuleQuestionBank_StateCode.Active,
                StatusCode = KTR_ModuleQuestionBank_StatusCode.Active,
            };
        }

        public ModuleQuestionBankBuilder WithSortOrder(int displayOrder)
        {
            _entity.KTR_SortOrder = displayOrder;
            return this;
        }

        public KTR_ModuleQuestionBank Build()
        {
            return _entity;
        }
    }
}
