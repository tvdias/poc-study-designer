using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Modules;

public static class GetModuleVersionsEndpoint
{
    public static void MapGetModuleVersionsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/modules/{moduleId}/versions", HandleAsync)
            .WithName("GetModuleVersions")
            .WithSummary("Get Module Versions")
            .WithTags("Modules");
    }

    public static async Task<List<ModuleVersionDto>> HandleAsync(
        Guid moduleId,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        return await db.ModuleVersions
            .AsNoTracking()
            .Where(v => v.ModuleId == moduleId)
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new ModuleVersionDto(
                v.Id,
                v.VersionNumber,
                v.ChangeDescription,
                v.CreatedOn,
                v.CreatedBy ?? "System"))
            .ToListAsync(cancellationToken);
    }
}

public record ModuleVersionDto(
    Guid Id,
    int VersionNumber,
    string? ChangeDescription,
    DateTime CreatedOn,
    string CreatedBy
);
