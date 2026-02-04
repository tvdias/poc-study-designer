using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.FieldworkMarkets;

public static class CreateFieldworkMarketEndpoint
{
    public static void MapCreateFieldworkMarketEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/fieldwork-markets", HandleAsync)
            .WithName("CreateFieldworkMarket")
            .WithSummary("Create Fieldwork Market")
            .WithTags("FieldworkMarkets");
    }

    public static async Task<Results<CreatedAtRoute<CreateFieldworkMarketResponse>, ValidationProblem, Conflict<string>>> HandleAsync(
        CreateFieldworkMarketRequest request,
        ApplicationDbContext db,
        IValidator<CreateFieldworkMarketRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var market = new FieldworkMarket
        {
            Id = Guid.NewGuid(),
            IsoCode = request.IsoCode,
            Name = request.Name,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System" // TODO: Replace with real user when auth is available
        };

        db.FieldworkMarkets.Add(market);

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
                return TypedResults.Conflict($"Fieldwork Market with ISO code '{request.IsoCode}' already exists.");
            }

            throw;
        }

        return TypedResults.CreatedAtRoute(new CreateFieldworkMarketResponse(market.Id, market.IsoCode, market.Name), "GetFieldworkMarketById", new { id = market.Id });
    }
}
