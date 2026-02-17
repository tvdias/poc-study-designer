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
                .Where(pq => pq.ProjectId == projectId)
                .OrderBy(pq => pq.SortOrder)
                .Select(pq => new ProjectQuestionnaireDto(
                    pq.Id,
                    pq.ProjectId,
                    pq.QuestionBankItemId,
                    pq.SortOrder,
                    pq.VariableName,
                    pq.Version,
                    pq.QuestionText,
                    pq.QuestionTitle,
                    pq.QuestionType,
                    pq.Classification,
                    pq.QuestionRationale,
                    pq.ScraperNotes,
                    pq.CustomNotes,
                    pq.RowSortOrder,
                    pq.ColumnSortOrder,
                    pq.AnswerMin,
                    pq.AnswerMax,
                    pq.QuestionFormatDetails,
                    pq.IsDummy
                ))
                .ToListAsync(cancellationToken);

            return TypedResults.Ok(questionnaires);
        })
        .WithName("GetProjectQuestionnaires")
        .WithTags("ProjectQuestionnaires");
    }
}
