namespace Api.Features.Products;

public record CreateProductTemplateLineRequest(
    Guid ProductTemplateId,
    string Name,
    string Type, // "Module" or "Question"
    bool IncludeByDefault,
    int SortOrder,
    Guid? ModuleId,
    Guid? QuestionBankItemId);

public record CreateProductTemplateLineResponse(
    Guid Id,
    Guid ProductTemplateId,
    string Name,
    string Type,
    bool IncludeByDefault,
    int SortOrder,
    Guid? ModuleId,
    Guid? QuestionBankItemId);

public record UpdateProductTemplateLineRequest(
    string Name,
    string Type,
    bool IncludeByDefault,
    int SortOrder,
    Guid? ModuleId,
    Guid? QuestionBankItemId,
    bool IsActive);

public record ProductTemplateLineInfo(
    Guid Id,
    Guid ProductTemplateId,
    string Name,
    string Type,
    bool IncludeByDefault,
    int SortOrder,
    Guid? ModuleId,
    Guid? QuestionBankItemId,
    bool IsActive,
    DateTime CreatedOn,
    string? ModuleName,
    string? QuestionBankItemName);
