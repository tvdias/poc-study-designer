using Microsoft.Xrm.Sdk;
using System;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class ManagedListBuilder
    {
        private readonly KTR_ManagedList _entity;

        public ManagedListBuilder(KT_Project project)
        {
            _entity = new KTR_ManagedList
            {
                Id = Guid.NewGuid(),
                StatusCode = KTR_ManagedList_StatusCode.Active,
                KTR_Project = new EntityReference(project.LogicalName, project.Id),
                KTR_EverInSnapshot = false,
            };
        }

        public ManagedListBuilder WithName(string name)
        {
            _entity.KTR_Name = name;
            return this;
        }

        public ManagedListBuilder WithId(Guid id)
        {
            _entity.Id = id;
            return this;
        }

        public ManagedListBuilder WithEverInSnapshot(bool everInSnapshot)
        {
            _entity.KTR_EverInSnapshot = everInSnapshot;
            return this;
        }

        public KTR_ManagedList Build()
        {
            return _entity;
        }
    }
}
