using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.ConfigurationQuestions;

public static class UpdateConfigurationAnswerEndpoint
{
    public static void MapUpdateConfigurationAnswerEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/configuration-answers/{id:guid}", HandleAsync)
            .WithName("UpdateConfigurationAnswer")
            .WithSummary("Update Configuration Answer")
            .WithTags("Configuration Answers");
    }

    public static async Task<Results<Ok<UpdateConfigurationAnswerResponse>, NotFound, ValidationProblem>> HandleAsync(
        Guid id,
        UpdateConfigurationAnswerRequest request,
        ApplicationDbContext db,
        IValidator<UpdateConfigurationAnswerRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var answer = await db.ConfigurationAnswers.FindAsync([id], cancellationToken);

        if (answer == null)
        {
            return TypedResults.NotFound();
        }

        answer.Name = request.Name;
        answer.IsActive = request.IsActive;
        answer.ModifiedOn = DateTime.UtcNow;
        answer.ModifiedBy = "System"; // TODO: Replace with real user when auth is available

        await db.SaveChangesAsync(cancellationToken);

        var response = new UpdateConfigurationAnswerResponse(
            answer.Id,
            answer.Name,
            answer.ConfigurationQuestionId,
            answer.IsActive
        );

        return TypedResults.Ok(response);
    }
}
