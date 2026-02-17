using Microsoft.Xrm.Sdk;
using System;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class ManagedListEntityBuilder
    {
        private readonly KTR_ManagedListEntity _entity;

        public ManagedListEntityBuilder()
        {
            _entity = new KTR_ManagedListEntity
            {
                Id = Guid.NewGuid(),
                StatusCode = KTR_ManagedListEntity_StatusCode.Active,
                StateCode = KTR_ManagedListEntity_StateCode.Active
            };
        }

        public ManagedListEntityBuilder(KTR_ManagedList managedList)
        {
            _entity = new KTR_ManagedListEntity
            {
                Id = Guid.NewGuid(),
                StatusCode = KTR_ManagedListEntity_StatusCode.Active,
                StateCode = KTR_ManagedListEntity_StateCode.Active,
                KTR_ManagedList = new EntityReference(managedList.LogicalName, managedList.Id)
            };
        }

        public ManagedListEntityBuilder WithId(Guid id)
        {
            _entity.Id = id;
            return this;
        }

        public ManagedListEntityBuilder WithEverinSnapshot(bool value)
        {
            _entity.KTR_EverInSnapshot = value;
            return this;
        }

        public ManagedListEntityBuilder WithManagedList(KTR_ManagedList managedList)
        {
            _entity.KTR_ManagedList = new EntityReference(managedList.LogicalName, managedList.Id);
            return this;
        }

        public ManagedListEntityBuilder WithManagedList(EntityReference managedListRef)
        {
            _entity.KTR_ManagedList = managedListRef;
            return this;
        }

        public ManagedListEntityBuilder WithAnswerCode(string answerCode)
        {
            _entity.KTR_AnswerCode = answerCode;
            return this;
        }

        public ManagedListEntityBuilder WithAnswerText(string answerText)
        {
            _entity.KTR_AnswerText = answerText;
            return this;
        }

        public ManagedListEntityBuilder WithAnswerTextValue(string answerTextValue)
        {
            _entity.KTR_AnswerTextValue = answerTextValue;
            return this;
        }

        public ManagedListEntityBuilder WithDisplayOrder(int displayOrder)
        {
            _entity.KTR_DisplayOrder = displayOrder;
            return this;
        }

        public ManagedListEntityBuilder WithEverInSnapshot(bool everInSnapshot)
        {
            _entity.KTR_EverInSnapshot = everInSnapshot;
            return this;
        }

        public ManagedListEntityBuilder WithSource(KTR_ManagedListEntity_KTR_Source source)
        {
            _entity.KTR_Source = source;
            return this;
        }

        public ManagedListEntityBuilder WithStatusCode(KTR_ManagedListEntity_StatusCode statusCode)
        {
            _entity.StatusCode = statusCode;
            return this;
        }

        public ManagedListEntityBuilder WithStateCode(KTR_ManagedListEntity_StateCode stateCode)
        {
            _entity.StateCode = stateCode;
            return this;
        }

        public KTR_ManagedListEntity Build()
        {
            return _entity;
        }
    }
}
