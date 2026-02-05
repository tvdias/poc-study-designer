using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.Modules;

public static class CreateModuleEndpoint
{
    public static void MapCreateModuleEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/modules", HandleAsync)
            .WithName("CreateModule")
            .WithSummary("Create Module")
            .WithTags("Modules");
    }

    public static async Task<Results<CreatedAtRoute<CreateModuleResponse>, ValidationProblem, Conflict<string>>> HandleAsync(
        CreateModuleRequest request,
        ApplicationDbContext db,
        IValidator<CreateModuleRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        // Validate parent module exists if provided
        if (request.ParentModuleId.HasValue)
        {
            var parentExists = await db.Modules.AnyAsync(m => m.Id == request.ParentModuleId.Value, cancellationToken);
            if (!parentExists)
            {
                return TypedResults.Conflict("Parent module does not exist.");
            }
        }

        var module = new Module
        {
            Id = Guid.NewGuid(),
            VariableName = request.VariableName,
            Label = request.Label,
            Description = request.Description,
            VersionNumber = 1,
            ParentModuleId = request.ParentModuleId,
            Instructions = request.Instructions,
            Status = request.Status,
            StatusReason = request.StatusReason,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System" // TODO: Replace with real user when auth is available
        };

        db.Modules.Add(module);

        // Create initial version record
        var version = new ModuleVersion
        {
            Id = Guid.NewGuid(),
            ModuleId = module.Id,
            VersionNumber = 1,
            ChangeDescription = "Initial version",
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System"
        };
        db.ModuleVersions.Add(version);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
                || ex.InnerException?.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) == true)
            {
                return TypedResults.Conflict($"Module with variable name '{request.VariableName}' already exists.");
            }

            throw;
        }

        var response = new CreateModuleResponse(
            module.Id,
            module.VariableName,
            module.Label,
            module.Description,
            module.VersionNumber,
            module.ParentModuleId,
            module.Instructions,
            module.Status,
            module.StatusReason
        );

        return TypedResults.CreatedAtRoute(response, "GetModuleById", new { id = module.Id });
    }
}
