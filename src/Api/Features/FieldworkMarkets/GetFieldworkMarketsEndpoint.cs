using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.FieldworkMarkets;

public static class GetFieldworkMarketsEndpoint
{
    public static void MapGetFieldworkMarketsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/fieldwork-markets", HandleAsync)
            .WithName("GetFieldworkMarkets")
            .WithSummary("Get Fieldwork Markets")
            .WithTags("FieldworkMarkets");
    }

    public static async Task<List<GetFieldworkMarketsResponse>> HandleAsync(
        string? query,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var marketsQuery = db.FieldworkMarkets.Where(m => m.IsActive);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            marketsQuery = marketsQuery.Where(m => EF.Functions.ILike(m.Name, pattern) || EF.Functions.ILike(m.IsoCode, pattern));
        }

        return await marketsQuery
            .Select(m => new GetFieldworkMarketsResponse(m.Id, m.IsoCode, m.Name))
            .ToListAsync(cancellationToken);
    }
}
