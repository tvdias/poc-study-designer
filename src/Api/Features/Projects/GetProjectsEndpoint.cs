using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Projects;

public record GetProjectsResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? ClientId,
    string? ClientName,
    Guid? ProductId,
    string? ProductName,
    string? Owner,
    ProjectStatus Status,
    bool CostManagementEnabled,
    DateTime? ModifiedOn,
    DateTime CreatedOn
);

public static class GetProjectsEndpoint
{
    public static void MapGetProjectsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/projects", async Task<Ok<List<GetProjectsResponse>>> (
            ApplicationDbContext db,
            string? query,
            CancellationToken ct) =>
        {
            var projectsQuery = db.Projects
                .Include(p => p.Client)
                .Include(p => p.Product)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                projectsQuery = projectsQuery.Where(p =>
                    p.Name.Contains(query) ||
                    (p.Description != null && p.Description.Contains(query)) ||
                    (p.Owner != null && p.Owner.Contains(query))
                );
            }

            var projects = await projectsQuery
                .OrderByDescending(p => p.ModifiedOn ?? p.CreatedOn)
                .Select(p => new GetProjectsResponse(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.ClientId,
                    p.Client != null ? p.Client.AccountName : null,
                    p.ProductId,
                    p.Product != null ? p.Product.Name : null,
                    p.Owner,
                    p.Status,
                    p.CostManagementEnabled,
                    p.ModifiedOn,
                    p.CreatedOn
                ))
                .ToListAsync(ct);

            return TypedResults.Ok(projects);
        })
        .WithName("GetProjects")
        .WithOpenApi();
    }
}
