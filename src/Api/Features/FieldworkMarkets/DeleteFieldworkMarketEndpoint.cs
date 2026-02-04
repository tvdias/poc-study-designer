using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.FieldworkMarkets;

public static class DeleteFieldworkMarketEndpoint
{
    public static void MapDeleteFieldworkMarketEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/fieldwork-markets/{id}", HandleAsync)
            .WithName("DeleteFieldworkMarket")
            .WithSummary("Delete Fieldwork Market")
            .WithTags("FieldworkMarkets");
    }

    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var market = await db.FieldworkMarkets.FindAsync([id], cancellationToken);

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
