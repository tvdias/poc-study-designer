namespace Api.Features.Questions;

public record GetQuestionsResponse(
    Guid Id,
    string VariableName,
    string QuestionType,
    string QuestionText,
    string QuestionSource,
    bool IsActive
);
