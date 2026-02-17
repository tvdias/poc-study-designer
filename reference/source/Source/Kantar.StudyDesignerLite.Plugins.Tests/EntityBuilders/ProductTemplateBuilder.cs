using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class ProductTemplateBuilder
    {
        private readonly KTR_ProductTemplate _entity;

        public ProductTemplateBuilder()
        {
            _entity = new KTR_ProductTemplate
            {
                Id = Guid.NewGuid(),
                StateCode = KTR_ProductTemplate_StateCode.Active,
                StatusCode = KTR_ProductTemplate_StatusCode.Active,
            };
        }

        public ProductTemplateBuilder WithProduct(KTR_Product product)
        {
            _entity.KTR_Product = new EntityReference(product.LogicalName, product.Id);
            return this;
        }

        public ProductTemplateBuilder WithName(string name)
        {
            _entity.KTR_Name = name;
            return this;
        }

        public KTR_ProductTemplate Build()
        {
            return _entity;
        }
    }
}
