namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class SubsetEntitiesBuilder
    {
        private KTR_SubsetEntities _entity;

        public SubsetEntitiesBuilder(KTR_ManagedListEntity managedListEntity, KTR_SubsetDefinition subsetDefinition)
        {
            _entity = new KTR_SubsetEntities()
            {
                Id = Guid.NewGuid(),
                StatusCode = KTR_SubsetEntities_StatusCode.Active,
                StateCode = KTR_SubsetEntities_StateCode.Active,
                KTR_ManagedListEntity = managedListEntity.ToEntityReference(),
                KTR_SubsetDeFinTion = subsetDefinition.ToEntityReference()
            };
        }

        public SubsetEntitiesBuilder WithName(string name)
        {
            _entity[KTR_SubsetEntities.Fields.KTR_Name] = name;
            return this;
        }

        public KTR_SubsetEntities Build()
        {
            return _entity;
        }
    }
}
