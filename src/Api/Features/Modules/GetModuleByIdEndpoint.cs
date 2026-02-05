using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Modules;

public static class GetModuleByIdEndpoint
{
    public static void MapGetModuleByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/modules/{id}", HandleAsync)
            .WithName("GetModuleById")
            .WithSummary("Get Module By Id")
            .WithTags("Modules");
    }

    public static async Task<Results<Ok<GetModuleByIdResponse>, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var module = await db.Modules
            .AsNoTracking()
            .Include(m => m.ModuleQuestions)
                .ThenInclude(mq => mq.Question)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (module is null)
        {
            return TypedResults.NotFound();
        }

        var questions = module.ModuleQuestions
            .OrderBy(mq => mq.DisplayOrder)
            .Select(mq => new ModuleQuestionDto(
                mq.Question.Id,
                mq.Question.VariableName,
                mq.Question.QuestionType,
                mq.Question.QuestionText,
                mq.Question.QuestionSource,
                mq.DisplayOrder,
                mq.Question.CreatedBy ?? "System"))
            .ToList();

        var response = new GetModuleByIdResponse(
            module.Id,
            module.VariableName,
            module.Label,
            module.Description,
            module.VersionNumber,
            module.ParentModuleId,
            module.Instructions,
            module.Status,
            module.StatusReason,
            module.IsActive,
            questions
        );

        return TypedResults.Ok(response);
    }
}

public record GetModuleByIdResponse(
    Guid Id,
    string VariableName,
    string Label,
    string? Description,
    int VersionNumber,
    Guid? ParentModuleId,
    string? Instructions,
    string Status,
    string? StatusReason,
    bool IsActive,
    List<ModuleQuestionDto> Questions
);

public record ModuleQuestionDto(
    Guid QuestionId,
    string VariableName,
    string QuestionType,
    string QuestionText,
    string QuestionSource,
    int DisplayOrder,
    string CreatedBy
);
