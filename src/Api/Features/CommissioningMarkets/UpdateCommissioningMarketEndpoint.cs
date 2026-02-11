using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.CommissioningMarkets;

public static class UpdateCommissioningMarketEndpoint
{
    public static void MapUpdateCommissioningMarketEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/commissioning-markets/{id}", HandleAsync)
            .WithName("UpdateCommissioningMarket")
            .WithSummary("Update Commissioning Market")
            .WithTags("CommissioningMarkets");
    }

    public static async Task<Results<Ok<UpdateCommissioningMarketResponse>, NotFound, ValidationProblem>> HandleAsync(
        Guid id,
        UpdateCommissioningMarketRequest request,
        ApplicationDbContext db,
        IValidator<UpdateCommissioningMarketRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var market = await db.CommissioningMarkets
            .Where(m => m.IsActive)
            .Where(m => m.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (market is null)
        {
            return TypedResults.NotFound();
        }

        market.IsoCode = request.IsoCode;
        market.Name = request.Name;
        market.ModifiedOn = DateTime.UtcNow;
        market.ModifiedBy = "System"; // TODO: Replace with real user

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(new UpdateCommissioningMarketResponse(market.Id, market.IsoCode, market.Name));
    }
}
