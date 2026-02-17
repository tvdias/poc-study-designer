using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class ProductBuilder
    {
        private readonly KTR_Product _entity;
        public ProductBuilder()
        {
            _entity = new KTR_Product
            {
                Id = Guid.NewGuid(),
                StateCode = KTR_Product_StateCode.Active,
                StatusCode = KTR_Product_StatusCode.Active,
            };
        }

        public ProductBuilder WithName(string name)
        {
            _entity.KTR_Name = name;
            return this;
        }

        public KTR_Product Build()
        {
            return _entity;
        }
    }
}
