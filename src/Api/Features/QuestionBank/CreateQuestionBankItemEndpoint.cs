using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.QuestionBank;

public static class CreateQuestionBankItemEndpoint
{
    public static void MapCreateQuestionBankItemEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/question-bank", HandleAsync)
            .WithName("CreateQuestionBankItem")
            .WithSummary("Create Question Bank Item")
            .WithTags("QuestionBank");
    }

    public static async Task<Results<CreatedAtRoute<CreateQuestionBankItemResponse>, ValidationProblem, Conflict<string>>> HandleAsync(
        CreateQuestionBankItemRequest request,
        IQuestionBankService questionBankService,
        IValidator<CreateQuestionBankItemRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var response = await questionBankService.CreateQuestionBankItemAsync(request, "System", cancellationToken);
            return TypedResults.CreatedAtRoute(response, "GetQuestionBankItemById", new { id = response.Id });
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.Conflict(ex.Message);
        }
    }
}
