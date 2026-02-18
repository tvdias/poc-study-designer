using Api.Features.ManagedLists.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Features.ManagedLists;

public static class SaveQuestionSelectionEndpoint
{
    public static void MapSaveQuestionSelectionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/subsets/save-selection", async Task<Results<Ok<SaveQuestionSelectionResponse>, ValidationProblem, BadRequest<string>>> (
            [FromBody] SaveQuestionSelectionRequest request,
            [FromServices] IValidator<SaveQuestionSelectionRequest> validator,
            [FromServices] ISubsetManagementService subsetService,
            CancellationToken cancellationToken) =>
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
        })
        .WithTags("Subsets")
        .WithName("SaveQuestionSelection")
        .WithSummary("Save question selection and automatically create or reuse subset definitions");
    }
}
