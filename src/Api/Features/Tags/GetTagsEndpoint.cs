using Api.Data;
using Microsoft.EntityFrameworkCore;

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

    public static async Task<List<GetTagsResponse>> HandleAsync(
        string? query,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var tagsQuery = db.Tags.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            tagsQuery = tagsQuery.Where(t => EF.Functions.ILike(t.Name, pattern));
        }

        return await tagsQuery
            .Select(t => new GetTagsResponse(t.Id, t.Name, t.IsActive))
            .ToListAsync(cancellationToken);
    }
}
