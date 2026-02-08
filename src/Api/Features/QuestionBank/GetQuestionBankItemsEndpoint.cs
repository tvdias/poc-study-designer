using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.QuestionBank;

public static class GetQuestionBankItemsEndpoint
{
    public static void MapGetQuestionBankItemsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/question-bank", HandleAsync)
            .WithName("GetQuestionBankItems")
            .WithSummary("Get Question Bank Items")
            .WithTags("QuestionBank");
    }

    public static async Task<Ok<List<GetQuestionBankItemsResponse>>> HandleAsync(
        ApplicationDbContext db,
        string? query,
        CancellationToken cancellationToken)
    {
        var questionsQuery = db.QuestionBankItems.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            questionsQuery = questionsQuery.Where(q => 
                EF.Functions.ILike(q.VariableName, $"%{query}%") ||
                EF.Functions.ILike(q.QuestionText ?? "", $"%{query}%") ||
                EF.Functions.ILike(q.QuestionTitle ?? "", $"%{query}%"));
        }

        var questions = await questionsQuery
            .OrderBy(q => q.VariableName)
            .Select(q => new GetQuestionBankItemsResponse(
                q.Id,
                q.VariableName,
                q.Version,
                q.QuestionType,
                q.QuestionText,
                q.Classification,
                q.IsDummy,
                q.QuestionTitle,
                q.Status,
                q.Methodology,
                q.CreatedOn,
                q.CreatedBy
            ))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(questions);
    }
}
