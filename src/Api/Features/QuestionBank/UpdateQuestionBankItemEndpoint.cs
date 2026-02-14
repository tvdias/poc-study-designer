using Api.Data;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.QuestionBank;

public static class UpdateQuestionBankItemEndpoint
{
    public static void MapUpdateQuestionBankItemEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/question-bank/{id:guid}", HandleAsync)
            .WithName("UpdateQuestionBankItem")
            .WithSummary("Update Question Bank Item")
            .WithTags("QuestionBank");
    }

    public static async Task<Results<Ok<UpdateQuestionBankItemResponse>, ValidationProblem, NotFound, Conflict<string>>> HandleAsync(
        Guid id,
        UpdateQuestionBankItemRequest request,
        ApplicationDbContext db,
        IValidator<UpdateQuestionBankItemRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var question = await db.QuestionBankItems.FindAsync(new object[] { id }, cancellationToken);
        if (question == null)
        {
            return TypedResults.NotFound();
        }

        question.VariableName = request.VariableName;
        question.Version = request.Version;
        question.QuestionType = request.QuestionType;
        question.QuestionText = request.QuestionText;
        question.Classification = request.Classification;
        question.IsDummy = request.IsDummy;
        question.QuestionTitle = request.QuestionTitle;
        question.Status = request.Status;
        question.Methodology = request.Methodology;
        question.DataQualityTag = request.DataQualityTag;
        question.RowSortOrder = request.RowSortOrder;
        question.ColumnSortOrder = request.ColumnSortOrder;
        question.AnswerMin = request.AnswerMin;
        question.AnswerMax = request.AnswerMax;
        question.QuestionFormatDetails = request.QuestionFormatDetails;
        question.ScraperNotes = request.ScraperNotes;
        question.CustomNotes = request.CustomNotes;
        question.MetricGroupId = request.MetricGroupId;
        question.TableTitle = request.TableTitle;
        question.QuestionRationale = request.QuestionRationale;
        question.SingleOrMulticode = request.SingleOrMulticode;
        question.ManagedListReferences = request.ManagedListReferences;
        question.IsTranslatable = request.IsTranslatable;
        question.IsHidden = request.IsHidden;
        question.IsQuestionActive = request.IsQuestionActive;
        question.IsQuestionOutOfUse = request.IsQuestionOutOfUse;
        question.AnswerRestrictionMin = request.AnswerRestrictionMin;
        question.AnswerRestrictionMax = request.AnswerRestrictionMax;
        question.RestrictionDataType = request.RestrictionDataType;
        question.RestrictedToClient = request.RestrictedToClient;
        question.AnswerTypeCode = request.AnswerTypeCode;
        question.IsAnswerRequired = request.IsAnswerRequired;
        question.ScalePoint = request.ScalePoint;
        question.ScaleType = request.ScaleType;
        question.DisplayType = request.DisplayType;
        question.InstructionText = request.InstructionText;
        question.ParentQuestionId = request.ParentQuestionId;
        question.QuestionFacet = request.QuestionFacet;
        question.ModifiedOn = DateTime.UtcNow;
        question.ModifiedBy = "System";

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
                || ex.InnerException?.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) == true)
            {
                return TypedResults.Conflict($"Question with variable name '{request.VariableName}' and version {request.Version} already exists.");
            }

            throw;
        }

        return TypedResults.Ok(new UpdateQuestionBankItemResponse(question.Id, question.VariableName, question.Version));
    }
}
