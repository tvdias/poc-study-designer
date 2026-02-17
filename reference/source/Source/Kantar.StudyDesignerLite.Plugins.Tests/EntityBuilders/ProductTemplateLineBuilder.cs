using Microsoft.Xrm.Sdk;
using System;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class ProductTemplateLineBuilder
    {
        private readonly KTR_ProductTemplateLine _entity;

        public ProductTemplateLineBuilder(
            KTR_ProductTemplate productTemplate)
        {
            _entity = new KTR_ProductTemplateLine
            {
                Id = Guid.NewGuid(),
                KTR_ProductTemplate = new EntityReference(productTemplate.LogicalName, productTemplate.Id),
                KTR_DisplayOrder = 0,
                KTR_IncludeByDefault = false,
                KTR_Type = KTR_ProductTemplateLineType.Question,
                StateCode = KTR_ProductTemplateLine_StateCode.Active,
                StatusCode = KTR_ProductTemplateLine_StatusCode.Active,
            };
        }

        public ProductTemplateLineBuilder WithType(KTR_ProductTemplateLineType type)
        {
            _entity.KTR_Type = type;
            return this;
        }

        public ProductTemplateLineBuilder WithIncludeByDefault(bool includeByDefault)
        {
            _entity.KTR_IncludeByDefault = includeByDefault;
            return this;
        }

        public ProductTemplateLineBuilder WithQuestionBank(KT_QuestionBank questionBank)
        {
            _entity.KTR_KT_QuestionBank = new EntityReference(questionBank.LogicalName, questionBank.Id);
            return this;
        }

        public ProductTemplateLineBuilder WithModule(KT_Module module)
        {
            _entity.KTR_KT_Module = new EntityReference(module.LogicalName, module.Id);
            return this;
        }

        public ProductTemplateLineBuilder WithDisplayOrder(int displayOrder)
        {
            _entity.KTR_DisplayOrder = displayOrder;
            return this;
        }

        public KTR_ProductTemplateLine Build()
        {
            return _entity;
        }
    }
}
