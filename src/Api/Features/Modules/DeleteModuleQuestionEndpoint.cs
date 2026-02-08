using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Modules;

public static class DeleteModuleQuestionEndpoint
{
    public static void MapDeleteModuleQuestionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/module-questions/{id:guid}", HandleAsync)
            .WithName("DeleteModuleQuestion")
            .WithSummary("Delete Module Question")
            .WithTags("ModuleQuestions");
    }

    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var moduleQuestion = await db.ModuleQuestions.FindAsync([id], cancellationToken);
        if (moduleQuestion is null)
        {
            return TypedResults.NotFound();
        }

        db.ModuleQuestions.Remove(moduleQuestion);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
