using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Tags;

public static class GetTagsEndpoint
{
    public static void MapGetTagsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/tags", HandleAsync)
            .WithName("GetTags")
            .WithSummary("Get Tags")
            .WithTags("Tags");
    }

    public static async Task<Results<Ok<List<GetTagsResponse>>, BadRequest<string>>> HandleAsync(
        string? query,
        ITagService tagService,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await tagService.GetTagsAsync(query, cancellationToken);
            return TypedResults.Ok(response);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }
}
