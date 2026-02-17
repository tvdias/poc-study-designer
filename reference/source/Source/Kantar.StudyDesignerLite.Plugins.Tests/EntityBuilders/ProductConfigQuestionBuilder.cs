using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class ProductConfigQuestionBuilder
    {
        private readonly KTR_ProductConfigQuestion _entity;
        public ProductConfigQuestionBuilder(KTR_Product product, KTR_ConfigurationQuestion configQuestion)
        {
            _entity = new KTR_ProductConfigQuestion()
            {
                Id = Guid.NewGuid(),
                KTR_Product = new EntityReference(product.LogicalName, product.Id),
                KTR_ConfigurationQuestion = new EntityReference(configQuestion.LogicalName, configQuestion.Id),
                StateCode = KTR_ProductConfigQuestion_StateCode.Active,
                StatusCode = KTR_ProductConfigQuestion_StatusCode.Active,
            };
        }

        public ProductConfigQuestionBuilder WithSortOrder(int sortOrder)
        {
            _entity.KTR_DisplayOrder = sortOrder;
            return this;
        }

        public ProductConfigQuestionBuilder WithState(KTR_ProductConfigQuestion_StateCode stateCode)
        {
            _entity.StateCode = stateCode;
            return this;
        }

        public ProductConfigQuestionBuilder WithId(Guid id)
        {
            _entity.Id = id;
            return this;
        }

        public KTR_ProductConfigQuestion Build()
        {
            return _entity;
        }

    }
}
