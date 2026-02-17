namespace DigTx.Designer.FunctionApp.Core.Extensions;

using Azure.Identity;
using DigTx.Designer.FunctionApp.Core.Healthcheck;
using DigTx.Designer.FunctionApp.Core.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.PowerPlatform.Dataverse.Client;

internal static class HealthCheckExtensions
{
    internal static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var dataverseOptions = OptionsExtensions
            .GetOptionsValue<DataverseOptions>(services);

        var kvOptions = OptionsExtensions
            .GetOptionsValue<KeyVaultOptions>(services);

        var serviceClient = new ServiceClient(dataverseOptions.ConnectionString);

        services.AddHealthChecks()
            .AddAzureKeyVault(
            new Uri(kvOptions.Url),
            new DefaultAzureCredential(),
            options =>
            {
                options.AddSecret("DESIGNER-ASSISTANT-CLIENT-ID");
            },
            name: "azure-keyvault",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["azure", "keyvault"])
            .AddCheck(
            "Dataverse",
            new DataverseHealthCheck(serviceClient),
            failureStatus: HealthStatus.Unhealthy,
            tags: ["Dataverse"]);

        return services;
    }
}
