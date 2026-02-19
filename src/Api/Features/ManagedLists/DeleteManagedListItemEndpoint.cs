using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.ManagedLists;

public static class DeleteManagedListItemEndpoint
{
    public static void MapDeleteManagedListItemEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/managedlists/{managedListId:guid}/items/{itemId:guid}", HandleAsync)
            .WithName("DeleteManagedListItem")
            .WithSummary("Delete Managed List Item")
            .WithTags("ManagedLists");
    }

    public static async Task<Results<NoContent, NotFound<string>>> HandleAsync(
        Guid managedListId,
        Guid itemId,
        ApplicationDbContext db,
        ISubsetManagementService subsetService,
        CancellationToken cancellationToken)
    {
        var item = await db.ManagedListItems.FindAsync(new object[] { itemId }, cancellationToken);
        if (item == null || item.ManagedListId != managedListId)
        {
            return TypedResults.NotFound($"Managed list item with ID '{itemId}' not found in managed list '{managedListId}'.");
        }

        // Trigger subset refresh before deletion (AC-SYNC-04)
        await subsetService.InvalidateSubsetsForItemAsync(itemId, "System", cancellationToken);

        db.ManagedListItems.Remove(item);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
