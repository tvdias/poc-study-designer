using Microsoft.AspNetCore.Http.HttpResults;

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
        ITagService tagService,
        CancellationToken cancellationToken)
    {
        var tag = await tagService.GetTagByIdAsync(id, cancellationToken);

        if (tag is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(tag);
    }
}
