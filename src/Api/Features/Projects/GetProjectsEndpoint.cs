using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Projects;

public record GetProjectsResponse(
    List<ProjectSummary> Projects
);

public record ProjectSummary(
    Guid Id,
    string Name,
    string? Description,
    Guid? ClientId,
    string? ClientName,
    Guid? CommissioningMarketId,
    Methodology? Methodology,
    bool HasStudies,
    int StudyCount,
    DateTime? LastStudyModifiedOn,
    DateTime CreatedOn,
    string CreatedBy
);

public static class GetProjectsEndpoint
{
    public static void MapGetProjectsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/projects", async Task<Ok<GetProjectsResponse>> (
            IProjectService projectService,
            string? query,
            CancellationToken ct) =>
        {
            var response = await projectService.GetProjectsAsync(ct);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var pattern = query.Trim().ToLower();
                var filtered = response.Projects
                    .Where(p => p.Name.ToLower().Contains(pattern) ||
                                (p.Description != null && p.Description.ToLower().Contains(pattern)))
                    .ToList();
                return TypedResults.Ok(new GetProjectsResponse(filtered));
            }

            return TypedResults.Ok(response);
        })
        .WithName("GetProjects")
        .WithOpenApi();
    }
}
