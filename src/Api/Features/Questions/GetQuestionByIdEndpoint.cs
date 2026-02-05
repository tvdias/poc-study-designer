using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Questions;

public static class GetQuestionByIdEndpoint
{
    public static void MapGetQuestionByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/questions/{id}", HandleAsync)
            .WithName("GetQuestionById")
            .WithSummary("Get Question By Id")
            .WithTags("Questions");
    }

    public static async Task<Results<Ok<GetQuestionsResponse>, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var question = await db.Questions
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (question is null)
        {
            return TypedResults.NotFound();
        }

        var response = new GetQuestionsResponse(
            question.Id,
            question.VariableName,
            question.QuestionType,
            question.QuestionText,
            question.QuestionSource,
            question.IsActive
        );

        return TypedResults.Ok(response);
    }
}
