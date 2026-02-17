namespace DigTx.Designer.FunctionApp.Core.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DigTx.Designer.FunctionApp.Core.Options;
using Microsoft.Extensions.Configuration;

[ExcludeFromCodeCoverage]
internal static class SettingsHelper
{
    internal static Action<DataverseOptions> GetDataverseOptions(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var keyVaultClient = GetKeyVaultClient(configuration);

        var url = configuration["DATAVERSE_URL"];
        var tenantId = configuration["TENANT_ID"];
        var clientId = keyVaultClient.GetSecretAsync("DESIGNER-ASSISTANT-CLIENT-ID").GetAwaiter().GetResult();
        var clientSecret = keyVaultClient.GetSecretAsync("DESIGNER-ASSISTANT-CLIENT-SECRET").GetAwaiter().GetResult();

        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(tenantId);
        ArgumentNullException.ThrowIfNull(clientId);
        ArgumentNullException.ThrowIfNull(clientSecret);

        var connectionString =
            $"AuthType=ClientSecret;Url={url};" +
            $"TenantId={tenantId};ClientId={clientId.Value.Value};ClientSecret={clientSecret.Value.Value};";

        return op =>
        {
            op.Url = url;
            op.ConnectionString = connectionString;
        };
    }

    internal static Action<KeyVaultOptions> GetKeyVaultOptions(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var keyVaultUrl = configuration["KEY_VAULT_URL"]!;

        ArgumentNullException.ThrowIfNull(keyVaultUrl);

        return op =>
        {
            op.Url = keyVaultUrl;
        };
    }

    private static SecretClient GetKeyVaultClient(IConfiguration configuration)
    {
        var keyVaultUrl = configuration["KEY_VAULT_URL"]!;

        var credential = new DefaultAzureCredential();

        return new SecretClient(new Uri(keyVaultUrl), credential);
    }
}
