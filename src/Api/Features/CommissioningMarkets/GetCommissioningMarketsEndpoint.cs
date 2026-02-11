using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.CommissioningMarkets;

public static class GetCommissioningMarketsEndpoint
{
    public static void MapGetCommissioningMarketsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/commissioning-markets", HandleAsync)
            .WithName("GetCommissioningMarkets")
            .WithSummary("Get Commissioning Markets")
            .WithTags("CommissioningMarkets");
    }

    public static async Task<List<GetCommissioningMarketsResponse>> HandleAsync(
        string? query,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var marketsQuery = db.CommissioningMarkets.Where(m => m.IsActive);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            marketsQuery = marketsQuery.Where(m => EF.Functions.ILike(m.Name, pattern) || EF.Functions.ILike(m.IsoCode, pattern));
        }

        return await marketsQuery
            .Select(m => new GetCommissioningMarketsResponse(m.Id, m.IsoCode, m.Name))
            .ToListAsync(cancellationToken);
    }
}
