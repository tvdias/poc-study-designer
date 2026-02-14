using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.QuestionBank;

public static class GetQuestionBankItemByIdEndpoint
{
    public static void MapGetQuestionBankItemByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/question-bank/{id:guid}", HandleAsync)
            .WithName("GetQuestionBankItemById")
            .WithSummary("Get Question Bank Item By ID")
            .WithTags("QuestionBank");
    }

    public static async Task<Results<Ok<GetQuestionBankItemByIdResponse>, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var question = await db.QuestionBankItems
            .Include(q => q.Answers)
            .Where(q => q.Id == id)
            .Select(q => new GetQuestionBankItemByIdResponse(
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
                q.DataQualityTag,
                q.RowSortOrder,
                q.ColumnSortOrder,
                q.AnswerMin,
                q.AnswerMax,
                q.QuestionFormatDetails,
                q.ScraperNotes,
                q.CustomNotes,
                q.MetricGroupId,
                q.MetricGroup != null ? q.MetricGroup.Name : null,
                q.TableTitle,
                q.QuestionRationale,
                q.SingleOrMulticode,
                q.ManagedListReferences,
                q.IsTranslatable,
                q.IsHidden,
                q.IsQuestionActive,
                q.IsQuestionOutOfUse,
                q.AnswerRestrictionMin,
                q.AnswerRestrictionMax,
                q.RestrictionDataType,
                q.RestrictedToClient,
                q.AnswerTypeCode,
                q.IsAnswerRequired,
                q.ScalePoint,
                q.ScaleType,
                q.DisplayType,
                q.InstructionText,
                q.ParentQuestionId,
                q.QuestionFacet,
                q.CreatedOn,
                q.CreatedBy,
                q.ModifiedOn,
                q.ModifiedBy,
                q.Answers.Select(a => new QuestionAnswerResponse(
                    a.Id,
                    a.AnswerText,
                    a.AnswerCode,
                    a.AnswerLocation,
                    a.IsOpen,
                    a.IsFixed,
                    a.IsExclusive,
                    a.IsActive,
                    a.CustomProperty,
                    a.Facets,
                    a.Version,
                    a.DisplayOrder,
                    a.CreatedOn,
                    a.CreatedBy
                )).ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (question == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(question);
    }
}
