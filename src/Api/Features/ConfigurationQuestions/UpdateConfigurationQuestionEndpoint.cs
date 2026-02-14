using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.ConfigurationQuestions;

public static class UpdateConfigurationQuestionEndpoint
{
    public static void MapUpdateConfigurationQuestionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/configuration-questions/{id:guid}", HandleAsync)
            .WithName("UpdateConfigurationQuestion")
            .WithSummary("Update Configuration Question")
            .WithTags("Configuration Questions");
    }

    public static async Task<Results<Ok<UpdateConfigurationQuestionResponse>, NotFound, ValidationProblem>> HandleAsync(
        Guid id,
        UpdateConfigurationQuestionRequest request,
        ApplicationDbContext db,
        IValidator<UpdateConfigurationQuestionRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var question = await db.ConfigurationQuestions
            .Where(q => q.IsActive)
            .Where(q => q.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (question == null)
        {
            return TypedResults.NotFound();
        }

        question.Question = request.Question;
        question.AiPrompt = request.AiPrompt;
        question.RuleType = request.RuleType;
        question.ModifiedOn = DateTime.UtcNow;
        question.ModifiedBy = "System"; // TODO: Replace with real user when auth is available
        question.Version += 1;

        await db.SaveChangesAsync(cancellationToken);

        var response = new UpdateConfigurationQuestionResponse(
            question.Id,
            question.Question,
            question.AiPrompt,
            question.RuleType,
            question.Version
        );

        return TypedResults.Ok(response);
    }
}
