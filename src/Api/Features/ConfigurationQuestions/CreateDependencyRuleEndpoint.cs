using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.ConfigurationQuestions;

public static class CreateDependencyRuleEndpoint
{
    public static void MapCreateDependencyRuleEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/dependency-rules", HandleAsync)
            .WithName("CreateDependencyRule")
            .WithSummary("Create Dependency Rule")
            .WithTags("Dependency Rules");
    }

    public static async Task<Results<CreatedAtRoute<CreateDependencyRuleResponse>, ValidationProblem, NotFound<string>>> HandleAsync(
        CreateDependencyRuleRequest request,
        ApplicationDbContext db,
        IValidator<CreateDependencyRuleRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        // Check if the configuration question exists
        var questionExists = await db.ConfigurationQuestions
            .AnyAsync(q => q.Id == request.ConfigurationQuestionId, cancellationToken);

        if (!questionExists)
        {
            return TypedResults.NotFound($"Configuration Question with ID {request.ConfigurationQuestionId} not found.");
        }

        // Check if the triggering answer exists (if provided)
        if (request.TriggeringAnswerId.HasValue)
        {
            var answerExists = await db.ConfigurationAnswers
                .AnyAsync(a => a.Id == request.TriggeringAnswerId.Value, cancellationToken);

            if (!answerExists)
            {
                return TypedResults.NotFound($"Configuration Answer with ID {request.TriggeringAnswerId.Value} not found.");
            }
        }

        var rule = new DependencyRule
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ConfigurationQuestionId = request.ConfigurationQuestionId,
            TriggeringAnswerId = request.TriggeringAnswerId,
            Classification = request.Classification,
            Type = request.Type,
            ContentType = request.ContentType,
            Module = request.Module,
            QuestionBank = request.QuestionBank,
            Tag = request.Tag,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System" // TODO: Replace with real user when auth is available
        };

        db.DependencyRules.Add(rule);
        await db.SaveChangesAsync(cancellationToken);

        var response = new CreateDependencyRuleResponse(
            rule.Id,
            rule.Name,
            rule.ConfigurationQuestionId,
            rule.TriggeringAnswerId,
            rule.Classification,
            rule.Type,
            rule.ContentType,
            rule.Module,
            rule.QuestionBank,
            rule.Tag,
            rule.StatusReason,
            rule.IsActive
        );

        return TypedResults.CreatedAtRoute(response, "GetDependencyRuleById", new { id = rule.Id });
    }
}
