using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.QuestionBank;

public static class UpdateQuestionBankItemEndpoint
{
    public static void MapUpdateQuestionBankItemEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/question-bank/{id:guid}", HandleAsync)
            .WithName("UpdateQuestionBankItem")
            .WithSummary("Update Question Bank Item")
            .WithTags("QuestionBank");
    }

    public static async Task<Results<Ok<UpdateQuestionBankItemResponse>, ValidationProblem, NotFound, Conflict<string>>> HandleAsync(
        Guid id,
        UpdateQuestionBankItemRequest request,
        IQuestionBankService questionBankService,
        IValidator<UpdateQuestionBankItemRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var response = await questionBankService.UpdateQuestionBankItemAsync(id, request, "System", cancellationToken);
            return TypedResults.Ok(response);
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
