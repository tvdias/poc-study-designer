using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using FluentValidation;

namespace Api.Features.ManagedLists;

public static class UpdateManagedListItemEndpoint
{
    public static void MapUpdateManagedListItemEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/managedlists/{managedListId:guid}/items/{itemId:guid}", HandleAsync)
            .WithName("UpdateManagedListItem")
            .WithSummary("Update Managed List Item")
            .WithTags("ManagedLists");
    }

    public static async Task<Results<Ok<UpdateManagedListItemResponse>, ValidationProblem, NotFound<string>>> HandleAsync(
        Guid managedListId,
        Guid itemId,
        UpdateManagedListItemRequest request,
        ApplicationDbContext db,
        IValidator<UpdateManagedListItemRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var item = await db.ManagedListItems.FindAsync(new object[] { itemId }, cancellationToken);
        if (item == null || item.ManagedListId != managedListId)
        {
            return TypedResults.NotFound($"Managed list item with ID '{itemId}' not found in managed list '{managedListId}'.");
        }

        item.Value = request.Value;
        item.Label = request.Label;
        item.SortOrder = request.SortOrder;
        item.IsActive = request.IsActive;
        item.ModifiedOn = DateTime.UtcNow;
        item.ModifiedBy = "System"; // TODO: Replace with real user when auth is available

        await db.SaveChangesAsync(cancellationToken);

        var response = new UpdateManagedListItemResponse(
            item.Id,
            item.ManagedListId,
            item.Value,
            item.Label,
            item.SortOrder,
            item.IsActive);

        return TypedResults.Ok(response);
    }
}
