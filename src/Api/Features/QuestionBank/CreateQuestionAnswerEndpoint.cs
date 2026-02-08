using Api.Data;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.QuestionBank;

public static class CreateQuestionAnswerEndpoint
{
    public static void MapCreateQuestionAnswerEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/question-bank/{questionId:guid}/answers", HandleAsync)
            .WithName("CreateQuestionAnswer")
            .WithSummary("Create Question Answer")
            .WithTags("QuestionBank");
    }

    public static async Task<Results<CreatedAtRoute<CreateQuestionAnswerResponse>, ValidationProblem, NotFound, Conflict<string>>> HandleAsync(
        Guid questionId,
        CreateQuestionAnswerRequest request,
        ApplicationDbContext db,
        IValidator<CreateQuestionAnswerRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var question = await db.QuestionBankItems.FindAsync(new object[] { questionId }, cancellationToken);
        if (question == null)
        {
            return TypedResults.NotFound();
        }

        var answer = new QuestionAnswer
        {
            Id = Guid.NewGuid(),
            QuestionBankItemId = questionId,
            AnswerText = request.AnswerText,
            AnswerCode = request.AnswerCode,
            AnswerLocation = request.AnswerLocation,
            IsOpen = request.IsOpen,
            IsFixed = request.IsFixed,
            IsExclusive = request.IsExclusive,
            IsActive = request.IsActive,
            CustomProperty = request.CustomProperty,
            Facets = request.Facets,
            Version = request.Version,
            DisplayOrder = request.DisplayOrder,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System"
        };

        db.QuestionAnswers.Add(answer);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
                || ex.InnerException?.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) == true)
            {
                return TypedResults.Conflict($"Answer with code '{request.AnswerCode}' already exists for this question.");
            }

            throw;
        }

        return TypedResults.CreatedAtRoute(
            new CreateQuestionAnswerResponse(answer.Id, answer.AnswerText),
            "GetQuestionBankItemById",
            new { id = questionId });
    }
}
