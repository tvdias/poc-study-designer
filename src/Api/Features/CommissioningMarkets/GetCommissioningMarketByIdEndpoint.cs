using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.CommissioningMarkets;

public record GetCommissioningMarketByIdResponse(Guid Id, string IsoCode, string Name, bool IsActive);

public static class GetCommissioningMarketByIdEndpoint
{
    public static void MapGetCommissioningMarketByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/commissioning-markets/{id}", HandleAsync)
            .WithName("GetCommissioningMarketById")
            .WithSummary("Get Commissioning Market by ID")
            .WithDescription("Retrieves a specific commissioning market by its unique identifier.")
            .WithTags("CommissioningMarkets");
    }

    public static async Task<Results<Ok<GetCommissioningMarketByIdResponse>, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var market = await db.CommissioningMarkets.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (market is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new GetCommissioningMarketByIdResponse(market.Id, market.IsoCode, market.Name, market.IsActive));
    }
}
