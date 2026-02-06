using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.ConfigurationQuestions;

public static class GetConfigurationAnswersEndpoint
{
    public static void MapGetConfigurationAnswersEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/configuration-answers", HandleAsync)
            .WithName("GetConfigurationAnswers")
            .WithSummary("Get Configuration Answers")
            .WithTags("Configuration Answers");
    }

    public static async Task<List<GetConfigurationAnswersResponse>> HandleAsync(
        Guid? questionId,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var answersQuery = db.ConfigurationAnswers
            .Include(a => a.ConfigurationQuestion)
            .AsNoTracking();

        if (questionId.HasValue)
        {
            answersQuery = answersQuery.Where(a => a.ConfigurationQuestionId == questionId.Value);
        }

        return await answersQuery
            .Select(a => new GetConfigurationAnswersResponse(
                a.Id,
                a.Name,
                a.ConfigurationQuestionId,
                a.ConfigurationQuestion!.Question,
                a.IsActive
            ))
            .ToListAsync(cancellationToken);
    }
}
