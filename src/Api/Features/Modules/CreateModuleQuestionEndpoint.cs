using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.Modules;

public static class CreateModuleQuestionEndpoint
{
    public static void MapCreateModuleQuestionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/module-questions", HandleAsync)
            .WithName("CreateModuleQuestion")
            .WithSummary("Create Module Question")
            .WithTags("ModuleQuestions");
    }

    public static async Task<Results<CreatedAtRoute<CreateModuleQuestionResponse>, ValidationProblem, Conflict<string>>> HandleAsync(
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

        var moduleQuestion = new ModuleQuestion
        {
            Id = Guid.NewGuid(),
            ModuleId = request.ModuleId,
            QuestionBankItemId = request.QuestionBankItemId,
            SortOrder = request.SortOrder,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System"
        };

        db.ModuleQuestions.Add(moduleQuestion);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
                || ex.InnerException?.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) == true)
            {
                return TypedResults.Conflict("This question is already assigned to this module.");
            }

            if (ex.InnerException?.Message.Contains("foreign key", StringComparison.OrdinalIgnoreCase) == true)
            {
                return TypedResults.Conflict("Referenced module or question bank item does not exist.");
            }

            throw;
        }

        return TypedResults.CreatedAtRoute(
            new CreateModuleQuestionResponse(
                moduleQuestion.Id,
                moduleQuestion.ModuleId,
                moduleQuestion.QuestionBankItemId,
                moduleQuestion.SortOrder),
            "GetModuleQuestionById",
            new { id = moduleQuestion.Id });
    }
}
