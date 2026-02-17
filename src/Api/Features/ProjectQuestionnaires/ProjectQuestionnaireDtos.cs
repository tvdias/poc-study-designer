namespace Api.Features.ProjectQuestionnaires;

public record AddProjectQuestionnaireRequest(
    Guid QuestionBankItemId
);

public record AddProjectQuestionnaireResponse(
    Guid Id,
    Guid ProjectId,
    Guid QuestionBankItemId,
    int SortOrder,
    QuestionBankItemSummary QuestionBankItem
);

public record QuestionBankItemSummary(
    Guid Id,
    string VariableName,
    int Version,
    string? QuestionText,
    string? QuestionType,
    string? Classification,
    string? QuestionRationale
);

public record ProjectQuestionnaireDto(
    Guid Id,
    Guid ProjectId,
    Guid QuestionBankItemId,
    int SortOrder,
    QuestionBankItemSummary QuestionBankItem
);

public record UpdateProjectQuestionnaireSortOrderRequest(
    Guid Id,
    int SortOrder
);

public record UpdateProjectQuestionnairesSortOrderRequest(
    List<UpdateProjectQuestionnaireSortOrderRequest> Items
);
