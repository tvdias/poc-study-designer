using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.ManagedLists;

public static class DeleteManagedListEndpoint
{
    public static void MapDeleteManagedListEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/managedlists/{id:guid}", HandleAsync)
            .WithName("DeleteManagedList")
            .WithSummary("Delete Managed List")
            .WithTags("ManagedLists");
    }

    public static async Task<Results<NoContent, NotFound<string>>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var managedList = await db.ManagedLists.FindAsync(new object[] { id }, cancellationToken);
        if (managedList == null)
        {
            return TypedResults.NotFound($"Managed list with ID '{id}' not found.");
        }

        db.ManagedLists.Remove(managedList);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
