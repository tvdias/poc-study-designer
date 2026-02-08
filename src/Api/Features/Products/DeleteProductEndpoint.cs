using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Products;

public static class DeleteProductEndpoint
{
    public static void MapDeleteProductEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/{id}", HandleAsync)
            .WithName("DeleteProduct")
            .WithSummary("Delete Product")
            .WithTags("Products");
    }

    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([id], cancellationToken);

        if (product == null)
        {
            return TypedResults.NotFound();
        }

        // Soft delete by setting IsActive to false
        product.IsActive = false;
        product.ModifiedOn = DateTime.UtcNow;
        product.ModifiedBy = "System"; // TODO: Replace with real user when auth is available

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
