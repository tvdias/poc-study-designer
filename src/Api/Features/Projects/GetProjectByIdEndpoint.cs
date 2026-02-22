using Microsoft.AspNetCore.Http.HttpResults;

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
    bool HasStudies,
    int StudyCount,
    DateTime? LastStudyModifiedOn,
    int QuestionnaireLineCount,
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
            IProjectService projectService,
            CancellationToken ct) =>
        {
            var response = await projectService.GetProjectByIdAsync(id, ct);

            if (response == null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(response);
        })
        .WithName("GetProjectById")
        .WithOpenApi();
    }
}
