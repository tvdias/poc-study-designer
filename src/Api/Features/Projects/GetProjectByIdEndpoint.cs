using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Projects;

public record GetProjectByIdResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? ClientId,
    string? ClientName,
    Guid? CommissioningMarketId,
    string? CommissioningMarketName,
    Methodology? Methodology,
    Guid? ProductId,
    string? ProductName,
    string? Owner,
    ProjectStatus Status,
    bool CostManagementEnabled,
    DateTime CreatedOn,
    string CreatedBy,
    DateTime? ModifiedOn,
    string? ModifiedBy
);

public static class GetProjectByIdEndpoint
{
    public static void MapGetProjectByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/projects/{id:guid}", async Task<Results<Ok<GetProjectByIdResponse>, NotFound>> (
            Guid id,
            ApplicationDbContext db,
            CancellationToken ct) =>
        {
            var project = await db.Projects
                .Include(p => p.Client)
                .Include(p => p.CommissioningMarket)
                .Include(p => p.Product)
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            if (project == null)
            {
                return TypedResults.NotFound();
            }

            var response = new GetProjectByIdResponse(
                project.Id,
                project.Name,
                project.Description,
                project.ClientId,
                project.Client?.AccountName,
                project.CommissioningMarketId,
                project.CommissioningMarket?.Name,
                project.Methodology,
                project.ProductId,
                project.Product?.Name,
                project.Owner,
                project.Status,
                project.CostManagementEnabled,
                project.CreatedOn,
                project.CreatedBy,
                project.ModifiedOn,
                project.ModifiedBy
            );

            return TypedResults.Ok(response);
        })
        .WithName("GetProjectById")
        .WithOpenApi();
    }
}
