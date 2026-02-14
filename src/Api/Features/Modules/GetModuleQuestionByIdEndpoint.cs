using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Modules;

public static class GetModuleQuestionByIdEndpoint
{
    public static void MapGetModuleQuestionByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/module-questions/{id:guid}", HandleAsync)
            .WithName("GetModuleQuestionById")
            .WithSummary("Get Module Question by ID")
            .WithTags("ModuleQuestions");
    }

    public static async Task<Results<Ok<ModuleQuestionInfo>, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var moduleQuestion = await db.ModuleQuestions
            .Include(mq => mq.QuestionBankItem)
            .Where(mq => mq.Id == id)
            .Select(mq => new ModuleQuestionInfo(
                mq.Id,
                mq.ModuleId,
                mq.QuestionBankItemId,
                mq.DisplayOrder,
                mq.IsActive,
                mq.CreatedOn,
                mq.QuestionBankItem != null ? mq.QuestionBankItem.VariableName : null,
                mq.QuestionBankItem != null ? mq.QuestionBankItem.QuestionText : null))
            .FirstOrDefaultAsync(cancellationToken);

        return moduleQuestion is not null
            ? TypedResults.Ok(moduleQuestion)
            : TypedResults.NotFound();
    }
}
