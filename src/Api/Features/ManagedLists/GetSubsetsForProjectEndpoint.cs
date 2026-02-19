using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.ManagedLists;

public static class GetSubsetsForProjectEndpoint
{
    public static void MapGetSubsetsForProjectEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/subsets/project/{projectId:guid}", async Task<Ok<GetSubsetsForProjectResponse>> (
            Guid projectId,
            ISubsetManagementService subsetService,
            CancellationToken cancellationToken) =>
        {
            var response = await subsetService.GetSubsetsForProjectAsync(projectId, cancellationToken);
            return TypedResults.Ok(response);
        })
        .WithTags("Subsets")
        .WithName("GetSubsetsForProject")
        .WithSummary("Get all subset definitions for a specific project");
    }
}
