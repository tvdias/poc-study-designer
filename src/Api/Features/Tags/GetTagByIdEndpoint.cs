using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Tags;

public record GetTagByIdResponse(Guid Id, string Name);

public static class GetTagByIdEndpoint
{
    public static void MapGetTagByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/tags/{id}", HandleAsync)
            .WithName("GetTagById")
            .WithSummary("Get Tag by ID")
            .WithDescription("Retrieves a specific tag by its unique identifier.")
            .WithTags("Tags");
    }

    public static async Task<Results<Ok<GetTagByIdResponse>, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var tag = await db.Tags
            .Where(t => t.IsActive)
            .Where(t => t.Id == id)
            .Select(t => new GetTagByIdResponse(t.Id, t.Name))
            .FirstOrDefaultAsync(cancellationToken);

        if (tag is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new GetTagByIdResponse(tag.Id, tag.Name));
    }
}
