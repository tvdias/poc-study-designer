using System;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class CommissioningMarketBuilder
    {
        private readonly KT_CommissioningMarket _entity;

        public CommissioningMarketBuilder()
        {
            _entity = new KT_CommissioningMarket
            {
                Id = Guid.NewGuid(),
                StateCode = KT_CommissioningMarket_StateCode.Active,
                StatusCode = KT_CommissioningMarket_StatusCode.Active,
            };
        }

        public CommissioningMarketBuilder WithName(string name)
        {
            _entity.KT_Name = name;
            return this;
        }

        public KT_CommissioningMarket Build()
        {
            return _entity;
        }
    }
}
