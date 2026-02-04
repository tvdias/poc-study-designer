using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.CommissioningMarkets;

public static class DeleteCommissioningMarketEndpoint
{
    public static void MapDeleteCommissioningMarketEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/commissioning-markets/{id}", HandleAsync)
            .WithName("DeleteCommissioningMarket")
            .WithSummary("Delete Commissioning Market")
            .WithTags("CommissioningMarkets");
    }

    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var market = await db.CommissioningMarkets.FindAsync([id], cancellationToken);

        if (market is null)
        {
            return TypedResults.NotFound();
        }

        // Soft delete
        market.IsActive = false;
        market.ModifiedOn = DateTime.UtcNow;
        market.ModifiedBy = "System"; // TODO: Replace with real user

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
