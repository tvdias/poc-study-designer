using Api.Data;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.QuestionBank;

public static class CreateQuestionBankItemEndpoint
{
    public static void MapCreateQuestionBankItemEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/question-bank", HandleAsync)
            .WithName("CreateQuestionBankItem")
            .WithSummary("Create Question Bank Item")
            .WithTags("QuestionBank");
    }

    public static async Task<Results<CreatedAtRoute<CreateQuestionBankItemResponse>, ValidationProblem, Conflict<string>>> HandleAsync(
        CreateQuestionBankItemRequest request,
        ApplicationDbContext db,
        IValidator<CreateQuestionBankItemRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var question = new QuestionBankItem
        {
            Id = Guid.NewGuid(),
            VariableName = request.VariableName,
            Version = request.Version,
            QuestionType = request.QuestionType,
            QuestionText = request.QuestionText,
            Classification = request.Classification,
            IsDummy = request.IsDummy,
            QuestionTitle = request.QuestionTitle,
            Status = request.Status,
            Methodology = request.Methodology,
            DataQualityTag = request.DataQualityTag,
            RowSortOrder = request.RowSortOrder,
            ColumnSortOrder = request.ColumnSortOrder,
            AnswerMin = request.AnswerMin,
            AnswerMax = request.AnswerMax,
            QuestionFormatDetails = request.QuestionFormatDetails,
            ScraperNotes = request.ScraperNotes,
            CustomNotes = request.CustomNotes,
            MetricGroupId = request.MetricGroupId,
            TableTitle = request.TableTitle,
            QuestionRationale = request.QuestionRationale,
            SingleOrMulticode = request.SingleOrMulticode,
            ManagedListReferences = request.ManagedListReferences,
            IsTranslatable = request.IsTranslatable,
            IsHidden = request.IsHidden,
            IsQuestionActive = request.IsQuestionActive,
            IsQuestionOutOfUse = request.IsQuestionOutOfUse,
            AnswerRestrictionMin = request.AnswerRestrictionMin,
            AnswerRestrictionMax = request.AnswerRestrictionMax,
            RestrictionDataType = request.RestrictionDataType,
            RestrictedToClient = request.RestrictedToClient,
            AnswerTypeCode = request.AnswerTypeCode,
            IsAnswerRequired = request.IsAnswerRequired,
            ScalePoint = request.ScalePoint,
            ScaleType = request.ScaleType,
            DisplayType = request.DisplayType,
            InstructionText = request.InstructionText,
            ParentQuestionId = request.ParentQuestionId,
            QuestionFacet = request.QuestionFacet,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System"
        };

        db.QuestionBankItems.Add(question);

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

        return TypedResults.CreatedAtRoute(
            new CreateQuestionBankItemResponse(question.Id, question.VariableName, question.Version),
            "GetQuestionBankItemById",
            new { id = question.Id });
    }
}
