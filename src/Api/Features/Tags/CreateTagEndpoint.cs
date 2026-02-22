using Microsoft.AspNetCore.Http.HttpResults;
using FluentValidation;

namespace Api.Features.Tags;

public static class CreateTagEndpoint
{
    public static void MapCreateTagEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/tags", HandleAsync)
            .WithName("CreateTag")
            .WithSummary("Create Tag")
            .WithTags("Tags");
    }

    public static async Task<Results<CreatedAtRoute<CreateTagResponse>, ValidationProblem, Conflict<string>>> HandleAsync(
        CreateTagRequest request,
        ITagService tagService,
        IValidator<CreateTagRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var response = await tagService.CreateTagAsync(request, "System", cancellationToken);
            return TypedResults.CreatedAtRoute(response, "GetTagById", new { id = response.Id });
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.Conflict(ex.Message);
        }
    }
}
