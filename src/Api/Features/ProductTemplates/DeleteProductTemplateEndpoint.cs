using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Api.Features.Products;

namespace Api.Features.ProductTemplates;

public static class DeleteProductTemplateEndpoint
{
    public static void MapDeleteProductTemplateEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/product-templates/{id}", HandleAsync)
            .WithName("DeleteProductTemplate")
            .WithSummary("Delete Product Template")
            .WithTags("ProductTemplates");
    }

    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var template = await db.ProductTemplates.FindAsync([id], cancellationToken);

        if (template == null)
        {
            return TypedResults.NotFound();
        }

        // Soft delete by setting IsActive to false
        template.IsActive = false;
        template.ModifiedOn = DateTime.UtcNow;
        template.ModifiedBy = "System"; // TODO: Replace with real user when auth is available

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
