using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Studies;

public static class GetStudyQuestionsEndpoint
{
    public static void MapGetStudyQuestionsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/studies/{id}/questions", HandleAsync)
            .WithName("GetStudyQuestions")
            .WithSummary("Get Study questionnaire lines")
            .WithTags("Studies");
    }

    public static async Task<Results<Ok<GetStudyQuestionsResponse>, NotFound>> HandleAsync(
        Guid id,
        IStudyService studyService,
        CancellationToken cancellationToken)
    {
        var response = await studyService.GetStudyQuestionsAsync(id, cancellationToken);
        return response == null 
            ? TypedResults.NotFound() 
            : TypedResults.Ok(response);
    }
}
