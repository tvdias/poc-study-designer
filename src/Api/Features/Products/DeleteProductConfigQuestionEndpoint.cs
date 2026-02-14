using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Products;

public static class DeleteProductConfigQuestionEndpoint
{
    public static void MapDeleteProductConfigQuestionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/product-config-questions/{id}", HandleAsync)
            .WithName("DeleteProductConfigQuestion")
            .WithSummary("Delete Product Config Question")
            .WithTags("ProductConfigQuestions");
    }

    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var pcq = await db.ProductConfigQuestions.FindAsync([id], cancellationToken);

        if (pcq == null)
        {
            return TypedResults.NotFound();
        }

        // Soft delete by setting IsActive to false
        pcq.IsActive = false;
        pcq.ModifiedOn = DateTime.UtcNow;
        pcq.ModifiedBy = "System"; // TODO: Replace with real user when auth is available

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
