namespace DigTx.IdGeneratorService.FunctionApp.Core.Extensions;

using System;
using System.Diagnostics.CodeAnalysis;
using DigTx.Designer.DesignerAssistant.FunctionApp.Infrastructure.ExecutionContext;
using DigTx.Designer.DesignerAssistant.FunctionApp.Infrastructure.ExecutionContext.Interfaces;
using DigTx.Designer.FunctionApp.Core.Extensions;
using DigTx.Designer.FunctionApp.Core.Options;
using DigTx.Designer.FunctionApp.Infrastructure;
using DigTx.Designer.FunctionApp.Infrastructure.Interfaces;
using DigTx.Designer.FunctionApp.Services;
using DigTx.Designer.FunctionApp.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.PowerPlatform.Dataverse.Client;

[ExcludeFromCodeCoverage]
internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddServiceCollection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthAndCurrentUser(configuration);
        services.AddDataverse();
        services.AddInfrastructure();
        services.AddDataverseAndServices();
        return services;
    }

    private static void AddAuthAndCurrentUser(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var audience = configuration["AUDIENCE"]!;
        var tenantId = configuration["TENANT_ID"];
        var validIssuers = new[]
        {
            $"https://sts.windows.net/{tenantId}/",
            $"https://login.microsoftonline.com/{tenantId}/v2.0"
        };
        var authority = $"https://login.microsoftonline.com/{tenantId}";

        services.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(_ =>
        {
            var mgr = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{authority}/v2.0/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever());
            mgr.AutomaticRefreshInterval = TimeSpan.FromHours(24);
            mgr.RefreshInterval = TimeSpan.FromMinutes(30);
            return mgr;
        });

        services.AddSingleton(new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = validIssuers,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(2),
        });

        services.AddScoped<IFunctionContextAccessor, FunctionContextAccessor>();
        services.AddScoped<ICurrentUser, CurrentUser>();
    }

    private static void AddDataverse(this IServiceCollection services)
    {
        var dataverseOptions = OptionsExtensions
            .GetOptionsValue<DataverseOptions>(services);

        services.AddScoped(_ =>
        {
            var client = new ServiceClient(dataverseOptions.ConnectionString);

            if (!client.IsReady)
            {
                throw new Exception($"Dataverse connection failed: {client.LastError}");
            }

            return client;
        });
    }

    private static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void AddDataverseAndServices(this IServiceCollection services)
    {
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IEnvironmentVariableValueService, EnvironmentVariableValueService>();
    }
}
