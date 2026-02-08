using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.QuestionBank;

public static class DeleteQuestionAnswerEndpoint
{
    public static void MapDeleteQuestionAnswerEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/question-bank/{questionId:guid}/answers/{answerId:guid}", HandleAsync)
            .WithName("DeleteQuestionAnswer")
            .WithSummary("Delete Question Answer")
            .WithTags("QuestionBank");
    }

    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        Guid questionId,
        Guid answerId,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var answer = await db.QuestionAnswers
            .FirstOrDefaultAsync(a => a.Id == answerId && a.QuestionBankItemId == questionId, cancellationToken);
        
        if (answer == null)
        {
            return TypedResults.NotFound();
        }

        db.QuestionAnswers.Remove(answer);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
