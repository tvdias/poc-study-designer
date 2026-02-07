namespace Api.Features.ConfigurationQuestions;

// Create Configuration Question
public record CreateConfigurationQuestionRequest(
    string Question,
    string? AiPrompt,
    RuleType RuleType
);

public record CreateConfigurationQuestionResponse(
    Guid Id,
    string Question,
    string? AiPrompt,
    RuleType RuleType,
    bool IsActive,
    int Version
);

// Update Configuration Question
public record UpdateConfigurationQuestionRequest(
    string Question,
    string? AiPrompt,
    RuleType RuleType,
    bool IsActive
);

public record UpdateConfigurationQuestionResponse(
    Guid Id,
    string Question,
    string? AiPrompt,
    RuleType RuleType,
    bool IsActive,
    int Version
);

// Get Configuration Questions
public record GetConfigurationQuestionsResponse(
    Guid Id,
    string Question,
    string? AiPrompt,
    RuleType RuleType,
    bool IsActive,
    int Version,
    int AnswersCount
);

// Get Configuration Question by ID
public record GetConfigurationQuestionByIdResponse(
    Guid Id,
    string Question,
    string? AiPrompt,
    RuleType RuleType,
    bool IsActive,
    int Version,
    List<ConfigurationAnswerDto> Answers,
    List<DependencyRuleDto> DependencyRules
);

public record ConfigurationAnswerDto(
    Guid Id,
    string Name,
    bool IsActive,
    DateTime CreatedOn,
    string? CreatedBy
);

public record DependencyRuleDto(
    Guid Id,
    string Name,
    Guid? TriggeringAnswerId,
    string? TriggeringAnswerName,
    string? Classification,
    string? Type,
    string? ContentType,
    string? Module,
    string? QuestionBank,
    string? Tag,
    string? StatusReason,
    bool IsActive,
    DateTime CreatedOn,
    string? CreatedBy
);
