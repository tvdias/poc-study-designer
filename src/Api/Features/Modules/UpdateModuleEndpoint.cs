using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.Modules;

public static class UpdateModuleEndpoint
{
    public static void MapUpdateModuleEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/modules/{id}", HandleAsync)
            .WithName("UpdateModule")
            .WithSummary("Update Module")
            .WithTags("Modules");
    }

    public static async Task<Results<Ok<UpdateModuleResponse>, NotFound, ValidationProblem, Conflict<string>>> HandleAsync(
        Guid id,
        UpdateModuleRequest request,
        ApplicationDbContext db,
        IValidator<UpdateModuleRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var module = await db.Modules.FindAsync([id], cancellationToken);

        if (module is null)
        {
            return TypedResults.NotFound();
        }

        // Validate parent module exists if provided and prevent circular reference
        if (request.ParentModuleId.HasValue)
        {
            if (request.ParentModuleId.Value == id)
            {
                return TypedResults.Conflict("A module cannot be its own parent.");
            }

            var parentExists = await db.Modules.AnyAsync(m => m.Id == request.ParentModuleId.Value, cancellationToken);
            if (!parentExists)
            {
                return TypedResults.Conflict("Parent module does not exist.");
            }
        }

        module.VariableName = request.VariableName;
        module.Label = request.Label;
        module.Description = request.Description;
        module.ParentModuleId = request.ParentModuleId;
        module.Instructions = request.Instructions;
        module.Status = request.Status;
        module.StatusReason = request.StatusReason;
        module.IsActive = request.IsActive;
        module.ModifiedOn = DateTime.UtcNow;
        module.ModifiedBy = "System"; // TODO: Replace with real user

        await db.SaveChangesAsync(cancellationToken);

        var response = new UpdateModuleResponse(
            module.Id,
            module.VariableName,
            module.Label,
            module.Description,
            module.VersionNumber,
            module.ParentModuleId,
            module.Instructions,
            module.Status,
            module.StatusReason,
            module.IsActive
        );

        return TypedResults.Ok(response);
    }
}
