namespace Api.Features.Modules;

public record GetModulesResponse(
    Guid Id,
    string VariableName,
    string Label,
    string? Description,
    int VersionNumber,
    Guid? ParentModuleId,
    bool IsActive
);
