using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.Projects;

public record CreateProjectRequest(
    string Name,
    string? Description,
    Guid? ClientId,
    Guid? CommissioningMarketId,
    Methodology? Methodology,
    Guid? ProductId,
    string? Owner,
    ProjectStatus? Status,
    bool? CostManagementEnabled
);

public record CreateProjectResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? ClientId,
    Guid? CommissioningMarketId,
    Methodology? Methodology,
    Guid? ProductId,
    string? Owner,
    ProjectStatus Status,
    bool CostManagementEnabled,
    DateTime CreatedOn
);

public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.Owner)
            .MaximumLength(100).WithMessage("Owner must not exceed 100 characters");
    }
}

public static class CreateProjectEndpoint
{
    public static void MapCreateProjectEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/projects", async Task<Results<CreatedAtRoute<CreateProjectResponse>, ValidationProblem, Conflict<string>>> (
            CreateProjectRequest request,
            ApplicationDbContext db,
            IValidator<CreateProjectRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return TypedResults.ValidationProblem(validationResult.ToDictionary());
            }

            // Check if a project with the same name already exists
            var existingProject = await db.Projects.FirstOrDefaultAsync(p => p.Name == request.Name, ct);
            if (existingProject != null)
            {
                return TypedResults.Conflict("A project with this name already exists");
            }

            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                ClientId = request.ClientId,
                CommissioningMarketId = request.CommissioningMarketId,
                Methodology = request.Methodology,
                ProductId = request.ProductId,
                Owner = request.Owner,
                Status = request.Status ?? ProjectStatus.Active,
                CostManagementEnabled = request.CostManagementEnabled ?? false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "system"
            };

            db.Projects.Add(project);
            await db.SaveChangesAsync(ct);

            var response = new CreateProjectResponse(
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
                project.CreatedOn
            );

            return TypedResults.CreatedAtRoute(response, "GetProjectById", new { id = project.Id });
        })
        .WithName("CreateProject")
        .WithOpenApi();
    }
}
