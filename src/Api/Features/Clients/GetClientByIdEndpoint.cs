using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;

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
        var client = await db.Clients.FindAsync([id], cancellationToken);

        if (client is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new GetClientsResponse(
            client.Id,
            client.AccountName,
            client.CompanyNumber,
            client.CustomerNumber,
            client.CompanyCode,
            client.IsActive));
    }
}
