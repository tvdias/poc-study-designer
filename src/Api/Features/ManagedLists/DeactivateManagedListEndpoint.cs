using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.ManagedLists;

public static class DeactivateManagedListEndpoint
{
    public static void MapDeactivateManagedListEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/managedlists/{id:guid}/deactivate", HandleAsync)
            .WithName("DeactivateManagedList")
            .WithSummary("Deactivate Managed List")
            .WithTags("ManagedLists");
    }

    public static async Task<Results<Ok, NotFound<string>>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var managedList = await db.ManagedLists.FindAsync(new object[] { id }, cancellationToken);
        if (managedList == null)
        {
            return TypedResults.NotFound($"Managed list with ID '{id}' not found.");
        }

        managedList.Status = ManagedListStatus.Inactive;
        managedList.ModifiedOn = DateTime.UtcNow;
        managedList.ModifiedBy = "System"; // TODO: Replace with real user when auth is available

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok();
    }
}
