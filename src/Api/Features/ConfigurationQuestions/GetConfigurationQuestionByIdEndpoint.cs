using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.ConfigurationQuestions;

public static class GetConfigurationQuestionByIdEndpoint
{
    public static void MapGetConfigurationQuestionByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/configuration-questions/{id:guid}", HandleAsync)
            .WithName("GetConfigurationQuestionById")
            .WithSummary("Get Configuration Question by ID")
            .WithTags("Configuration Questions");
    }

    public static async Task<Results<Ok<GetConfigurationQuestionByIdResponse>, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var question = await db.ConfigurationQuestions
            .Include(q => q.Answers)
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (question == null)
        {
            return TypedResults.NotFound();
        }

        // Get dependency rules for this question
        var dependencyRules = await db.DependencyRules
            .Include(dr => dr.TriggeringAnswer)
            .Where(dr => dr.ConfigurationQuestionId == id)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var response = new GetConfigurationQuestionByIdResponse(
            question.Id,
            question.Question,
            question.AiPrompt,
            question.RuleType,
            question.Version,
            question.Answers.Select(a => new ConfigurationAnswerDto(
                a.Id,
                a.Name,
                a.CreatedOn,
                a.CreatedBy
            )).ToList(),
            dependencyRules.Select(dr => new DependencyRuleDto(
                dr.Id,
                dr.Name,
                dr.TriggeringAnswerId,
                dr.TriggeringAnswer?.Name,
                dr.Classification,
                dr.Type,
                dr.ContentType,
                dr.Module,
                dr.QuestionBank,
                dr.Tag,
                dr.StatusReason,
                dr.CreatedOn,
                dr.CreatedBy
            )).ToList()
        );

        return TypedResults.Ok(response);
    }
}
