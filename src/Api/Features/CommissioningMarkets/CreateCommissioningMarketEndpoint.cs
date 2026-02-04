using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.CommissioningMarkets;

public static class CreateCommissioningMarketEndpoint
{
    public static void MapCreateCommissioningMarketEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/commissioning-markets", HandleAsync)
            .WithName("CreateCommissioningMarket")
            .WithSummary("Create Commissioning Market")
            .WithTags("CommissioningMarkets");
    }

    public static async Task<Results<CreatedAtRoute<CreateCommissioningMarketResponse>, ValidationProblem, Conflict<string>>> HandleAsync(
        CreateCommissioningMarketRequest request,
        ApplicationDbContext db,
        IValidator<CreateCommissioningMarketRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var market = new CommissioningMarket
        {
            Id = Guid.NewGuid(),
            IsoCode = request.IsoCode,
            Name = request.Name,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System" // TODO: Replace with real user when auth is available
        };

        db.CommissioningMarkets.Add(market);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            // Different database providers use different error codes for unique constraint violations.
            // This is a generic check that looks for common keywords in the inner exception.
            if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
                || ex.InnerException?.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) == true)
            {
                return TypedResults.Conflict($"Commissioning Market with ISO code '{request.IsoCode}' already exists.");
            }

            throw;
        }

        return TypedResults.CreatedAtRoute(new CreateCommissioningMarketResponse(market.Id, market.IsoCode, market.Name, market.IsActive), "GetCommissioningMarketById", new { id = market.Id });
    }
}
