using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
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

    public static async Task<Results<Ok<UpdateManagedListItemResponse>, ValidationProblem, NotFound<string>, Conflict<string>>> HandleAsync(
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

        // Check for duplicate Value (Code) within the same ManagedList (case-insensitive)
        // Exclude the current item from the duplicate check
        var duplicateExists = await db.ManagedListItems
            .AnyAsync(i => i.ManagedListId == managedListId && 
                          i.Id != itemId &&
                          i.Value.ToLower() == request.Value.ToLower(), 
                      cancellationToken);
        
        if (duplicateExists)
        {
            return TypedResults.Conflict($"Another item with code '{request.Value}' already exists in this managed list.");
        }

        item.Value = request.Value;
        item.Label = request.Label;
        item.SortOrder = request.SortOrder;
        item.IsActive = request.IsActive;
        item.Metadata = request.Metadata;
        item.ModifiedOn = DateTime.UtcNow;
        item.ModifiedBy = "System"; // TODO: Replace with real user when auth is available

        await db.SaveChangesAsync(cancellationToken);

        var response = new UpdateManagedListItemResponse(
            item.Id,
            item.ManagedListId,
            item.Value,
            item.Label,
            item.SortOrder,
            item.IsActive,
            item.Metadata);

        return TypedResults.Ok(response);
    }
}
