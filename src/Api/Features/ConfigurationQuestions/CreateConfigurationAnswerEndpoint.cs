using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.ConfigurationQuestions;

public static class CreateConfigurationAnswerEndpoint
{
    public static void MapCreateConfigurationAnswerEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/configuration-answers", HandleAsync)
            .WithName("CreateConfigurationAnswer")
            .WithSummary("Create Configuration Answer")
            .WithTags("Configuration Answers");
    }

    public static async Task<Results<CreatedAtRoute<CreateConfigurationAnswerResponse>, ValidationProblem, NotFound<string>>> HandleAsync(
        CreateConfigurationAnswerRequest request,
        ApplicationDbContext db,
        IValidator<CreateConfigurationAnswerRequest> validator,
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

        var answer = new ConfigurationAnswer
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ConfigurationQuestionId = request.ConfigurationQuestionId,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System" // TODO: Replace with real user when auth is available
        };

        db.ConfigurationAnswers.Add(answer);
        await db.SaveChangesAsync(cancellationToken);

        var response = new CreateConfigurationAnswerResponse(
            answer.Id,
            answer.Name,
            answer.ConfigurationQuestionId,
            answer.IsActive
        );

        return TypedResults.CreatedAtRoute(response, "GetConfigurationAnswerById", new { id = answer.Id });
    }
}
