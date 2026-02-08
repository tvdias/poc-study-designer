namespace Api.Features.Products;

public record CreateProductConfigQuestionDisplayRuleRequest(
    Guid ProductConfigQuestionId,
    Guid TriggeringConfigurationQuestionId,
    Guid? TriggeringAnswerId,
    string DisplayCondition); // "Show" or "Hide"

public record CreateProductConfigQuestionDisplayRuleResponse(
    Guid Id,
    Guid ProductConfigQuestionId,
    Guid TriggeringConfigurationQuestionId,
    Guid? TriggeringAnswerId,
    string DisplayCondition);

public record UpdateProductConfigQuestionDisplayRuleRequest(
    Guid TriggeringConfigurationQuestionId,
    Guid? TriggeringAnswerId,
    string DisplayCondition,
    bool IsActive);

public record ProductConfigQuestionDisplayRuleInfo(
    Guid Id,
    Guid ProductConfigQuestionId,
    Guid TriggeringConfigurationQuestionId,
    Guid? TriggeringAnswerId,
    string DisplayCondition,
    bool IsActive,
    DateTime CreatedOn,
    string? TriggeringQuestionText,
    string? TriggeringAnswerName);
