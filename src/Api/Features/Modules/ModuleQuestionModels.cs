namespace Api.Features.Modules;

public record AddQuestionToModuleRequest(
    Guid QuestionId,
    int DisplayOrder
);

public record AddQuestionToModuleResponse(
    Guid Id,
    Guid ModuleId,
    Guid QuestionId,
    int DisplayOrder
);

public record ReorderModuleQuestionsRequest(
    List<QuestionOrderDto> Questions
);

public record QuestionOrderDto(
    Guid QuestionId,
    int DisplayOrder
);
