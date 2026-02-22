using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.QuestionBank;

public static class CreateQuestionAnswerEndpoint
{
    public static void MapCreateQuestionAnswerEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/question-bank/{questionId:guid}/answers", HandleAsync)
            .WithName("CreateQuestionAnswer")
            .WithSummary("Create Question Answer")
            .WithTags("QuestionBank");
    }

    public static async Task<Results<CreatedAtRoute<CreateQuestionAnswerResponse>, ValidationProblem, NotFound, Conflict<string>>> HandleAsync(
        Guid questionId,
        CreateQuestionAnswerRequest request,
        IQuestionBankService questionBankService,
        IValidator<CreateQuestionAnswerRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var response = await questionBankService.CreateQuestionAnswerAsync(questionId, request, "System", cancellationToken);
            return TypedResults.CreatedAtRoute(response, "GetQuestionBankItemById", new { id = questionId });
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
