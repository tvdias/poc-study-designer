using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Modules;

public static class DeleteModuleEndpoint
{
    public static void MapDeleteModuleEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/modules/{id}", HandleAsync)
            .WithName("DeleteModule")
            .WithSummary("Delete Module")
            .WithTags("Modules");
    }

    public static async Task<Results<NoContent, NotFound, Conflict<string>>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var module = await db.Modules
            .Include(m => m.ChildModules)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (module is null)
        {
            return TypedResults.NotFound();
        }

        // Check if module has child modules
        if (module.ChildModules.Any())
        {
            return TypedResults.Conflict("Cannot delete module with child modules. Remove child modules first.");
        }

        db.Modules.Remove(module);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
