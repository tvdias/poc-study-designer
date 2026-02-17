namespace DigTx.Designer.FunctionApp.Core.Healthcheck;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.PowerPlatform.Dataverse.Client;

public class DataverseHealthCheck : IHealthCheck
{
    private readonly ServiceClient _serviceClient;

    public DataverseHealthCheck(ServiceClient serviceClient)
    {
        _serviceClient = serviceClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _serviceClient.ExecuteAsync(new WhoAmIRequest(), cancellationToken);

            return HealthCheckResult.Healthy("Dataverse connection successful.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Dataverse connection failed.", ex);
        }
    }
}
