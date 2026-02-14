using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Modules;

public static class GetModuleQuestionsEndpoint
{
    public static void MapGetModuleQuestionsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/module-questions", HandleAsync)
            .WithName("GetModuleQuestions")
            .WithSummary("Get Module Questions")
            .WithTags("ModuleQuestions");
    }

    public static async Task<Ok<List<ModuleQuestionInfo>>> HandleAsync(
        ApplicationDbContext db,
        Guid? moduleId,
        CancellationToken cancellationToken)
    {
        var query = db.ModuleQuestions
            .Include(mq => mq.QuestionBankItem)
            .AsQueryable();

        if (moduleId.HasValue)
        {
            query = query.Where(mq => mq.ModuleId == moduleId.Value);
        }

        var moduleQuestions = await query
            .OrderBy(mq => mq.DisplayOrder)
            .Select(mq => new ModuleQuestionInfo(
                mq.Id,
                mq.ModuleId,
                mq.QuestionBankItemId,
                mq.DisplayOrder,
                mq.IsActive,
                mq.CreatedOn,
                mq.QuestionBankItem != null ? mq.QuestionBankItem.VariableName : null,
                mq.QuestionBankItem != null ? mq.QuestionBankItem.QuestionText : null))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(moduleQuestions);
    }
}
