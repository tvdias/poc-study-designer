using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.CommissioningMarkets;

public record GetCommissioningMarketByIdResponse(Guid Id, string IsoCode, string Name);

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
        var marketResponse = await db.CommissioningMarkets
            .Where(m => m.IsActive)
            .Where(m => m.Id == id)
            .Select(m => new GetCommissioningMarketByIdResponse(m.Id, m.IsoCode, m.Name))
            .FirstOrDefaultAsync(cancellationToken);

        if (marketResponse is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(marketResponse);
    }
}
