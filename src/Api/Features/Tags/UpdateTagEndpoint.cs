using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Tags;

public static class UpdateTagEndpoint
{
    public static void MapUpdateTagEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/tags/{id}", HandleAsync)
            .WithName("UpdateTag")
            .WithSummary("Update Tag")
            .WithTags("Tags");
    }

    public static async Task<Results<Ok<UpdateTagResponse>, NotFound, BadRequest<string>>> HandleAsync(
        Guid id,
        UpdateTagRequest request,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return TypedResults.BadRequest("Tag name is required.");
        }

        var tag = await db.Tags.FindAsync([id], cancellationToken);

        if (tag is null)
        {
            return TypedResults.NotFound();
        }

        tag.Name = request.Name;
        tag.IsActive = request.IsActive;
        tag.ModifiedOn = DateTime.UtcNow;
        tag.ModifiedBy = "System"; // TODO: Replace with real user

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(new UpdateTagResponse(tag.Id, tag.Name, tag.IsActive));
    }
}
