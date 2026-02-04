using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.FieldworkMarkets;

public record GetFieldworkMarketByIdResponse(Guid Id, string IsoCode, string Name, bool IsActive);

public static class GetFieldworkMarketByIdEndpoint
{
    public static void MapGetFieldworkMarketByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/fieldwork-markets/{id}", HandleAsync)
            .WithName("GetFieldworkMarketById")
            .WithSummary("Get Fieldwork Market by ID")
            .WithDescription("Retrieves a specific fieldwork market by its unique identifier.")
            .WithTags("FieldworkMarkets");
    }

    public static async Task<Results<Ok<GetFieldworkMarketByIdResponse>, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var market = await db.FieldworkMarkets.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (market is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new GetFieldworkMarketByIdResponse(market.Id, market.IsoCode, market.Name, market.IsActive));
    }
}
