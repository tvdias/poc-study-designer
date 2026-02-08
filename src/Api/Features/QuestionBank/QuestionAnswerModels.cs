namespace Api.Features.QuestionBank;

// Response model for answers
public record QuestionAnswerResponse(
    Guid Id,
    string AnswerText,
    string? AnswerCode,
    string? AnswerLocation,
    bool IsOpen,
    bool IsFixed,
    bool IsExclusive,
    bool IsActive,
    string? CustomProperty,
    string? Facets,
    int Version,
    int? DisplayOrder,
    DateTime CreatedOn,
    string? CreatedBy
);

// Create answer
public record CreateQuestionAnswerRequest(
    string AnswerText,
    string? AnswerCode,
    string? AnswerLocation,
    bool IsOpen,
    bool IsFixed,
    bool IsExclusive,
    bool IsActive,
    string? CustomProperty,
    string? Facets,
    int Version,
    int? DisplayOrder
);

public record CreateQuestionAnswerResponse(
    Guid Id,
    string AnswerText
);

// Update answer
public record UpdateQuestionAnswerRequest(
    string AnswerText,
    string? AnswerCode,
    string? AnswerLocation,
    bool IsOpen,
    bool IsFixed,
    bool IsExclusive,
    bool IsActive,
    string? CustomProperty,
    string? Facets,
    int Version,
    int? DisplayOrder
);

public record UpdateQuestionAnswerResponse(
    Guid Id,
    string AnswerText
);
