namespace Api.Features.Modules;

public record CreateModuleQuestionRequest(
    Guid ModuleId,
    Guid QuestionBankItemId,
    int SortOrder);

public record CreateModuleQuestionResponse(
    Guid Id,
    Guid ModuleId,
    Guid QuestionBankItemId,
    int SortOrder);

public record UpdateModuleQuestionRequest(
    int SortOrder,
    bool IsActive);

public record ModuleQuestionInfo(
    Guid Id,
    Guid ModuleId,
    Guid QuestionBankItemId,
    int SortOrder,
    bool IsActive,
    DateTime CreatedOn,
    string? QuestionVariableName,
    string? QuestionText);
