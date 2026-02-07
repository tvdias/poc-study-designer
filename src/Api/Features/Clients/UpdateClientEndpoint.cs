using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.Clients;

public static class UpdateClientEndpoint
{
    public static void MapUpdateClientEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/clients/{id}", HandleAsync)
            .WithName("UpdateClient")
            .WithSummary("Update Client")
            .WithTags("Clients");
    }

    public static async Task<Results<Ok<UpdateClientResponse>, NotFound, ValidationProblem>> HandleAsync(
        Guid id,
        UpdateClientRequest request,
        ApplicationDbContext db,
        IValidator<UpdateClientRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var client = await db.Clients.FindAsync([id], cancellationToken);

        if (client is null)
        {
            return TypedResults.NotFound();
        }

        client.AccountName = request.AccountName;
        client.CompanyNumber = request.CompanyNumber;
        client.CustomerNumber = request.CustomerNumber;
        client.CompanyCode = request.CompanyCode;
        client.IsActive = request.IsActive;
        client.ModifiedOn = DateTime.UtcNow;
        client.ModifiedBy = "System"; // TODO: Replace with real user

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(new UpdateClientResponse(
            client.Id,
            client.AccountName,
            client.CompanyNumber,
            client.CustomerNumber,
            client.CompanyCode,
            client.IsActive,
            client.CreatedOn));
    }
}
