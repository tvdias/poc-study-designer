using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.Linq;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Services.General
{
    public class EnvironmentVariablesService : IEnvironmentVariablesService
    {
        private readonly IOrganizationService _service;

        public EnvironmentVariablesService(IOrganizationService service)
        {
            _service = service;
        }

        public string GetEnvironmentVariableValue(string name)
        {
            var query = new QueryExpression(EnvironmentVariableValue.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(EnvironmentVariableValue.Fields.Value),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(EnvironmentVariableValue.Fields.SchemaName, ConditionOperator.Equal, name)
                    }
                }
            };

            var result = _service.RetrieveMultiple(query)
                .Entities
                .FirstOrDefault();
            return result?.GetAttributeValue<string>(EnvironmentVariableValue.Fields.Value);
        }
    }
}
