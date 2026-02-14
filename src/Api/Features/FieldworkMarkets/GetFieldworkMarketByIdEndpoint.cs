using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.FieldworkMarkets;

public record GetFieldworkMarketByIdResponse(Guid Id, string IsoCode, string Name);

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
        var marketResponse = await db.FieldworkMarkets
            .Where(m => m.IsActive)
            .Where(m => m.Id == id)
            .Select(m => new GetFieldworkMarketByIdResponse(m.Id, m.IsoCode, m.Name))
            .FirstOrDefaultAsync(cancellationToken);

        if (marketResponse is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(marketResponse);
    }
}
