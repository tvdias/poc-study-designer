using Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api.Features.QuestionBank;

public interface IQuestionBankService
{
    Task<CreateQuestionBankItemResponse> CreateQuestionBankItemAsync(
        CreateQuestionBankItemRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    Task<UpdateQuestionBankItemResponse> UpdateQuestionBankItemAsync(
        Guid id,
        UpdateQuestionBankItemRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    Task<CreateQuestionAnswerResponse> CreateQuestionAnswerAsync(
        Guid questionId,
        CreateQuestionAnswerRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    Task DeleteQuestionAnswerAsync(
        Guid answerId,
        CancellationToken cancellationToken = default);

    Task<UpdateQuestionAnswerResponse> UpdateQuestionAnswerAsync(
        Guid answerId,
        UpdateQuestionAnswerRequest request,
        string userId,
        CancellationToken cancellationToken = default);
}

public class QuestionBankService : IQuestionBankService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<QuestionBankService> _logger;

    public QuestionBankService(ApplicationDbContext context, ILogger<QuestionBankService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CreateQuestionBankItemResponse> CreateQuestionBankItemAsync(
        CreateQuestionBankItemRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating question bank item: {VariableName} v{Version}", request.VariableName, request.Version);

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
            CreatedBy = userId
        };

        _context.QuestionBankItems.Add(question);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
                || ex.InnerException?.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) == true)
            {
                _logger.LogWarning("Duplicate question: {VariableName} v{Version}", request.VariableName, request.Version);
                throw new InvalidOperationException($"Question with variable name '{request.VariableName}' and version {request.Version} already exists.", ex);
            }
            throw;
        }

        _logger.LogInformation("Successfully created question bank item {QuestionId}", question.Id);

        return new CreateQuestionBankItemResponse(question.Id, question.VariableName, question.Version);
    }

    public async Task<UpdateQuestionBankItemResponse> UpdateQuestionBankItemAsync(
        Guid id,
        UpdateQuestionBankItemRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating question bank item {QuestionId}", id);

        var question = await _context.QuestionBankItems.FindAsync([id], cancellationToken);
        if (question == null)
        {
            throw new InvalidOperationException($"Question bank item {id} not found.");
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
        question.ModifiedBy = userId;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
                || ex.InnerException?.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) == true)
            {
                _logger.LogWarning("Duplicate question: {VariableName} v{Version}", request.VariableName, request.Version);
                throw new InvalidOperationException($"Question with variable name '{request.VariableName}' and version {request.Version} already exists.", ex);
            }
            throw;
        }

        _logger.LogInformation("Successfully updated question bank item {QuestionId}", id);

        return new UpdateQuestionBankItemResponse(question.Id, question.VariableName, question.Version);
    }

    public async Task<CreateQuestionAnswerResponse> CreateQuestionAnswerAsync(
        Guid questionId,
        CreateQuestionAnswerRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating answer for question {QuestionId}: {AnswerCode}", questionId, request.AnswerCode);

        var question = await _context.QuestionBankItems.FindAsync([questionId], cancellationToken);
        if (question == null)
        {
            throw new InvalidOperationException($"Question bank item {questionId} not found.");
        }

        var answer = new QuestionAnswer
        {
            Id = Guid.NewGuid(),
            QuestionBankItemId = questionId,
            AnswerText = request.AnswerText,
            AnswerCode = request.AnswerCode,
            AnswerLocation = request.AnswerLocation,
            IsOpen = request.IsOpen,
            IsFixed = request.IsFixed,
            IsExclusive = request.IsExclusive,
            IsActive = request.IsActive,
            CustomProperty = request.CustomProperty,
            Facets = request.Facets,
            Version = request.Version,
            DisplayOrder = request.DisplayOrder,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.QuestionAnswers.Add(answer);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
                || ex.InnerException?.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) == true)
            {
                _logger.LogWarning("Duplicate answer code: {AnswerCode}", request.AnswerCode);
                throw new InvalidOperationException($"Answer with code '{request.AnswerCode}' already exists for this question.", ex);
            }
            throw;
        }

        _logger.LogInformation("Successfully created answer {AnswerId} for question {QuestionId}", answer.Id, questionId);

        return new CreateQuestionAnswerResponse(answer.Id, answer.AnswerText);
    }

    public async Task DeleteQuestionAnswerAsync(
        Guid answerId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting question answer {AnswerId}", answerId);

        var answer = await _context.QuestionAnswers.FindAsync([answerId], cancellationToken);
        if (answer == null)
        {
            throw new InvalidOperationException($"Question answer {answerId} not found.");
        }

        _context.QuestionAnswers.Remove(answer);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully deleted question answer {AnswerId}", answerId);
    }

    public async Task<UpdateQuestionAnswerResponse> UpdateQuestionAnswerAsync(
        Guid answerId,
        UpdateQuestionAnswerRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating question answer {AnswerId}", answerId);

        var answer = await _context.QuestionAnswers.FindAsync([answerId], cancellationToken);
        if (answer == null)
        {
            throw new InvalidOperationException($"Question answer {answerId} not found.");
        }

        answer.AnswerText = request.AnswerText;
        answer.AnswerCode = request.AnswerCode;
        answer.AnswerLocation = request.AnswerLocation;
        answer.IsOpen = request.IsOpen;
        answer.IsFixed = request.IsFixed;
        answer.IsExclusive = request.IsExclusive;
        answer.IsActive = request.IsActive;
        answer.CustomProperty = request.CustomProperty;
        answer.Facets = request.Facets;
        answer.Version = request.Version;
        answer.DisplayOrder = request.DisplayOrder;
        answer.ModifiedOn = DateTime.UtcNow;
        answer.ModifiedBy = userId;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated question answer {AnswerId}", answerId);

        return new UpdateQuestionAnswerResponse(answer.Id, answer.AnswerText);
    }
}
