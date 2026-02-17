using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.ProjectQuestionnaires;

public static class GetProjectQuestionnairesEndpoint
{
    public static void MapGetProjectQuestionnairesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/projects/{projectId:guid}/questionnaires", async Task<Results<Ok<List<ProjectQuestionnaireDto>>, NotFound>> (
            Guid projectId,
            ApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            // Check if project exists
            var projectExists = await context.Projects.AnyAsync(p => p.Id == projectId, cancellationToken);
            if (!projectExists)
            {
                return TypedResults.NotFound();
            }

            var questionnaires = await context.Set<ProjectQuestionnaire>()
                .Include(pq => pq.QuestionBankItem)
                .Where(pq => pq.ProjectId == projectId)
                .OrderBy(pq => pq.SortOrder)
                .Select(pq => new ProjectQuestionnaireDto(
                    pq.Id,
                    pq.ProjectId,
                    pq.QuestionBankItemId,
                    pq.SortOrder,
                    new QuestionBankItemSummary(
                        pq.QuestionBankItem.Id,
                        pq.QuestionBankItem.VariableName,
                        pq.QuestionBankItem.Version,
                        pq.QuestionBankItem.QuestionText,
                        pq.QuestionBankItem.QuestionType,
                        pq.QuestionBankItem.Classification,
                        pq.QuestionBankItem.QuestionRationale
                    )
                ))
                .ToListAsync(cancellationToken);

            return TypedResults.Ok(questionnaires);
        })
        .WithName("GetProjectQuestionnaires")
        .WithTags("ProjectQuestionnaires");
    }
}
