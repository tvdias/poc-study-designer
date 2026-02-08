using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Api.Features.Products;

namespace Api.Features.ProductTemplates;

public static class GetProductTemplatesEndpoint
{
    public static void MapGetProductTemplatesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/product-templates", HandleAsync)
            .WithName("GetProductTemplates")
            .WithSummary("Get Product Templates")
            .WithTags("ProductTemplates");
    }

    public static async Task<Ok<List<GetProductTemplatesResponse>>> HandleAsync(
        ApplicationDbContext db,
        string? query,
        Guid? productId,
        CancellationToken cancellationToken)
    {
        var templatesQuery = db.ProductTemplates
            .Include(pt => pt.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            templatesQuery = templatesQuery.Where(pt => EF.Functions.ILike(pt.Name, $"%{query}%"));
        }

        if (productId.HasValue)
        {
            templatesQuery = templatesQuery.Where(pt => pt.ProductId == productId.Value);
        }

        var templates = await templatesQuery
            .OrderBy(pt => pt.Product!.Name)
            .ThenBy(pt => pt.Name)
            .Select(pt => new GetProductTemplatesResponse(
                pt.Id, 
                pt.Name, 
                pt.Version, 
                pt.ProductId, 
                pt.Product!.Name, 
                pt.IsActive))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(templates);
    }
}
