using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
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
        ApplicationDbContext db,
        IValidator<UpdateTagRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var tag = await db.Tags.FindAsync([id], cancellationToken);

        if (tag is null)
        {
            return TypedResults.NotFound();
        }

        tag.Name = request.Name;
        tag.ModifiedOn = DateTime.UtcNow;
        tag.ModifiedBy = "System"; // TODO: Replace with real user

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(new UpdateTagResponse(tag.Id, tag.Name));
    }
}
