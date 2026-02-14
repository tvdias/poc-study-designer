using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Modules;

public static class DeleteModuleQuestionEndpoint
{
    public static void MapDeleteModuleQuestionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/modules/{moduleId:guid}/questions/{id:guid}", HandleAsync)
            .WithName("DeleteModuleQuestion")
            .WithSummary("Remove a question from a module")
            .WithTags("Modules");
    }

    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        Guid moduleId,
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var moduleQuestion = await db.Set<ModuleQuestion>().FindAsync(new object[] { id }, cancellationToken);
        
        if (moduleQuestion == null || moduleQuestion.ModuleId != moduleId)
        {
            return TypedResults.NotFound();
        }

        db.Set<ModuleQuestion>().Remove(moduleQuestion);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
