using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.Modules;

public static class AddQuestionToModuleEndpoint
{
    public static void MapAddQuestionToModuleEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/modules/{moduleId}/questions", HandleAsync)
            .WithName("AddQuestionToModule")
            .WithSummary("Add Question to Module")
            .WithTags("Modules");
    }

    public static async Task<Results<CreatedAtRoute<AddQuestionToModuleResponse>, ValidationProblem, NotFound, Conflict<string>>> HandleAsync(
        Guid moduleId,
        AddQuestionToModuleRequest request,
        ApplicationDbContext db,
        IValidator<AddQuestionToModuleRequest> validator,
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

        // Verify question exists
        var questionExists = await db.Questions.AnyAsync(q => q.Id == request.QuestionId, cancellationToken);
        if (!questionExists)
        {
            return TypedResults.Conflict("Question does not exist.");
        }

        // Check if question is already in module
        var exists = await db.ModuleQuestions.AnyAsync(
            mq => mq.ModuleId == moduleId && mq.QuestionId == request.QuestionId,
            cancellationToken);

        if (exists)
        {
            return TypedResults.Conflict("Question is already assigned to this module.");
        }

        var moduleQuestion = new ModuleQuestion
        {
            Id = Guid.NewGuid(),
            ModuleId = moduleId,
            QuestionId = request.QuestionId,
            DisplayOrder = request.DisplayOrder,
            CreatedOn = DateTime.UtcNow
        };

        db.ModuleQuestions.Add(moduleQuestion);
        await db.SaveChangesAsync(cancellationToken);

        var response = new AddQuestionToModuleResponse(
            moduleQuestion.Id,
            moduleQuestion.ModuleId,
            moduleQuestion.QuestionId,
            moduleQuestion.DisplayOrder
        );

        return TypedResults.CreatedAtRoute(response, "GetModuleById", new { id = moduleId });
    }
}
