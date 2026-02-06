namespace Api.Features.ConfigurationQuestions;

// Create Configuration Answer
public record CreateConfigurationAnswerRequest(
    string Name,
    Guid ConfigurationQuestionId
);

public record CreateConfigurationAnswerResponse(
    Guid Id,
    string Name,
    Guid ConfigurationQuestionId,
    bool IsActive
);

// Update Configuration Answer
public record UpdateConfigurationAnswerRequest(
    string Name,
    bool IsActive
);

public record UpdateConfigurationAnswerResponse(
    Guid Id,
    string Name,
    Guid ConfigurationQuestionId,
    bool IsActive
);

// Get Configuration Answers
public record GetConfigurationAnswersResponse(
    Guid Id,
    string Name,
    Guid ConfigurationQuestionId,
    string QuestionText,
    bool IsActive
);
