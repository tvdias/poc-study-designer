using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.ManagedLists;

public static class UpdateManagedListEndpoint
{
    public static void MapUpdateManagedListEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/managedlists/{id:guid}", HandleAsync)
            .WithName("UpdateManagedList")
            .WithSummary("Update Managed List")
            .WithTags("ManagedLists");
    }

    public static async Task<Results<Ok<UpdateManagedListResponse>, ValidationProblem, NotFound<string>, Conflict<string>>> HandleAsync(
        Guid id,
        UpdateManagedListRequest request,
        ApplicationDbContext db,
        IValidator<UpdateManagedListRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var managedList = await db.ManagedLists.FindAsync(new object[] { id }, cancellationToken);
        if (managedList == null)
        {
            return TypedResults.NotFound($"Managed list with ID '{id}' not found.");
        }

        // Check for duplicate name within the same project (excluding current)
        var duplicateExists = await db.ManagedLists
            .AnyAsync(ml => ml.ProjectId == managedList.ProjectId && ml.Name == request.Name && ml.Id != id, cancellationToken);
        
        if (duplicateExists)
        {
            return TypedResults.Conflict($"A managed list with name '{request.Name}' already exists in this project.");
        }

        managedList.Name = request.Name;
        managedList.Description = request.Description;
        managedList.ModifiedOn = DateTime.UtcNow;
        managedList.ModifiedBy = "System"; // TODO: Replace with real user when auth is available

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            // Handle unique constraint violations
            if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
                || ex.InnerException?.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) == true)
            {
                return TypedResults.Conflict($"A managed list with name '{request.Name}' already exists in this project.");
            }

            throw;
        }

        var response = new UpdateManagedListResponse(
            managedList.Id,
            managedList.ProjectId,
            managedList.Name,
            managedList.Description,
            managedList.Status);

        return TypedResults.Ok(response);
    }
}
