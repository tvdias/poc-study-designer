namespace Api.Features.Modules;

public record UpdateModuleRequest(
    string VariableName,
    string Label,
    string? Description,
    Guid? ParentModuleId,
    string? Instructions,
    string Status,
    string? StatusReason,
    bool IsActive
);

public record UpdateModuleResponse(
    Guid Id,
    string VariableName,
    string Label,
    string? Description,
    int VersionNumber,
    Guid? ParentModuleId,
    string? Instructions,
    string Status,
    string? StatusReason,
    bool IsActive
);
