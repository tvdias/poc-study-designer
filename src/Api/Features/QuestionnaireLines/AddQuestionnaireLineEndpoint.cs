using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.QuestionnaireLines;

public static class AddQuestionnaireLineEndpoint
{
    public static void MapAddQuestionnaireLineEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/projects/{projectId:guid}/questionnairelines", HandleAsync)
            .WithName("AddQuestionnaireLine")
            .WithTags("QuestionnaireLines");
    }

    public static async Task<Results<Created<AddQuestionnaireLineResponse>, ValidationProblem, NotFound, Conflict<string>>> HandleAsync(
        Guid projectId,
        AddQuestionnaireLineRequest request,
        IQuestionnaireLineService questionnaireLineService,
        IValidator<AddQuestionnaireLineRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var response = await questionnaireLineService.AddQuestionnaireLineAsync(projectId, request, "system", cancellationToken);
            return TypedResults.Created($"/api/projects/{projectId}/questionnairelines/{response.Id}", response);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return TypedResults.NotFound();
            }
            return TypedResults.Conflict(ex.Message);
        }
    }
}
