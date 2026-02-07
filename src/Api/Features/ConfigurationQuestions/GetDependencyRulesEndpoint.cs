using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.ConfigurationQuestions;

public static class GetDependencyRulesEndpoint
{
    public static void MapGetDependencyRulesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/dependency-rules", HandleAsync)
            .WithName("GetDependencyRules")
            .WithSummary("Get Dependency Rules")
            .WithTags("Dependency Rules");
    }

    public static async Task<List<GetDependencyRulesResponse>> HandleAsync(
        Guid? questionId,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var rulesQuery = db.DependencyRules
            .Include(dr => dr.ConfigurationQuestion)
            .Include(dr => dr.TriggeringAnswer)
            .AsNoTracking();

        if (questionId.HasValue)
        {
            rulesQuery = rulesQuery.Where(dr => dr.ConfigurationQuestionId == questionId.Value);
        }

        return await rulesQuery
            .Select(dr => new GetDependencyRulesResponse(
                dr.Id,
                dr.Name,
                dr.ConfigurationQuestionId,
                dr.ConfigurationQuestion!.Question,
                dr.TriggeringAnswerId,
                dr.TriggeringAnswer != null ? dr.TriggeringAnswer.Name : null,
                dr.Classification,
                dr.Type,
                dr.ContentType,
                dr.Module,
                dr.QuestionBank,
                dr.Tag,
                dr.StatusReason,
                dr.IsActive
            ))
            .ToListAsync(cancellationToken);
    }
}
