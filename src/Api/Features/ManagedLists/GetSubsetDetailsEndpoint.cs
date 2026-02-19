using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.ManagedLists;

public static class GetSubsetDetailsEndpoint
{
    public static void MapGetSubsetDetailsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/subsets/{id:guid}", async Task<Results<Ok<GetSubsetDetailsResponse>, NotFound>> (
            Guid id,
            ISubsetManagementService subsetService,
            CancellationToken cancellationToken) =>
        {
            var response = await subsetService.GetSubsetDetailsAsync(id, cancellationToken);
            
            if (response == null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(response);
        })
        .WithTags("Subsets")
        .WithName("GetSubsetDetails")
        .WithSummary("Get details of a specific subset definition including its members");
    }
}
