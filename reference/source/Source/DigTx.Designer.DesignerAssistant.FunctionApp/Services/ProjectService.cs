namespace DigTx.Designer.FunctionApp.Services;

using System;
using System.Threading.Tasks;
using DigTx.Designer.FunctionApp.Infrastructure.Interfaces;
using DigTx.Designer.FunctionApp.Services.Interfaces;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Extensions.Logging;

/// <summary>
/// Project Service implementation.
/// </summary>
public partial class ProjectService : IProjectService
{
    private readonly ILogger<ProjectService> _logger;
    private readonly IUnitOfWork _uow;
    private readonly IEnvironmentVariableValueService _environmentVariableValueService;

    public ProjectService(
        ILogger<ProjectService> logger,
        IUnitOfWork uow,
        IEnvironmentVariableValueService environmentVariableValueService)
    {
        _logger = logger
            ?? throw new ArgumentNullException(nameof(ILogger<ProjectService>));
        _uow = uow
            ?? throw new ArgumentNullException(nameof(IProjectRepository));
        _environmentVariableValueService = environmentVariableValueService
            ?? throw new ArgumentNullException(nameof(IEnvironmentVariableValueService));
    }

    public async Task<KT_Project?> GetByIdAsync(Guid id)
    {
        var project = await _uow.ProjectRepository.GetByIdAsync(id);

        if (project is null)
        {
            _logger.LogWarning("Project with ID {ProjectId} not found.", id);
            return null;
        }

        return project;
    }
}
