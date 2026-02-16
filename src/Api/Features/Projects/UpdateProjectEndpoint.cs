using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.Projects;

public record UpdateProjectRequest(
    string Name,
    string? Description,
    Guid? ClientId,
    Guid? ProductId,
    string? Owner,
    ProjectStatus Status,
    bool CostManagementEnabled
);

public record UpdateProjectResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? ClientId,
    Guid? ProductId,
    string? Owner,
    ProjectStatus Status,
    bool CostManagementEnabled,
    DateTime ModifiedOn
);

public class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectRequestValidator()
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

public static class UpdateProjectEndpoint
{
    public static void MapUpdateProjectEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/projects/{id:guid}", async Task<Results<Ok<UpdateProjectResponse>, ValidationProblem, NotFound, Conflict<string>>> (
            Guid id,
            UpdateProjectRequest request,
            ApplicationDbContext db,
            IValidator<UpdateProjectRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return TypedResults.ValidationProblem(validationResult.ToDictionary());
            }

            var project = await db.Projects.FindAsync([id], ct);
            if (project == null)
            {
                return TypedResults.NotFound();
            }

            // Check if another project with the same name exists
            var existingProject = await db.Projects.FirstOrDefaultAsync(
                p => p.Name == request.Name && p.Id != id, ct);
            if (existingProject != null)
            {
                return TypedResults.Conflict("A project with this name already exists");
            }

            project.Name = request.Name;
            project.Description = request.Description;
            project.ClientId = request.ClientId;
            project.ProductId = request.ProductId;
            project.Owner = request.Owner;
            project.Status = request.Status;
            project.CostManagementEnabled = request.CostManagementEnabled;
            project.ModifiedOn = DateTime.UtcNow;
            project.ModifiedBy = "system";

            await db.SaveChangesAsync(ct);

            var response = new UpdateProjectResponse(
                project.Id,
                project.Name,
                project.Description,
                project.ClientId,
                project.ProductId,
                project.Owner,
                project.Status,
                project.CostManagementEnabled,
                project.ModifiedOn.Value
            );

            return TypedResults.Ok(response);
        })
        .WithName("UpdateProject")
        .WithOpenApi();
    }
}
