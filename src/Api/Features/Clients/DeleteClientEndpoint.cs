using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Clients;

public static class DeleteClientEndpoint
{
    public static void MapDeleteClientEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/clients/{id}", HandleAsync)
            .WithName("DeleteClient")
            .WithSummary("Delete Client")
            .WithTags("Clients");
    }

    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        Guid id,
        IClientService clientService,
        CancellationToken cancellationToken)
    {
        try
        {
            await clientService.DeleteClientAsync(id, cancellationToken);
            return TypedResults.NoContent();
        }
        catch (InvalidOperationException)
        {
            return TypedResults.NotFound();
        }
    }
}
