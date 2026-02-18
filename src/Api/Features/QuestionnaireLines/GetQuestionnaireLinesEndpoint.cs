using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.QuestionnaireLines;

public static class GetQuestionnaireLinesEndpoint
{
    public static void MapGetQuestionnaireLinesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/projects/{projectId:guid}/questionnairelines", async Task<Results<Ok<List<QuestionnaireLineDto>>, NotFound>> (
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

            var questionnaires = await context.Set<QuestionnaireLine>()
                .Where(pq => pq.ProjectId == projectId)
                .OrderBy(pq => pq.SortOrder)
                .Select(pq => new QuestionnaireLineDto(
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
        .WithName("GetQuestionnaireLines")
        .WithTags("QuestionnaireLines");
    }
}
