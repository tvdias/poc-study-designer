using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.ConfigurationQuestions;

public static class GetConfigurationAnswerByIdEndpoint
{
    public static void MapGetConfigurationAnswerByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/configuration-answers/{id:guid}", HandleAsync)
            .WithName("GetConfigurationAnswerById")
            .WithSummary("Get Configuration Answer by ID")
            .WithTags("Configuration Answers");
    }

    public static async Task<Results<Ok<GetConfigurationAnswersResponse>, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var answer = await db.ConfigurationAnswers
            .Include(a => a.ConfigurationQuestion)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (answer == null)
        {
            return TypedResults.NotFound();
        }

        var response = new GetConfigurationAnswersResponse(
            answer.Id,
            answer.Name,
            answer.ConfigurationQuestionId,
            answer.ConfigurationQuestion!.Question,
            answer.IsActive
        );

        return TypedResults.Ok(response);
    }
}
