using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Products;

public static class DeleteProductTemplateLineEndpoint
{
    public static void MapDeleteProductTemplateLineEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/product-template-lines/{id:guid}", HandleAsync)
            .WithName("DeleteProductTemplateLine")
            .WithSummary("Delete Product Template Line")
            .WithTags("ProductTemplateLines");
    }

    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var templateLine = await db.ProductTemplateLines.FindAsync([id], cancellationToken);
        if (templateLine is null)
        {
            return TypedResults.NotFound();
        }

        db.ProductTemplateLines.Remove(templateLine);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
