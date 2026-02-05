using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Modules;

public static class GetModulesEndpoint
{
    public static void MapGetModulesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/modules", HandleAsync)
            .WithName("GetModules")
            .WithSummary("Get Modules")
            .WithTags("Modules");
    }

    public static async Task<List<GetModulesResponse>> HandleAsync(
        string? query,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var modulesQuery = db.Modules.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            modulesQuery = modulesQuery.Where(m => 
                EF.Functions.ILike(m.VariableName, pattern) ||
                EF.Functions.ILike(m.Label, pattern));
        }

        return await modulesQuery
            .Select(m => new GetModulesResponse(
                m.Id,
                m.VariableName,
                m.Label,
                m.Description,
                m.VersionNumber,
                m.ParentModuleId,
                m.Status,
                m.IsActive))
            .ToListAsync(cancellationToken);
    }
}
