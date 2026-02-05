using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Modules;

public static class RemoveQuestionFromModuleEndpoint
{
    public static void MapRemoveQuestionFromModuleEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/modules/{moduleId}/questions/{questionId}", HandleAsync)
            .WithName("RemoveQuestionFromModule")
            .WithSummary("Remove Question from Module")
            .WithTags("Modules");
    }

    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        Guid moduleId,
        Guid questionId,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var moduleQuestion = await db.ModuleQuestions
            .FirstOrDefaultAsync(mq => mq.ModuleId == moduleId && mq.QuestionId == questionId, cancellationToken);

        if (moduleQuestion is null)
        {
            return TypedResults.NotFound();
        }

        db.ModuleQuestions.Remove(moduleQuestion);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
