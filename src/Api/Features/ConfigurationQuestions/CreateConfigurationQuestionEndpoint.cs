using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.ConfigurationQuestions;

public static class CreateConfigurationQuestionEndpoint
{
    public static void MapCreateConfigurationQuestionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/configuration-questions", HandleAsync)
            .WithName("CreateConfigurationQuestion")
            .WithSummary("Create Configuration Question")
            .WithTags("Configuration Questions");
    }

    public static async Task<Results<CreatedAtRoute<CreateConfigurationQuestionResponse>, ValidationProblem>> HandleAsync(
        CreateConfigurationQuestionRequest request,
        ApplicationDbContext db,
        IValidator<CreateConfigurationQuestionRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var question = new ConfigurationQuestion
        {
            Id = Guid.NewGuid(),
            Question = request.Question,
            AiPrompt = request.AiPrompt,
            RuleType = request.RuleType,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System" // TODO: Replace with real user when auth is available
        };

        db.ConfigurationQuestions.Add(question);
        await db.SaveChangesAsync(cancellationToken);

        var response = new CreateConfigurationQuestionResponse(
            question.Id,
            question.Question,
            question.AiPrompt,
            question.RuleType,
            question.IsActive,
            question.Version
        );

        return TypedResults.CreatedAtRoute(response, "GetConfigurationQuestionById", new { id = question.Id });
    }
}
