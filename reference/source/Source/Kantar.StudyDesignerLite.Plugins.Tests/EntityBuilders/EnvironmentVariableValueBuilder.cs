using System;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class EnvironmentVariableValueBuilder
    {
        private readonly EnvironmentVariableValue _entity;

        public EnvironmentVariableValueBuilder()
        {
            _entity = new EnvironmentVariableValue
            {
                Id = Guid.NewGuid(),
            };
        }

        public EnvironmentVariableValueBuilder WithValue(string value)
        {
            _entity.Value = value;
            return this;
        }

        public EnvironmentVariableValueBuilder WithSchemaName(string schemaName)
        {
            _entity.SchemaName = schemaName;
            return this;
        }

        public EnvironmentVariableValue Build()
        {
            return _entity;
        }
    }
}
