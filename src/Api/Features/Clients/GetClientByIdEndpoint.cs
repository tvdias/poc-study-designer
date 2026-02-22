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

    public static async Task<Results<Ok<GetClientByIdResponse>, NotFound>> HandleAsync(
        Guid id,
        IClientService clientService,
        CancellationToken cancellationToken)
    {
        var clientResponse = await clientService.GetClientByIdAsync(id, cancellationToken);

        if (clientResponse is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(clientResponse);
    }
}
