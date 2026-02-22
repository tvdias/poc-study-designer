using Microsoft.AspNetCore.Http.HttpResults;
using FluentValidation;

namespace Api.Features.Tags;

public static class UpdateTagEndpoint
{
    public static void MapUpdateTagEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/tags/{id}", HandleAsync)
            .WithName("UpdateTag")
            .WithSummary("Update Tag")
            .WithTags("Tags");
    }

    public static async Task<Results<Ok<UpdateTagResponse>, NotFound, ValidationProblem>> HandleAsync(
        Guid id,
        UpdateTagRequest request,
        ITagService tagService,
        IValidator<UpdateTagRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var response = await tagService.UpdateTagAsync(id, request, "System", cancellationToken);
            return TypedResults.Ok(response);
        }
        catch (InvalidOperationException)
        {
            return TypedResults.NotFound();
        }
    }
}
