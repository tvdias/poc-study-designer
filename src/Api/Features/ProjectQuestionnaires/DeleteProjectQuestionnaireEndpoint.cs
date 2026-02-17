using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.ProjectQuestionnaires;

public static class DeleteProjectQuestionnaireEndpoint
{
    public static void MapDeleteProjectQuestionnaireEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/projects/{projectId:guid}/questionnaires/{id:guid}", async Task<Results<NoContent, NotFound>> (
            Guid projectId,
            Guid id,
            ApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var questionnaire = await context.Set<ProjectQuestionnaire>()
                .FirstOrDefaultAsync(pq => pq.Id == id && pq.ProjectId == projectId, cancellationToken);

            if (questionnaire == null)
            {
                return TypedResults.NotFound();
            }

            context.Set<ProjectQuestionnaire>().Remove(questionnaire);
            await context.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        })
        .WithName("DeleteProjectQuestionnaire")
        .WithTags("ProjectQuestionnaires");
    }
}
