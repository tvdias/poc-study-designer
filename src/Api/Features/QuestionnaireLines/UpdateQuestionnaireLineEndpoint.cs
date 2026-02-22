using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.QuestionnaireLines;

public static class UpdateQuestionnaireLineEndpoint
{
    public static void MapUpdateQuestionnaireLineEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/projects/{projectId:guid}/questionnairelines/{id:guid}", HandleAsync)
            .WithName("UpdateQuestionnaireLine")
            .WithTags("QuestionnaireLines");
    }

    public static async Task<Results<Ok<QuestionnaireLineDto>, ValidationProblem, NotFound>> HandleAsync(
        Guid projectId,
        Guid id,
        UpdateQuestionnaireLineRequest request,
        IQuestionnaireLineService questionnaireLineService,
        IValidator<UpdateQuestionnaireLineRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var response = await questionnaireLineService.UpdateQuestionnaireLineAsync(projectId, id, request, "system", cancellationToken);
            return TypedResults.Ok(response);
        }
        catch (InvalidOperationException)
        {
            return TypedResults.NotFound();
        }
    }
}
