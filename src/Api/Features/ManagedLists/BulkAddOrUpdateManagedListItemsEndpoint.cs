using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using System.Text.RegularExpressions;

namespace Api.Features.ManagedLists;

public static class BulkAddOrUpdateManagedListItemsEndpoint
{
    public static void MapBulkAddOrUpdateManagedListItemsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/managedlists/{managedListId:guid}/items/bulk", HandleAsync)
            .WithName("BulkAddOrUpdateManagedListItems")
            .WithSummary("Bulk Add or Update Managed List Items")
            .WithTags("ManagedLists");
    }

    public static async Task<Results<Ok<BulkOperationResult>, NotFound<string>, BadRequest<string>>> HandleAsync(
        Guid managedListId,
        BulkAddOrUpdateManagedListItemsRequest request,
        ApplicationDbContext db,
        ISubsetManagementService subsetService,
        IAutoAssociationService autoAssociationService,
        CancellationToken cancellationToken)
    {
        // Check if managed list exists
        var managedListExists = await db.ManagedLists.AnyAsync(ml => ml.Id == managedListId, cancellationToken);
        if (!managedListExists)
        {
            return TypedResults.NotFound($"Managed list with ID '{managedListId}' not found.");
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            return TypedResults.BadRequest("No items provided for bulk operation.");
        }

        var results = new List<BulkOperationRowResult>();
        int insertedCount = 0;
        int updatedCount = 0;
        int skippedCount = 0;
        int rejectedCount = 0;

        // Load existing items for this managed list (case-insensitive lookup)
        var existingItems = await db.ManagedListItems
            .Where(item => item.ManagedListId == managedListId)
            .ToListAsync(cancellationToken);

        var existingItemsDict = existingItems
            .ToDictionary(item => item.Value.ToLower(), item => item);

        // Track codes within this batch to detect duplicates
        var codesInBatch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        // Track which items were updated/inserted for auto-association and subset refresh
        var updatedItemIds = new List<Guid>();
        var insertedItemIds = new List<Guid>();

        for (int i = 0; i < request.Items.Count; i++)
        {
            var input = request.Items[i];
            var rowIndex = i + 1;

            // Validate the row
            var validationError = ValidateItem(input);
            if (validationError != null)
            {
                results.Add(new BulkOperationRowResult(rowIndex, input.Value, "rejected", validationError));
                rejectedCount++;
                continue;
            }

            // Check for duplicates within the batch
            if (codesInBatch.Contains(input.Value))
            {
                results.Add(new BulkOperationRowResult(
                    rowIndex, 
                    input.Value, 
                    "rejected", 
                    $"Duplicate code '{input.Value}' found in the import batch."));
                rejectedCount++;
                continue;
            }

            codesInBatch.Add(input.Value);

            var valueLower = input.Value.ToLower();

            // Check if item exists
            if (existingItemsDict.TryGetValue(valueLower, out var existingItem))
            {
                // Item exists - update if allowed
                if (request.AllowUpdates)
                {
                    existingItem.Label = input.Label;
                    existingItem.SortOrder = input.SortOrder;
                    existingItem.IsActive = input.IsActive;
                    existingItem.Metadata = input.Metadata;
                    existingItem.ModifiedOn = DateTime.UtcNow;
                    existingItem.ModifiedBy = "System"; // TODO: Replace with real user when auth is available

                    updatedItemIds.Add(existingItem.Id);
                    results.Add(new BulkOperationRowResult(rowIndex, input.Value, "updated", null));
                    updatedCount++;
                }
                else
                {
                    results.Add(new BulkOperationRowResult(
                        rowIndex, 
                        input.Value, 
                        "skipped", 
                        "Item already exists and updates are not allowed."));
                    skippedCount++;
                }
            }
            else
            {
                // Item doesn't exist - insert
                var newItem = new ManagedListItem
                {
                    Id = Guid.NewGuid(),
                    ManagedListId = managedListId,
                    Value = input.Value,
                    Label = input.Label,
                    SortOrder = input.SortOrder,
                    IsActive = input.IsActive,
                    Metadata = input.Metadata,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "System" // TODO: Replace with real user when auth is available
                };

                db.ManagedListItems.Add(newItem);
                insertedItemIds.Add(newItem.Id);
                results.Add(new BulkOperationRowResult(rowIndex, input.Value, "inserted", null));
                insertedCount++;
            }
        }

        // Save all changes in a single transaction
        if (insertedCount > 0 || updatedCount > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
            
            // Trigger auto-association for newly inserted items (US5 - AC-AUTO-01)
            foreach (var itemId in insertedItemIds)
            {
                await autoAssociationService.OnManagedListItemCreatedAsync(itemId, "System", cancellationToken);
            }
            
            // Trigger subset refresh for updated items (AC-SYNC-04)
            foreach (var itemId in updatedItemIds)
            {
                await subsetService.InvalidateSubsetsForItemAsync(itemId, "System", cancellationToken);
            }
        }

        var bulkResult = new BulkOperationResult(
            request.Items.Count,
            insertedCount,
            updatedCount,
            skippedCount,
            rejectedCount,
            results);

        return TypedResults.Ok(bulkResult);
    }

    private static string? ValidateItem(BulkManagedListItemInput item)
    {
        // Validate Value (Code)
        if (string.IsNullOrWhiteSpace(item.Value))
        {
            return "Item value (code) is required.";
        }

        if (item.Value.Length > 100)
        {
            return "Item value (code) must not exceed 100 characters.";
        }

        if (!Regex.IsMatch(item.Value, @"^[a-zA-Z][a-zA-Z0-9_]*$"))
        {
            return "Item value (code) must start with a letter and contain only alphanumeric characters and underscores.";
        }

        // Validate Label (Name)
        if (string.IsNullOrWhiteSpace(item.Label))
        {
            return "Item label (name) is required.";
        }

        if (item.Label.Length > 200)
        {
            return "Item label (name) must not exceed 200 characters.";
        }

        // Validate SortOrder
        if (item.SortOrder < 0)
        {
            return "Sort order must be greater than or equal to 0.";
        }

        return null;
    }
}
