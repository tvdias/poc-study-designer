using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Products;

public static class GetProductByIdEndpoint
{
    public static void MapGetProductByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id}", HandleAsync)
            .WithName("GetProductById")
            .WithSummary("Get Product By Id")
            .WithTags("Products");
    }

    public static async Task<Results<Ok<GetProductDetailResponse>, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var product = await db.Products
            .Include(p => p.ProductTemplates)
            .Include(p => p.ProductConfigQuestions)
                .ThenInclude(pcq => pcq.ConfigurationQuestion)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product == null)
        {
            return TypedResults.NotFound();
        }

        var response = new GetProductDetailResponse(
            product.Id,
            product.Name,
            product.Description,
            product.IsActive,
            product.ProductTemplates
                .Select(pt => new ProductTemplateInfo(pt.Id, pt.Name, pt.Version))
                .ToList(),
            product.ProductConfigQuestions
                .Where(pcq => pcq.ConfigurationQuestion != null)
                .Select(pcq => new ProductConfigQuestionInfo(
                    pcq.Id, 
                    pcq.ConfigurationQuestionId, 
                    pcq.ConfigurationQuestion!.Question))
                .ToList()
        );

        return TypedResults.Ok(response);
    }
}
