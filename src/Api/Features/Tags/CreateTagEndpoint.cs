using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

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

    public static async Task<Results<CreatedAtRoute<CreateTagResponse>, BadRequest<string>, Conflict<string>>> HandleAsync(
        CreateTagRequest request,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return TypedResults.BadRequest("Tag name is required.");
        }

        var existingTag = await db.Tags
            .FirstOrDefaultAsync(t => t.Name == request.Name, cancellationToken);
        
        if (existingTag != null) 
        {
             return TypedResults.Conflict($"Tag '{request.Name}' already exists.");
        }

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System" // TODO: Replace with real user when auth is available
        };

        db.Tags.Add(tag);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.CreatedAtRoute(new CreateTagResponse(tag.Id, tag.Name), "GetTagById", new { id = tag.Id });
    }
}
