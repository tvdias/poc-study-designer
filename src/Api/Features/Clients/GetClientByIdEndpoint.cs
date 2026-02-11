using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Clients;

public static class GetClientByIdEndpoint
{
    public static void MapGetClientByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/clients/{id}", HandleAsync)
            .WithName("GetClientById")
            .WithSummary("Get Client By Id")
            .WithTags("Clients");
    }

    public static async Task<Results<Ok<GetClientsResponse>, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var clientResponse = await db.Clients
            .Where(c => c.IsActive)
            .Where(c => c.Id == id)
            .Select(c => new GetClientsResponse(
                c.Id,
                c.AccountName,
                c.CompanyNumber,
                c.CustomerNumber,
                c.CompanyCode,
                c.CreatedOn))
            .FirstOrDefaultAsync(cancellationToken);

        if (clientResponse is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(clientResponse);
    }
}
