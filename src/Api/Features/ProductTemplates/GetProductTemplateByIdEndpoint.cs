using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Api.Features.Products;

namespace Api.Features.ProductTemplates;

public static class GetProductTemplateByIdEndpoint
{
    public static void MapGetProductTemplateByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/product-templates/{id}", HandleAsync)
            .WithName("GetProductTemplateById")
            .WithSummary("Get Product Template By Id")
            .WithTags("ProductTemplates");
    }

    public static async Task<Results<Ok<GetProductTemplatesResponse>, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var template = await db.ProductTemplates
            .AsNoTracking()
            .Where(pt => pt.IsActive)
            .Where(pt => pt.Id == id)
            .Include(pt => pt.Product)
            .FirstOrDefaultAsync(cancellationToken);

        if (template == null || template.Product == null)
        {
            return TypedResults.NotFound();
        }

        var response = new GetProductTemplatesResponse(
            template.Id,
            template.Name,
            template.Version,
            template.ProductId,
            template.Product.Name
        );

        return TypedResults.Ok(response);
    }
}
