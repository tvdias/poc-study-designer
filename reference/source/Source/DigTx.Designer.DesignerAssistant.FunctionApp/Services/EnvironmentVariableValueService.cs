namespace DigTx.Designer.FunctionApp.Services;

using System;
using System.Threading.Tasks;
using DigTx.Designer.FunctionApp.Infrastructure.Interfaces;
using DigTx.Designer.FunctionApp.Services.Interfaces;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Extensions.Logging;

public class EnvironmentVariableValueService : IEnvironmentVariableValueService
{
    private readonly ILogger<EnvironmentVariableValueService> _logger;
    private readonly IUnitOfWork _uow;

    public EnvironmentVariableValueService(
        ILogger<EnvironmentVariableValueService> logger,
        IUnitOfWork uow)
    {
        _logger = logger
            ?? throw new ArgumentNullException(nameof(ILogger<EnvironmentVariableValueService>));
        _uow = uow
            ?? throw new ArgumentNullException(nameof(IProjectRepository));
    }

    public async Task<string> GetProjectUrlAsync(Guid projectId)
    {
        var envVariableNameOrgUrl = await GetNameOrgUrlAsync();

        var envVariableNameAppId = await GetNameAppIdAsync();

        return BuildEntityUrl(
            envVariableNameOrgUrl,
            envVariableNameAppId,
            KT_Project.EntityLogicalName,
            projectId);
    }

    private static string BuildEntityUrl(
        string envVariableNameOrgUrl,
        string envVariableNameAppId,
        string entityLogicalName,
        Guid entityId)
    {
        if (string.IsNullOrEmpty(envVariableNameOrgUrl))
        {
            throw new InvalidOperationException("Organization URL must be provided.");
        }

        if (string.IsNullOrEmpty(envVariableNameAppId))
        {
            throw new InvalidOperationException("App ID must be provided.");
        }

        if (string.IsNullOrEmpty(entityLogicalName))
        {
            throw new InvalidOperationException("Entity logical name must be provided.");
        }

        if (entityId == Guid.Empty)
        {
            throw new InvalidOperationException("Entity Id must be provided.");
        }

        return $"{envVariableNameOrgUrl}/main.aspx?appid={envVariableNameAppId}&pagetype=entityrecord&etn={entityLogicalName}&id={entityId}";
    }

    private async Task<string> GetNameOrgUrlAsync()
    {
        return await _uow.EnvironmentVariableValueRepository.GetEnvironmentVariableNameOrgUrlAsync();
    }

    private async Task<string> GetNameAppIdAsync()
    {
        return await _uow.EnvironmentVariableValueRepository.GetEnvironmentVariableNameAppIdAsync();
    }

}
