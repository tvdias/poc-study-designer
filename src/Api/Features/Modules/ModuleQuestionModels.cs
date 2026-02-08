namespace Api.Features.Modules;

public record CreateModuleQuestionRequest(
    Guid QuestionBankItemId,
    int DisplayOrder
);

public record CreateModuleQuestionResponse(
    Guid Id,
    Guid ModuleId,
    Guid QuestionBankItemId,
    string QuestionVariableName,
    string? QuestionType,
    string? QuestionText,
    string? Classification,
    int DisplayOrder
);

public record ModuleQuestionDto(
    Guid Id,
    Guid ModuleId,
    Guid QuestionBankItemId,
    string QuestionVariableName,
    string? QuestionType,
    string? QuestionText,
    string? Classification,
    int DisplayOrder
);
