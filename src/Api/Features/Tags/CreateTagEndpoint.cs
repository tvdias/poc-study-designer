using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
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
        ApplicationDbContext db,
        IValidator<CreateTagRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System" // TODO: Replace with real user when auth is available
        };

        db.Tags.Add(tag);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            // Different database providers use different error codes for unique constraint violations.
            // This is a generic check that looks for common keywords in the inner exception.
            if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
                || ex.InnerException?.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) == true)
            {
                return TypedResults.Conflict($"Tag '{request.Name}' already exists.");
            }

            throw;
        }

        return TypedResults.CreatedAtRoute(new CreateTagResponse(tag.Id, tag.Name), "GetTagById", new { id = tag.Id });
    }
}
