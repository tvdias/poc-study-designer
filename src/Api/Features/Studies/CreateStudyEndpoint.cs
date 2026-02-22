using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Studies;

public static class CreateStudyEndpoint
{
    public static void MapCreateStudyEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/studies", HandleAsync)
            .WithName("CreateStudy")
            .WithSummary("Create a new Study (Version 1)")
            .WithTags("Studies");
    }

    public static async Task<Results<CreatedAtRoute<CreateStudyResponse>, ValidationProblem, ProblemHttpResult>> HandleAsync(
        CreateStudyRequest request,
        IStudyService studyService,
        IValidator<CreateStudyRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var response = await studyService.CreateStudyV1Async(request, "System", cancellationToken);
            return TypedResults.CreatedAtRoute(response, "GetStudyById", new { id = response.StudyId });
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status409Conflict,
                title: "Project Configuration Conflict"
            );
        }
    }
}
