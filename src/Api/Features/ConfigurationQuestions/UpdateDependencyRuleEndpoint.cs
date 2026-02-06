using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.ConfigurationQuestions;

public static class UpdateDependencyRuleEndpoint
{
    public static void MapUpdateDependencyRuleEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/dependency-rules/{id:guid}", HandleAsync)
            .WithName("UpdateDependencyRule")
            .WithSummary("Update Dependency Rule")
            .WithTags("Dependency Rules");
    }

    public static async Task<Results<Ok<UpdateDependencyRuleResponse>, NotFound, ValidationProblem, NotFound<string>>> HandleAsync(
        Guid id,
        UpdateDependencyRuleRequest request,
        ApplicationDbContext db,
        IValidator<UpdateDependencyRuleRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var rule = await db.DependencyRules.FindAsync([id], cancellationToken);

        if (rule == null)
        {
            return TypedResults.NotFound();
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

        rule.Name = request.Name;
        rule.TriggeringAnswerId = request.TriggeringAnswerId;
        rule.Classification = request.Classification;
        rule.Type = request.Type;
        rule.ContentType = request.ContentType;
        rule.Module = request.Module;
        rule.QuestionBank = request.QuestionBank;
        rule.Tag = request.Tag;
        rule.StatusReason = request.StatusReason;
        rule.IsActive = request.IsActive;
        rule.ModifiedOn = DateTime.UtcNow;
        rule.ModifiedBy = "System"; // TODO: Replace with real user when auth is available

        await db.SaveChangesAsync(cancellationToken);

        var response = new UpdateDependencyRuleResponse(
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

        return TypedResults.Ok(response);
    }
}
