using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.ManagedLists;

public static class RefreshProjectSummaryEndpoint
{
    public static void MapRefreshProjectSummaryEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/subsets/project/{projectId:guid}/refresh",
            HandleAsync)
            .WithName("RefreshProjectSubsetSummary")
            .WithTags("Subsets");
    }

    public static async Task<Ok<ProjectSubsetSummaryResponse>> HandleAsync(
        Guid projectId,
        ISubsetManagementService subsetService,
        CancellationToken cancellationToken)
    {
        var response = await subsetService.RefreshProjectSummaryAsync(projectId, cancellationToken);
        return TypedResults.Ok(response);
    }
}
