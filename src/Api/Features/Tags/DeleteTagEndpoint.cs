using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Tags;

public static class DeleteTagEndpoint
{
    public static void MapDeleteTagEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/tags/{id}", HandleAsync)
            .WithName("DeleteTag")
            .WithSummary("Delete Tag")
            .WithTags("Tags");
    }

    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        Guid id,
        ITagService tagService,
        CancellationToken cancellationToken)
    {
        try
        {
            await tagService.DeleteTagAsync(id, cancellationToken);
            return TypedResults.NoContent();
        }
        catch (InvalidOperationException)
        {
            return TypedResults.NotFound();
        }
    }
}
