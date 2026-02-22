using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Projects;

public static class DeleteProjectEndpoint
{
    public static void MapDeleteProjectEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/projects/{id:guid}", async Task<Results<NoContent, NotFound>> (
            Guid id,
            IProjectService projectService,
            CancellationToken ct) =>
        {
            try
            {
                await projectService.DeleteProjectAsync(id, ct);
                return TypedResults.NoContent();
            }
            catch (InvalidOperationException)
            {
                return TypedResults.NotFound();
            }
        })
        .WithName("DeleteProject")
        .WithOpenApi();
    }
}
