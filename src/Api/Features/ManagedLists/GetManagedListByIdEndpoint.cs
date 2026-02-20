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
            .Include(ml => ml.Items)
            .FirstOrDefaultAsync(ml => ml.Id == id, cancellationToken);

        if (managedList == null)
        {
            return TypedResults.NotFound($"Managed list with ID '{id}' not found.");
        }

        // Order items by SortOrder if provided, otherwise alphabetically by Label (Name)
        var items = managedList.Items
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.Label)
            .Select(i => new ManagedListItemDto(i.Id, i.Code, i.Label, i.SortOrder, i.IsActive, i.Metadata))
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
