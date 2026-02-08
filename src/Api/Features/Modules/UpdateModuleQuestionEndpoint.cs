using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.Modules;

public static class UpdateModuleQuestionEndpoint
{
    public static void MapUpdateModuleQuestionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/module-questions/{id:guid}", HandleAsync)
            .WithName("UpdateModuleQuestion")
            .WithSummary("Update Module Question")
            .WithTags("ModuleQuestions");
    }

    public static async Task<Results<Ok<ModuleQuestionInfo>, ValidationProblem, NotFound>> HandleAsync(
        Guid id,
        UpdateModuleQuestionRequest request,
        ApplicationDbContext db,
        IValidator<UpdateModuleQuestionRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var moduleQuestion = await db.ModuleQuestions.FindAsync([id], cancellationToken);
        if (moduleQuestion is null)
        {
            return TypedResults.NotFound();
        }

        moduleQuestion.SortOrder = request.SortOrder;
        moduleQuestion.IsActive = request.IsActive;

        await db.SaveChangesAsync(cancellationToken);

        var updatedQuestion = await db.ModuleQuestions
            .Include(mq => mq.QuestionBankItem)
            .Where(mq => mq.Id == id)
            .Select(mq => new ModuleQuestionInfo(
                mq.Id,
                mq.ModuleId,
                mq.QuestionBankItemId,
                mq.SortOrder,
                mq.IsActive,
                mq.CreatedOn,
                mq.QuestionBankItem != null ? mq.QuestionBankItem.VariableName : null,
                mq.QuestionBankItem != null ? mq.QuestionBankItem.QuestionText : null))
            .FirstAsync(cancellationToken);

        return TypedResults.Ok(updatedQuestion);
    }
}
