using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.ManagedLists;

public static class GetManagedListByIdEndpoint
{
    public static void MapGetManagedListByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/managedlists/{id:guid}", HandleAsync)
            .WithName("GetManagedListById")
            .WithSummary("Get Managed List by ID")
            .WithTags("ManagedLists");
    }

    public static async Task<Results<Ok<GetManagedListByIdResponse>, NotFound<string>>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var managedList = await db.ManagedLists
            .Include(ml => ml.Items.Where(i => i.IsActive))
            .FirstOrDefaultAsync(ml => ml.Id == id, cancellationToken);

        if (managedList == null)
        {
            return TypedResults.NotFound($"Managed list with ID '{id}' not found.");
        }

        var items = managedList.Items
            .OrderBy(i => i.SortOrder)
            .Select(i => new ManagedListItemDto(i.Id, i.Value, i.Label, i.SortOrder, i.IsActive))
            .ToList();

        var response = new GetManagedListByIdResponse(
            managedList.Id,
            managedList.ProjectId,
            managedList.Name,
            managedList.Description,
            managedList.Status,
            managedList.CreatedOn,
            managedList.CreatedBy,
            managedList.ModifiedOn,
            managedList.ModifiedBy,
            items);

        return TypedResults.Ok(response);
    }
}
