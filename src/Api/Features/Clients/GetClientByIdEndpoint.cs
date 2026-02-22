using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Clients;

public record GetClientByIdResponse(
    Guid Id,
    string AccountName,
    string? CompanyNumber,
    string? CustomerNumber,
    string? CompanyCode,
    DateTime CreatedOn,
    string CreatedBy,
    DateTime? ModifiedOn,
    string? ModifiedBy);

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
