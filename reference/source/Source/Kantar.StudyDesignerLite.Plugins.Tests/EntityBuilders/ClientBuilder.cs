using System;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class ClientBuilder
    {
        private readonly Account _entity;

        public ClientBuilder()
        {
            _entity = new Account
            {
                Id = Guid.NewGuid(),
                StateCode = Account_StateCode.Active,
                StatusCode = Account_StatusCode.Active,
            };
        }

        public ClientBuilder WithName(string name)
        {
            _entity.Name = name;
            return this;
        }

        public Account Build()
        {
            return _entity;
        }
    }
}
