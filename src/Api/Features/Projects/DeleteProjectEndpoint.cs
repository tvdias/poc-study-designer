using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Projects;

public static class DeleteProjectEndpoint
{
    public static void MapDeleteProjectEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/projects/{id:guid}", async Task<Results<NoContent, NotFound>> (
            Guid id,
            ApplicationDbContext db,
            CancellationToken ct) =>
        {
            var project = await db.Projects.FindAsync([id], ct);
            if (project == null)
            {
                return TypedResults.NotFound();
            }

            db.Projects.Remove(project);
            await db.SaveChangesAsync(ct);

            return TypedResults.NoContent();
        })
        .WithName("DeleteProject")
        .WithOpenApi();
    }
}
