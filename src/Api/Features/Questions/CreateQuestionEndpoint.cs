using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.Questions;

public static class CreateQuestionEndpoint
{
    public static void MapCreateQuestionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/questions", HandleAsync)
            .WithName("CreateQuestion")
            .WithSummary("Create Question")
            .WithTags("Questions");
    }

    public static async Task<Results<CreatedAtRoute<CreateQuestionResponse>, ValidationProblem, Conflict<string>>> HandleAsync(
        CreateQuestionRequest request,
        ApplicationDbContext db,
        IValidator<CreateQuestionRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var question = new Question
        {
            Id = Guid.NewGuid(),
            VariableName = request.VariableName,
            QuestionType = request.QuestionType,
            QuestionText = request.QuestionText,
            QuestionSource = request.QuestionSource,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System" // TODO: Replace with real user when auth is available
        };

        db.Questions.Add(question);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
                || ex.InnerException?.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) == true)
            {
                return TypedResults.Conflict($"Question with variable name '{request.VariableName}' already exists.");
            }

            throw;
        }

        var response = new CreateQuestionResponse(
            question.Id,
            question.VariableName,
            question.QuestionType,
            question.QuestionText,
            question.QuestionSource
        );

        return TypedResults.CreatedAtRoute(response, "GetQuestionById", new { id = question.Id });
    }
}
