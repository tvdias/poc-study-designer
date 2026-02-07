using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.ConfigurationQuestions;

public static class GetConfigurationQuestionsEndpoint
{
    public static void MapGetConfigurationQuestionsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/configuration-questions", HandleAsync)
            .WithName("GetConfigurationQuestions")
            .WithSummary("Get Configuration Questions")
            .WithTags("Configuration Questions");
    }

    public static async Task<List<GetConfigurationQuestionsResponse>> HandleAsync(
        string? query,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var questionsQuery = db.ConfigurationQuestions
            .Include(q => q.Answers)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            questionsQuery = questionsQuery.Where(q => EF.Functions.ILike(q.Question, pattern));
        }

        return await questionsQuery
            .Select(q => new GetConfigurationQuestionsResponse(
                q.Id,
                q.Question,
                q.AiPrompt,
                q.RuleType,
                q.IsActive,
                q.Version,
                q.Answers.Count
            ))
            .ToListAsync(cancellationToken);
    }
}
