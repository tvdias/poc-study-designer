using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.QuestionnaireLines;

public static class DeleteQuestionnaireLineEndpoint
{
    public static void MapDeleteQuestionnaireLineEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/projects/{projectId:guid}/questionnairelines/{id:guid}", async Task<Results<NoContent, NotFound>> (
            Guid projectId,
            Guid id,
            ApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var questionnaire = await context.Set<QuestionnaireLine>()
                .FirstOrDefaultAsync(pq => pq.Id == id && pq.ProjectId == projectId, cancellationToken);

            if (questionnaire == null)
            {
                return TypedResults.NotFound();
            }

            context.Set<QuestionnaireLine>().Remove(questionnaire);
            await context.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        })
        .WithName("DeleteQuestionnaireLine")
        .WithTags("QuestionnaireLines");
    }
}
