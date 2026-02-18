using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.ManagedLists;

public static class CreateManagedListItemEndpoint
{
    public static void MapCreateManagedListItemEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/managedlists/{managedListId:guid}/items", HandleAsync)
            .WithName("CreateManagedListItem")
            .WithSummary("Create Managed List Item")
            .WithTags("ManagedLists");
    }

    public static async Task<Results<CreatedAtRoute<CreateManagedListItemResponse>, ValidationProblem, NotFound<string>>> HandleAsync(
        Guid managedListId,
        CreateManagedListItemRequest request,
        ApplicationDbContext db,
        IValidator<CreateManagedListItemRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        // Check if managed list exists
        var managedListExists = await db.ManagedLists.AnyAsync(ml => ml.Id == managedListId, cancellationToken);
        if (!managedListExists)
        {
            return TypedResults.NotFound($"Managed list with ID '{managedListId}' not found.");
        }

        var item = new ManagedListItem
        {
            Id = Guid.NewGuid(),
            ManagedListId = managedListId,
            Value = request.Value,
            Label = request.Label,
            SortOrder = request.SortOrder,
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System" // TODO: Replace with real user when auth is available
        };

        db.ManagedListItems.Add(item);
        await db.SaveChangesAsync(cancellationToken);

        var response = new CreateManagedListItemResponse(
            item.Id,
            item.ManagedListId,
            item.Value,
            item.Label,
            item.SortOrder,
            item.IsActive);

        return TypedResults.CreatedAtRoute(response, "GetManagedListById", new { id = managedListId });
    }
}
