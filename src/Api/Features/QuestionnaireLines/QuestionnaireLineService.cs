using Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api.Features.QuestionnaireLines;

public interface IQuestionnaireLineService
{
    Task<AddQuestionnaireLineResponse> AddQuestionnaireLineAsync(
        Guid projectId,
        AddQuestionnaireLineRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    Task<QuestionnaireLineDto> UpdateQuestionnaireLineAsync(
        Guid projectId,
        Guid id,
        UpdateQuestionnaireLineRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    Task DeleteQuestionnaireLineAsync(
        Guid projectId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<List<QuestionnaireLineDto>> GetQuestionnaireLinesAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);

    Task UpdateQuestionnaireLinesSortOrderAsync(
        Guid projectId,
        List<UpdateSortOrderRequest> sortOrders,
        string userId,
        CancellationToken cancellationToken = default);
}

public class QuestionnaireLineService : IQuestionnaireLineService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<QuestionnaireLineService> _logger;

    public QuestionnaireLineService(ApplicationDbContext context, ILogger<QuestionnaireLineService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AddQuestionnaireLineResponse> AddQuestionnaireLineAsync(
        Guid projectId,
        AddQuestionnaireLineRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding questionnaire line to project {ProjectId}", projectId);

        var projectExists = await _context.Projects.AnyAsync(p => p.Id == projectId, cancellationToken);
        if (!projectExists)
        {
            throw new InvalidOperationException($"Project {projectId} not found.");
        }

        string variableName;
        int version;
        string? questionText = null;
        string? questionTitle = null;
        string? questionType = null;
        string? classification = null;
        string? questionRationale = null;
        string? scraperNotes = null;
        string? customNotes = null;
        int? rowSortOrder = null;
        int? columnSortOrder = null;
        int? answerMin = null;
        int? answerMax = null;
        string? questionFormatDetails = null;
        bool isDummy = false;

        if (request.QuestionBankItemId.HasValue)
        {
            var questionBankItem = await _context.QuestionBankItems
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == request.QuestionBankItemId.Value, cancellationToken);

            if (questionBankItem == null)
            {
                throw new InvalidOperationException($"Question bank item {request.QuestionBankItemId.Value} not found.");
            }

            var exists = await _context.Set<QuestionnaireLine>()
                .AnyAsync(pq => pq.ProjectId == projectId && pq.QuestionBankItemId == request.QuestionBankItemId.Value, cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException("This question has already been added to the project questionnaire.");
            }

            variableName = questionBankItem.VariableName;
            version = questionBankItem.Version;
            questionText = questionBankItem.QuestionText;
            questionTitle = questionBankItem.QuestionTitle;
            questionType = questionBankItem.QuestionType;
            classification = questionBankItem.Classification;
            questionRationale = questionBankItem.QuestionRationale;
            scraperNotes = questionBankItem.ScraperNotes;
            customNotes = questionBankItem.CustomNotes;
            rowSortOrder = questionBankItem.RowSortOrder;
            columnSortOrder = questionBankItem.ColumnSortOrder;
            answerMin = questionBankItem.AnswerMin;
            answerMax = questionBankItem.AnswerMax;
            questionFormatDetails = questionBankItem.QuestionFormatDetails;
            isDummy = questionBankItem.IsDummy;
        }
        else
        {
            variableName = request.VariableName!;
            version = request.Version ?? 1;
            questionText = request.QuestionText;
            questionTitle = request.QuestionTitle;
            questionType = request.QuestionType;
            classification = request.Classification;
            questionRationale = request.QuestionRationale;
            scraperNotes = request.ScraperNotes;
            customNotes = request.CustomNotes;
            rowSortOrder = request.RowSortOrder;
            columnSortOrder = request.ColumnSortOrder;
            answerMin = request.AnswerMin;
            answerMax = request.AnswerMax;
            questionFormatDetails = request.QuestionFormatDetails;
            isDummy = request.IsDummy ?? false;
        }

        var maxSortOrder = await _context.Set<QuestionnaireLine>()
            .Where(pq => pq.ProjectId == projectId)
            .MaxAsync(pq => (int?)pq.SortOrder, cancellationToken) ?? -1;

        var questionnaireLine = new QuestionnaireLine
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            QuestionBankItemId = request.QuestionBankItemId,
            SortOrder = maxSortOrder + 1,
            VariableName = variableName,
            Version = version,
            QuestionText = questionText,
            QuestionTitle = questionTitle,
            QuestionType = questionType,
            Classification = classification,
            QuestionRationale = questionRationale,
            ScraperNotes = scraperNotes,
            CustomNotes = customNotes,
            RowSortOrder = rowSortOrder,
            ColumnSortOrder = columnSortOrder,
            AnswerMin = answerMin,
            AnswerMax = answerMax,
            QuestionFormatDetails = questionFormatDetails,
            IsDummy = isDummy,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.Set<QuestionnaireLine>().Add(questionnaireLine);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully added questionnaire line {LineId} to project {ProjectId}", questionnaireLine.Id, projectId);

        return new AddQuestionnaireLineResponse(
            questionnaireLine.Id,
            questionnaireLine.ProjectId,
            questionnaireLine.QuestionBankItemId,
            questionnaireLine.SortOrder,
            questionnaireLine.VariableName,
            questionnaireLine.Version,
            questionnaireLine.QuestionText,
            questionnaireLine.QuestionTitle,
            questionnaireLine.QuestionType,
            questionnaireLine.Classification,
            questionnaireLine.QuestionRationale);
    }

    public async Task<QuestionnaireLineDto> UpdateQuestionnaireLineAsync(
        Guid projectId,
        Guid id,
        UpdateQuestionnaireLineRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating questionnaire line {LineId} in project {ProjectId}", id, projectId);

        var questionnaire = await _context.Set<QuestionnaireLine>()
            .FirstOrDefaultAsync(pq => pq.Id == id && pq.ProjectId == projectId, cancellationToken);

        if (questionnaire == null)
        {
            throw new InvalidOperationException($"Questionnaire line {id} not found in project {projectId}.");
        }

        questionnaire.QuestionText = request.QuestionText;
        questionnaire.QuestionTitle = request.QuestionTitle;
        questionnaire.QuestionRationale = request.QuestionRationale;
        questionnaire.ScraperNotes = request.ScraperNotes;
        questionnaire.CustomNotes = request.CustomNotes;
        questionnaire.RowSortOrder = request.RowSortOrder;
        questionnaire.ColumnSortOrder = request.ColumnSortOrder;
        questionnaire.AnswerMin = request.AnswerMin;
        questionnaire.AnswerMax = request.AnswerMax;
        questionnaire.QuestionFormatDetails = request.QuestionFormatDetails;
        questionnaire.ModifiedOn = DateTime.UtcNow;
        questionnaire.ModifiedBy = userId;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated questionnaire line {LineId}", id);

        return new QuestionnaireLineDto(
            questionnaire.Id,
            questionnaire.ProjectId,
            questionnaire.QuestionBankItemId,
            questionnaire.SortOrder,
            questionnaire.VariableName,
            questionnaire.Version,
            questionnaire.QuestionText,
            questionnaire.QuestionTitle,
            questionnaire.QuestionType,
            questionnaire.Classification,
            questionnaire.QuestionRationale,
            questionnaire.ScraperNotes,
            questionnaire.CustomNotes,
            questionnaire.RowSortOrder,
            questionnaire.ColumnSortOrder,
            questionnaire.AnswerMin,
            questionnaire.AnswerMax,
            questionnaire.QuestionFormatDetails,
            questionnaire.IsDummy);
    }

    public async Task DeleteQuestionnaireLineAsync(
        Guid projectId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting questionnaire line {LineId} from project {ProjectId}", id, projectId);

        var questionnaire = await _context.Set<QuestionnaireLine>()
            .FirstOrDefaultAsync(pq => pq.Id == id && pq.ProjectId == projectId, cancellationToken);

        if (questionnaire == null)
        {
            throw new InvalidOperationException($"Questionnaire line {id} not found in project {projectId}.");
        }

        _context.Set<QuestionnaireLine>().Remove(questionnaire);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully deleted questionnaire line {LineId}", id);
    }

    public async Task<List<QuestionnaireLineDto>> GetQuestionnaireLinesAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var lines = await _context.Set<QuestionnaireLine>()
            .Where(q => q.ProjectId == projectId)
            .OrderBy(q => q.SortOrder)
            .Select(q => new QuestionnaireLineDto(
                q.Id,
                q.ProjectId,
                q.QuestionBankItemId,
                q.SortOrder,
                q.VariableName,
                q.Version,
                q.QuestionText,
                q.QuestionTitle,
                q.QuestionType,
                q.Classification,
                q.QuestionRationale,
                q.ScraperNotes,
                q.CustomNotes,
                q.RowSortOrder,
                q.ColumnSortOrder,
                q.AnswerMin,
                q.AnswerMax,
                q.QuestionFormatDetails,
                q.IsDummy))
            .ToListAsync(cancellationToken);

        return lines;
    }

    public async Task UpdateQuestionnaireLinesSortOrderAsync(
        Guid projectId,
        List<UpdateSortOrderRequest> sortOrders,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating sort order for {Count} questionnaire lines in project {ProjectId}", sortOrders.Count, projectId);

        var lineIds = sortOrders.Select(s => s.Id).ToList();
        var lines = await _context.Set<QuestionnaireLine>()
            .Where(q => q.ProjectId == projectId && lineIds.Contains(q.Id))
            .ToListAsync(cancellationToken);

        foreach (var sortOrder in sortOrders)
        {
            var line = lines.FirstOrDefault(l => l.Id == sortOrder.Id);
            if (line != null)
            {
                line.SortOrder = sortOrder.SortOrder;
                line.ModifiedOn = DateTime.UtcNow;
                line.ModifiedBy = userId;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated sort order for questionnaire lines in project {ProjectId}", projectId);
    }
}

public record UpdateSortOrderRequest(Guid Id, int SortOrder);
