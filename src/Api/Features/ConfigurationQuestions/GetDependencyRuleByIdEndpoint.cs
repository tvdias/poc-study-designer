using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.ConfigurationQuestions;

public static class GetDependencyRuleByIdEndpoint
{
    public static void MapGetDependencyRuleByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/dependency-rules/{id:guid}", HandleAsync)
            .WithName("GetDependencyRuleById")
            .WithSummary("Get Dependency Rule by ID")
            .WithTags("Dependency Rules");
    }

    public static async Task<Results<Ok<GetDependencyRulesResponse>, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var rule = await db.DependencyRules
            .Include(dr => dr.ConfigurationQuestion)
            .Include(dr => dr.TriggeringAnswer)
            .AsNoTracking()
            .FirstOrDefaultAsync(dr => dr.Id == id, cancellationToken);

        if (rule == null)
        {
            return TypedResults.NotFound();
        }

        var response = new GetDependencyRulesResponse(
            rule.Id,
            rule.Name,
            rule.ConfigurationQuestionId,
            rule.ConfigurationQuestion!.Question,
            rule.TriggeringAnswerId,
            rule.TriggeringAnswer?.Name,
            rule.Classification,
            rule.Type,
            rule.ContentType,
            rule.Module,
            rule.QuestionBank,
            rule.Tag,
            rule.StatusReason,
            rule.IsActive
        );

        return TypedResults.Ok(response);
    }
}
