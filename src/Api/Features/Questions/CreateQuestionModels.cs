namespace Api.Features.Questions;

public record CreateQuestionRequest(
    string VariableName,
    string QuestionType,
    string QuestionText,
    string QuestionSource
);

public record CreateQuestionResponse(
    Guid Id,
    string VariableName,
    string QuestionType,
    string QuestionText,
    string QuestionSource
);
