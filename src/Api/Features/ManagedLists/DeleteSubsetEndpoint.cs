using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace Api.Features.ManagedLists;

public static class DeleteSubsetEndpoint
{
    public static void MapDeleteSubsetEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/subsets/{id:guid}",
            HandleAsync)
            .WithName("DeleteSubset")
            .WithTags("Subsets");
    }

    public static async Task<Results<Ok<DeleteSubsetResponse>, NotFound, BadRequest<string>>> HandleAsync(
        Guid id,
        ISubsetManagementService subsetService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            var response = await subsetService.DeleteSubsetAsync(id, userId, cancellationToken);
            return TypedResults.Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }
}
