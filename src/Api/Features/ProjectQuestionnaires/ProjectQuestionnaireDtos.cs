namespace Api.Features.ProjectQuestionnaires;

public record AddProjectQuestionnaireRequest(
    Guid QuestionBankItemId
);

public record AddProjectQuestionnaireResponse(
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

public record ProjectQuestionnaireDto(
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

public record UpdateProjectQuestionnaireRequest(
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

public record UpdateProjectQuestionnaireSortOrderRequest(
    Guid Id,
    int SortOrder
);

public record UpdateProjectQuestionnairesSortOrderRequest(
    List<UpdateProjectQuestionnaireSortOrderRequest> Items
);
