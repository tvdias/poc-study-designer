using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.ManagedLists;

public static class CreateManagedListEndpoint
{
    public static void MapCreateManagedListEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/managedlists", HandleAsync)
            .WithName("CreateManagedList")
            .WithSummary("Create Managed List")
            .WithTags("ManagedLists");
    }

    public static async Task<Results<CreatedAtRoute<CreateManagedListResponse>, ValidationProblem, Conflict<string>, NotFound<string>>> HandleAsync(
        CreateManagedListRequest request,
        ApplicationDbContext db,
        IValidator<CreateManagedListRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        // Check if project exists
        var projectExists = await db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            return TypedResults.NotFound($"Project with ID '{request.ProjectId}' not found.");
        }

        // Check for duplicate name within the same project
        var duplicateExists = await db.ManagedLists
            .AnyAsync(ml => ml.ProjectId == request.ProjectId && ml.Name == request.Name, cancellationToken);
        
        if (duplicateExists)
        {
            return TypedResults.Conflict($"A managed list with name '{request.Name}' already exists in this project.");
        }

        var managedList = new ManagedList
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Name = request.Name,
            Description = request.Description,
            Status = ManagedListStatus.Active,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System" // TODO: Replace with real user when auth is available
        };

        db.ManagedLists.Add(managedList);

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

        var response = new CreateManagedListResponse(
            managedList.Id,
            managedList.ProjectId,
            managedList.Name,
            managedList.Description,
            managedList.Status);

        return TypedResults.CreatedAtRoute(response, "GetManagedListById", new { id = managedList.Id });
    }
}
