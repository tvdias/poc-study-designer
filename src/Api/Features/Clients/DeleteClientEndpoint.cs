using Api.Data;
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
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var client = await db.Clients.FindAsync([id], cancellationToken);

        if (client is null)
        {
            return TypedResults.NotFound();
        }

        db.Clients.Remove(client);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
