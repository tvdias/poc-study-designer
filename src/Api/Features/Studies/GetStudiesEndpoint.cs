using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Studies;

public static class GetStudiesEndpoint
{
    public static void MapGetStudiesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/studies", HandleAsync)
            .WithName("GetStudies")
            .WithSummary("Get all Studies for a Project")
            .WithTags("Studies");
    }

    public static async Task<Ok<GetStudiesResponse>> HandleAsync(
        Guid projectId,
        IStudyService studyService,
        CancellationToken cancellationToken)
    {
        var response = await studyService.GetStudiesAsync(projectId, cancellationToken);
        return TypedResults.Ok(response);
    }
}
