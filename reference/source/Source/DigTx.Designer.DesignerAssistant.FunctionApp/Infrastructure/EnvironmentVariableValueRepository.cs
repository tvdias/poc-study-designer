namespace DigTx.Designer.FunctionApp.Infrastructure;

using System.Linq;
using DigTx.Designer.FunctionApp.Infrastructure.Interfaces;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;

public class EnvironmentVariableValueRepository : IEnvironmentVariableValueRepository
{
    private const string EnvVariableNameOrgUrl = "ktr_OrgUrl";
    private const string EnvVariableNameAppId = "ktr_AppId";

    private readonly ServiceClient _service;

    public EnvironmentVariableValueRepository(ServiceClient service)
    {
        _service = service;
    }

    public async Task<string> GetEnvironmentVariableNameOrgUrlAsync()
    {
        return await GetAsync(EnvVariableNameOrgUrl);
    }

    public async Task<string> GetEnvironmentVariableNameAppIdAsync()
    {
        return await GetAsync(EnvVariableNameAppId);
    }

    private async Task<string> GetAsync(string name)
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

        var resultCollection = await _service.RetrieveMultipleAsync(query);

        var result = resultCollection.Entities.FirstOrDefault();

        return result?.GetAttributeValue<string>(EnvironmentVariableValue.Fields.Value) ?? string.Empty;
    }
}
