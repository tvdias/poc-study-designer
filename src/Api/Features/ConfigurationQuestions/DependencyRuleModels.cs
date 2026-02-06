namespace Api.Features.ConfigurationQuestions;

// Create Dependency Rule
public record CreateDependencyRuleRequest(
    string Name,
    Guid ConfigurationQuestionId,
    Guid? TriggeringAnswerId,
    string? Classification,
    string? Type,
    string? ContentType,
    string? Module,
    string? QuestionBank,
    string? Tag
);

public record CreateDependencyRuleResponse(
    Guid Id,
    string Name,
    Guid ConfigurationQuestionId,
    Guid? TriggeringAnswerId,
    string? Classification,
    string? Type,
    string? ContentType,
    string? Module,
    string? QuestionBank,
    string? Tag,
    string? StatusReason,
    bool IsActive
);

// Update Dependency Rule
public record UpdateDependencyRuleRequest(
    string Name,
    Guid? TriggeringAnswerId,
    string? Classification,
    string? Type,
    string? ContentType,
    string? Module,
    string? QuestionBank,
    string? Tag,
    string? StatusReason,
    bool IsActive
);

public record UpdateDependencyRuleResponse(
    Guid Id,
    string Name,
    Guid ConfigurationQuestionId,
    Guid? TriggeringAnswerId,
    string? Classification,
    string? Type,
    string? ContentType,
    string? Module,
    string? QuestionBank,
    string? Tag,
    string? StatusReason,
    bool IsActive
);

// Get Dependency Rules
public record GetDependencyRulesResponse(
    Guid Id,
    string Name,
    Guid ConfigurationQuestionId,
    string QuestionText,
    Guid? TriggeringAnswerId,
    string? TriggeringAnswerName,
    string? Classification,
    string? Type,
    string? ContentType,
    string? Module,
    string? QuestionBank,
    string? Tag,
    string? StatusReason,
    bool IsActive
);
