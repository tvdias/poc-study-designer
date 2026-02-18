namespace Api.Features.QuestionnaireLines;

public record AddQuestionnaireLineRequest(
    Guid QuestionBankItemId
);

public record AddQuestionnaireLineResponse(
    Guid Id,
    Guid ProjectId,
    Guid QuestionBankItemId,
    int SortOrder,
    string VariableName,
    int Version,
    string? QuestionText,
    string? QuestionTitle,
    string? QuestionType,
    string? Classification,
    string? QuestionRationale
);

public record QuestionnaireLineDto(
    Guid Id,
    Guid ProjectId,
    Guid QuestionBankItemId,
    int SortOrder,
    string VariableName,
    int Version,
    string? QuestionText,
    string? QuestionTitle,
    string? QuestionType,
    string? Classification,
    string? QuestionRationale,
    string? ScraperNotes,
    string? CustomNotes,
    int? RowSortOrder,
    int? ColumnSortOrder,
    int? AnswerMin,
    int? AnswerMax,
    string? QuestionFormatDetails,
    bool IsDummy
);

public record UpdateQuestionnaireLineRequest(
    string? QuestionText,
    string? QuestionTitle,
    string? QuestionRationale,
    string? ScraperNotes,
    string? CustomNotes,
    int? RowSortOrder,
    int? ColumnSortOrder,
    int? AnswerMin,
    int? AnswerMax,
    string? QuestionFormatDetails
);

public record UpdateQuestionnaireLineSortOrderRequest(
    Guid Id,
    int SortOrder
);

public record UpdateQuestionnaireLinesSortOrderRequest(
    List<UpdateQuestionnaireLineSortOrderRequest> Items
);
