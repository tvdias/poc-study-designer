using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.FieldworkMarkets;

public static class UpdateFieldworkMarketEndpoint
{
    public static void MapUpdateFieldworkMarketEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/fieldwork-markets/{id}", HandleAsync)
            .WithName("UpdateFieldworkMarket")
            .WithSummary("Update Fieldwork Market")
            .WithTags("FieldworkMarkets");
    }

    public static async Task<Results<Ok<UpdateFieldworkMarketResponse>, NotFound, ValidationProblem>> HandleAsync(
        Guid id,
        UpdateFieldworkMarketRequest request,
        ApplicationDbContext db,
        IValidator<UpdateFieldworkMarketRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var market = await db.FieldworkMarkets.FindAsync([id], cancellationToken);

        if (market is null)
        {
            return TypedResults.NotFound();
        }

        market.IsoCode = request.IsoCode;
        market.Name = request.Name;
        market.IsActive = request.IsActive;
        market.ModifiedOn = DateTime.UtcNow;
        market.ModifiedBy = "System"; // TODO: Replace with real user

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(new UpdateFieldworkMarketResponse(market.Id, market.IsoCode, market.Name, market.IsActive));
    }
}
