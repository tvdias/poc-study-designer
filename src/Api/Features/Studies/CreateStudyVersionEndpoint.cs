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

    public static async Task<Results<CreatedAtRoute<CreateStudyVersionResponse>, ValidationProblem, Conflict<string>>> HandleAsync(
        Guid parentStudyId,
        CreateStudyVersionRequest request,
        IStudyService studyService,
        IValidator<CreateStudyVersionRequest> validator,
        CancellationToken cancellationToken)
    {
        // Merge parentStudyId from route into request
        var mergedRequest = request with { ParentStudyId = parentStudyId };

        var validationResult = await validator.ValidateAsync(mergedRequest, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var response = await studyService.CreateStudyVersionAsync(mergedRequest, "System", cancellationToken);
            return TypedResults.CreatedAtRoute(response, "GetStudyById", new { id = response.StudyId });
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.Conflict(ex.Message);
        }
    }
}
