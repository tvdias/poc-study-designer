using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.Modules;

public static class ReorderModuleQuestionsEndpoint
{
    public static void MapReorderModuleQuestionsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/modules/{moduleId}/questions/reorder", HandleAsync)
            .WithName("ReorderModuleQuestions")
            .WithSummary("Reorder Questions in Module")
            .WithTags("Modules");
    }

    public static async Task<Results<Ok, ValidationProblem, NotFound>> HandleAsync(
        Guid moduleId,
        ReorderModuleQuestionsRequest request,
        ApplicationDbContext db,
        IValidator<ReorderModuleQuestionsRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        // Verify module exists
        var moduleExists = await db.Modules.AnyAsync(m => m.Id == moduleId, cancellationToken);
        if (!moduleExists)
        {
            return TypedResults.NotFound();
        }

        // Get all module questions for this module
        var moduleQuestions = await db.ModuleQuestions
            .Where(mq => mq.ModuleId == moduleId)
            .ToListAsync(cancellationToken);

        // Update display order for each question
        foreach (var orderDto in request.Questions)
        {
            var moduleQuestion = moduleQuestions.FirstOrDefault(mq => mq.QuestionId == orderDto.QuestionId);
            if (moduleQuestion != null)
            {
                moduleQuestion.DisplayOrder = orderDto.DisplayOrder;
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok();
    }
}
