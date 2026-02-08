using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.QuestionBank;

public static class DeleteQuestionBankItemEndpoint
{
    public static void MapDeleteQuestionBankItemEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/question-bank/{id:guid}", HandleAsync)
            .WithName("DeleteQuestionBankItem")
            .WithSummary("Delete Question Bank Item")
            .WithTags("QuestionBank");
    }

    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var question = await db.QuestionBankItems.FindAsync(new object[] { id }, cancellationToken);
        if (question == null)
        {
            return TypedResults.NotFound();
        }

        db.QuestionBankItems.Remove(question);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
