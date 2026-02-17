namespace DigTx.Designer.FunctionApp.Core.Extensions;

using DigTx.Designer.FunctionApp.Core.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

internal static class OptionsExtensions
{
    internal static T GetOptionsValue<T>(IServiceCollection services)
    where T : class
    {
        var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<IOptions<T>>().Value;
    }

    internal static IServiceCollection AddOptionsDependecies(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .Configure(SettingsHelper.GetDataverseOptions(configuration));

        services
            .Configure(SettingsHelper.GetKeyVaultOptions(configuration));

        return services;
    }
}
