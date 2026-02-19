using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Studies;

public static class GetStudyByIdEndpoint
{
    public static void MapGetStudyByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/studies/{id}", HandleAsync)
            .WithName("GetStudyById")
            .WithSummary("Get Study details by ID")
            .WithTags("Studies");
    }

    public static async Task<Results<Ok<GetStudyDetailsResponse>, NotFound>> HandleAsync(
        Guid id,
        IStudyService studyService,
        CancellationToken cancellationToken)
    {
        var response = await studyService.GetStudyByIdAsync(id, cancellationToken);
        return response == null 
            ? TypedResults.NotFound() 
            : TypedResults.Ok(response);
    }
}
