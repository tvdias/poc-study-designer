using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Modules;

public static class GetModuleByIdEndpoint
{
    public static void MapGetModuleByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/modules/{id}", HandleAsync)
            .WithName("GetModuleById")
            .WithSummary("Get Module By Id")
            .WithTags("Modules");
    }

    public static async Task<Results<Ok<GetModuleByIdResponse>, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var module = await db.Modules
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (module is null)
        {
            return TypedResults.NotFound();
        }

        var response = new GetModuleByIdResponse(
            module.Id,
            module.VariableName,
            module.Label,
            module.Description,
            module.VersionNumber,
            module.ParentModuleId,
            module.Instructions,
            module.IsActive
        );

        return TypedResults.Ok(response);
    }
}

public record GetModuleByIdResponse(
    Guid Id,
    string VariableName,
    string Label,
    string? Description,
    int VersionNumber,
    Guid? ParentModuleId,
    string? Instructions,
    bool IsActive
);
