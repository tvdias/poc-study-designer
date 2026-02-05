using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Questions;

public static class GetQuestionsEndpoint
{
    public static void MapGetQuestionsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/questions", HandleAsync)
            .WithName("GetQuestions")
            .WithSummary("Get Questions")
            .WithTags("Questions");
    }

    public static async Task<List<GetQuestionsResponse>> HandleAsync(
        string? query,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var questionsQuery = db.Questions.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            questionsQuery = questionsQuery.Where(q => 
                EF.Functions.ILike(q.VariableName, pattern) ||
                EF.Functions.ILike(q.QuestionText, pattern));
        }

        return await questionsQuery
            .Select(q => new GetQuestionsResponse(
                q.Id,
                q.VariableName,
                q.QuestionType,
                q.QuestionText,
                q.QuestionSource,
                q.IsActive))
            .ToListAsync(cancellationToken);
    }
}
