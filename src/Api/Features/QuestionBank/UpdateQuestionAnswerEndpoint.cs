using Api.Data;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.QuestionBank;

public static class UpdateQuestionAnswerEndpoint
{
    public static void MapUpdateQuestionAnswerEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/question-bank/{questionId:guid}/answers/{answerId:guid}", HandleAsync)
            .WithName("UpdateQuestionAnswer")
            .WithSummary("Update Question Answer")
            .WithTags("QuestionBank");
    }

    public static async Task<Results<Ok<UpdateQuestionAnswerResponse>, ValidationProblem, NotFound, Conflict<string>>> HandleAsync(
        Guid questionId,
        Guid answerId,
        UpdateQuestionAnswerRequest request,
        ApplicationDbContext db,
        IValidator<UpdateQuestionAnswerRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var answer = await db.QuestionAnswers
            .FirstOrDefaultAsync(a => a.Id == answerId && a.QuestionBankItemId == questionId, cancellationToken);
        
        if (answer == null)
        {
            return TypedResults.NotFound();
        }

        answer.AnswerText = request.AnswerText;
        answer.AnswerCode = request.AnswerCode;
        answer.AnswerLocation = request.AnswerLocation;
        answer.IsOpen = request.IsOpen;
        answer.IsFixed = request.IsFixed;
        answer.IsExclusive = request.IsExclusive;
        answer.IsActive = request.IsActive;
        answer.CustomProperty = request.CustomProperty;
        answer.Facets = request.Facets;
        answer.Version = request.Version;
        answer.DisplayOrder = request.DisplayOrder;
        answer.ModifiedOn = DateTime.UtcNow;
        answer.ModifiedBy = "System";

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

        return TypedResults.Ok(new UpdateQuestionAnswerResponse(answer.Id, answer.AnswerText));
    }
}
