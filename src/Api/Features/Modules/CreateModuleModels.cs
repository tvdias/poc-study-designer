namespace Api.Features.Modules;

public record CreateModuleRequest(
    string VariableName,
    string Label,
    string? Description,
    Guid? ParentModuleId,
    string? Instructions,
    string Status,
    string? StatusReason
);

public record CreateModuleResponse(
    Guid Id,
    string VariableName,
    string Label,
    string? Description,
    int VersionNumber,
    Guid? ParentModuleId,
    string? Instructions,
    string Status,
    string? StatusReason
);
