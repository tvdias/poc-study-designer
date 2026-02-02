using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Tags;

public static class DeleteTagEndpoint
{
    public static void MapDeleteTagEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/tags/{id}", HandleAsync)
            .WithName("DeleteTag")
            .WithOpenApi();
    }

    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var tag = await db.Tags.FindAsync([id], cancellationToken);

        if (tag is null)
        {
            return TypedResults.NotFound();
        }

        // Soft delete
        tag.IsActive = false;
        tag.ModifiedOn = DateTime.UtcNow;
        tag.ModifiedBy = "System"; // TODO: Replace with real user

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
