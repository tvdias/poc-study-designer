using Api.Data;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Modules;

public static class CreateModuleQuestionEndpoint
{
    public static void MapCreateModuleQuestionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/modules/{moduleId:guid}/questions", HandleAsync)
            .WithName("CreateModuleQuestion")
            .WithSummary("Add a question to a module")
            .WithTags("Modules");
    }

    public static async Task<Results<CreatedAtRoute<CreateModuleQuestionResponse>, ValidationProblem, NotFound, Conflict<string>>> HandleAsync(
        Guid moduleId,
        CreateModuleQuestionRequest request,
        ApplicationDbContext db,
        IValidator<CreateModuleQuestionRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var module = await db.Modules.FindAsync(new object[] { moduleId }, cancellationToken);
        if (module == null)
        {
            return TypedResults.NotFound();
        }

        var questionBankItem = await db.QuestionBankItems.FindAsync(new object[] { request.QuestionBankItemId }, cancellationToken);
        if (questionBankItem == null)
        {
            return TypedResults.NotFound();
        }

        // Check if this question is already added to this module
        var existingRelation = await db.Set<ModuleQuestion>()
            .FirstOrDefaultAsync(mq => mq.ModuleId == moduleId && mq.QuestionBankItemId == request.QuestionBankItemId, cancellationToken);
        
        if (existingRelation != null)
        {
            return TypedResults.Conflict($"Question '{questionBankItem.VariableName}' is already added to this module.");
        }

        var moduleQuestion = new ModuleQuestion
        {
            Id = Guid.NewGuid(),
            ModuleId = moduleId,
            QuestionBankItemId = request.QuestionBankItemId,
            DisplayOrder = request.DisplayOrder,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System"
        };

        db.Set<ModuleQuestion>().Add(moduleQuestion);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
                || ex.InnerException?.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) == true)
            {
                return TypedResults.Conflict($"Question '{questionBankItem.VariableName}' is already added to this module.");
            }

            throw;
        }

        var response = new CreateModuleQuestionResponse(
            moduleQuestion.Id,
            moduleQuestion.ModuleId,
            moduleQuestion.QuestionBankItemId,
            questionBankItem.VariableName,
            questionBankItem.QuestionType,
            questionBankItem.QuestionText,
            questionBankItem.Classification,
            moduleQuestion.DisplayOrder
        );

        return TypedResults.CreatedAtRoute(
            response,
            "GetModuleById",
            new { id = moduleId });
    }
}
