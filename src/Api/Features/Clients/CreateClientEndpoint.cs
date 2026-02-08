using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.Clients;

public static class CreateClientEndpoint
{
    public static void MapCreateClientEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/clients", HandleAsync)
            .WithName("CreateClient")
            .WithSummary("Create Client")
            .WithTags("Clients");
    }

    public static async Task<Results<CreatedAtRoute<CreateClientResponse>, ValidationProblem, Conflict<string>>> HandleAsync(
        CreateClientRequest request,
        ApplicationDbContext db,
        IValidator<CreateClientRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var client = new Client
        {
            Id = Guid.NewGuid(),
            AccountName = request.AccountName,
            CompanyNumber = request.CompanyNumber,
            CustomerNumber = request.CustomerNumber,
            CompanyCode = request.CompanyCode,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System" // TODO: Replace with real user when auth is available
        };

        db.Clients.Add(client);

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
                return TypedResults.Conflict($"Client '{request.AccountName}' already exists.");
            }

            throw;
        }

        return TypedResults.CreatedAtRoute(
            new CreateClientResponse(client.Id, client.AccountName, client.CompanyNumber, client.CustomerNumber, client.CompanyCode, client.IsActive, client.CreatedOn),
            "GetClientById",
            new { id = client.Id });
    }
}
