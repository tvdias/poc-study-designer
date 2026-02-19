using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.ManagedLists;

public static class GetSubsetDetailsEndpoint
{
    public static void MapGetSubsetDetailsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/subsets/{id:guid}", HandleAsync)
            .WithName("GetSubsetDetails")
            .WithSummary("Get details of a specific subset definition")
            .WithTags("Subsets");
    }
    
    public static async Task<Results<Ok<GetSubsetDetailsResponse>, NotFound>> HandleAsync(
        Guid id,
        ISubsetManagementService subsetService,
        CancellationToken cancellationToken)
    {
        var response = await subsetService.GetSubsetDetailsAsync(id, cancellationToken);
        
        if (response == null)
        {
            return TypedResults.NotFound();
        }
        
        return TypedResults.Ok(response);
    }
}
