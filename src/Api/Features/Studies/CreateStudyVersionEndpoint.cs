using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Studies;

public static class CreateStudyVersionEndpoint
{
    public static void MapCreateStudyVersionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/studies/{parentStudyId}/versions", HandleAsync)
            .WithName("CreateStudyVersion")
            .WithSummary("Create a new version of an existing Study")
            .WithTags("Studies");
    }

    public static async Task<Results<CreatedAtRoute<CreateStudyVersionResponse>, Conflict<string>>> HandleAsync(
        Guid parentStudyId,
        IStudyService studyService,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await studyService.CreateStudyVersionAsync(parentStudyId, "System", cancellationToken);
            return TypedResults.CreatedAtRoute(response, "GetStudyById", new { id = response.StudyId });
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.Conflict(ex.Message);
        }
    }
}
