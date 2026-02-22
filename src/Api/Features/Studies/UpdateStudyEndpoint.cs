using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Studies;

public static class UpdateStudyEndpoint
{
    public static void MapUpdateStudyEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/studies/{studyId:guid}", HandleAsync)
            .WithName("UpdateStudy")
            .WithSummary("Update study name, description, or status")
            .WithTags("Studies");
    }

    public static async Task<Results<Ok, NotFound, ValidationProblem, ProblemHttpResult>> HandleAsync(
        Guid studyId,
        UpdateStudyRequest request,
        IStudyService studyService,
        IValidator<UpdateStudyRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            await studyService.UpdateStudyAsync(studyId, request, "System", cancellationToken);
            return TypedResults.Ok();
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return TypedResults.NotFound();
            }
            return TypedResults.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status409Conflict,
                title: "Study Update Conflict"
            );
        }
    }
}
