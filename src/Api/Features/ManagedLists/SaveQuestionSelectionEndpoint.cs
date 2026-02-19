using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.ManagedLists;

public static class SaveQuestionSelectionEndpoint
{
    public static void MapSaveQuestionSelectionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/subsets/save-selection", HandleAsync)
            .WithName("SaveQuestionSelection")
            .WithSummary("Save question selection and automatically create or reuse subset definitions")
            .WithTags("Subsets");
    }

    public static async Task<Results<Ok<SaveQuestionSelectionResponse>, ValidationProblem, BadRequest<string>>> HandleAsync(
        SaveQuestionSelectionRequest request,
        IValidator<SaveQuestionSelectionRequest> validator,
        ISubsetManagementService subsetService,
        CancellationToken cancellationToken)
    {
        // Validate request
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            // TODO: Get actual user ID from authentication context
            var userId = "system";
            
            var response = await subsetService.SaveQuestionSelectionAsync(request, userId, cancellationToken);
            return TypedResults.Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }
}
