using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Products;

public static class GetProductsEndpoint
{
    public static void MapGetProductsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/products", HandleAsync)
            .WithName("GetProducts")
            .WithSummary("Get Products")
            .WithTags("Products");
    }

    public static async Task<Ok<List<GetProductsResponse>>> HandleAsync(
        ApplicationDbContext db,
        string? query,
        CancellationToken cancellationToken)
    {
        var productsQuery = db.Products.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(query))
        {
            productsQuery = productsQuery.Where(p => EF.Functions.ILike(p.Name, $"%{query}%"));
        }

        var products = await productsQuery
            .OrderBy(p => p.Name)
            .Select(p => new GetProductsResponse(p.Id, p.Name, p.Description))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(products);
    }
}
