using Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api.Features.Projects;

public interface IProjectService
{
    Task<CreateProjectResponse> CreateProjectAsync(
        CreateProjectRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    Task<UpdateProjectResponse> UpdateProjectAsync(
        Guid projectId,
        UpdateProjectRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    Task<GetProjectByIdResponse?> GetProjectByIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);

    Task<GetProjectsResponse> GetProjectsAsync(
        CancellationToken cancellationToken = default);

    Task DeleteProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);
}

public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(ApplicationDbContext context, ILogger<ProjectService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CreateProjectResponse> CreateProjectAsync(
        CreateProjectRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating project: {ProjectName}", request.Name);

        var trimmedName = request.Name.Trim();

        // Check if a project with the same name already exists for this client
        var isNameDuplicated = await _context.Projects.AnyAsync(
            p => p.ClientId == request.ClientId && p.Name.ToLower() == trimmedName.ToLower(),
            cancellationToken);

        if (isNameDuplicated)
        {
            _logger.LogWarning("Duplicate project name for client {ClientId}: {ProjectName}", request.ClientId, trimmedName);
            throw new InvalidOperationException("A project with this name already exists for this client");
        }

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = trimmedName,
            Description = request.Description,
            ClientId = request.ClientId,
            CommissioningMarketId = request.CommissioningMarketId,
            Methodology = request.Methodology,
            ProductId = request.ProductId,
            Owner = request.Owner,
            Status = request.Status ?? ProjectStatus.Active,
            CostManagementEnabled = request.CostManagementEnabled ?? false,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully created project {ProjectId}: {ProjectName}", project.Id, project.Name);

        return new CreateProjectResponse(
            project.Id,
            project.Name,
            project.Description,
            project.ClientId,
            project.CommissioningMarketId,
            project.Methodology,
            project.ProductId,
            project.Owner,
            project.Status,
            project.CostManagementEnabled,
            project.CreatedOn);
    }

    public async Task<UpdateProjectResponse> UpdateProjectAsync(
        Guid projectId,
        UpdateProjectRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating project {ProjectId}", projectId);

        var project = await _context.Projects.FindAsync([projectId], cancellationToken);

        if (project == null)
        {
            throw new InvalidOperationException($"Project {projectId} not found.");
        }

        var trimmedName = request.Name.Trim();

        // Check if another project with the same name exists for this client
        var isNameDuplicated = await _context.Projects.AnyAsync(
            p => p.ClientId == request.ClientId && p.Name.ToLower() == trimmedName.ToLower() && p.Id != projectId,
            cancellationToken);

        if (isNameDuplicated)
        {
            _logger.LogWarning("Duplicate project name for client {ClientId}: {ProjectName}", request.ClientId, trimmedName);
            throw new InvalidOperationException("A project with this name already exists for this client");
        }

        project.Name = trimmedName;
        project.Description = request.Description;
        project.ClientId = request.ClientId;
        project.CommissioningMarketId = request.CommissioningMarketId;
        project.Methodology = request.Methodology;
        project.ProductId = request.ProductId;
        project.Owner = request.Owner;
        project.Status = request.Status;
        project.CostManagementEnabled = request.CostManagementEnabled;
        project.ModifiedOn = DateTime.UtcNow;
        project.ModifiedBy = userId;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated project {ProjectId}", projectId);

        return new UpdateProjectResponse(
            project.Id,
            project.Name,
            project.Description,
            project.ClientId,
            project.CommissioningMarketId,
            project.Methodology,
            project.ProductId,
            project.Owner,
            project.Status,
            project.CostManagementEnabled,
            project.ModifiedOn.Value);
    }

    public async Task<GetProjectByIdResponse?> GetProjectByIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .Include(p => p.Client)
            .Include(p => p.CommissioningMarket)
            .Include(p => p.Product)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            return null;
        }

        var questionnaireLineCount = await _context.QuestionnaireLines
            .CountAsync(ql => ql.ProjectId == projectId, cancellationToken);

        return new GetProjectByIdResponse(
            project.Id,
            project.Name,
            project.Description,
            project.ClientId,
            project.Client?.AccountName,
            project.CommissioningMarketId,
            project.CommissioningMarket?.Name,
            project.Methodology,
            project.ProductId,
            project.Product?.Name,
            project.Owner,
            project.Status,
            project.CostManagementEnabled,
            project.HasStudies,
            project.StudyCount,
            project.LastStudyModifiedOn,
            questionnaireLineCount,
            project.CreatedOn,
            project.CreatedBy,
            project.ModifiedOn,
            project.ModifiedBy);
    }

    public async Task<GetProjectsResponse> GetProjectsAsync(
        CancellationToken cancellationToken = default)
    {
        var projects = await _context.Projects
            .OrderByDescending(p => p.CreatedOn)
            .Include(p => p.Client)
            .Select(p => new ProjectSummary(
                p.Id,
                p.Name,
                p.Description,
                p.ClientId,
                p.Client.AccountName,
                p.CommissioningMarketId,
                p.Status,
                p.HasStudies,
                p.StudyCount,
                p.LastStudyModifiedOn,
                p.CreatedOn,
                p.CreatedBy))
            .ToListAsync(cancellationToken);

        return new GetProjectsResponse(projects);
    }

    public async Task DeleteProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting project {ProjectId}", projectId);

        var project = await _context.Projects.FindAsync([projectId], cancellationToken);

        if (project == null)
        {
            throw new InvalidOperationException($"Project {projectId} not found.");
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully deleted project {ProjectId}", projectId);
    }
}
