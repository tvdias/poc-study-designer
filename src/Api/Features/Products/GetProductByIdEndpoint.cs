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
        var product = await db.Products.AsNoTracking()
            .Where(p => p.IsActive)
            .Where(p => p.Id == id)
            .Include(p => p.ProductTemplates)
            .Include(p => p.ProductConfigQuestions)
                .ThenInclude(pcq => pcq.ConfigurationQuestion)
            .FirstOrDefaultAsync(cancellationToken);

        if (product == null)
        {
            return TypedResults.NotFound();
        }

        var response = new GetProductDetailResponse(
            product.Id,
            product.Name,
            product.Description,
            [.. product.ProductTemplates
                .Where(pt => pt.IsActive)
                .Select(pt => new ProductTemplateInfo(pt.Id, pt.Name, pt.Version))],
            [.. product.ProductConfigQuestions
                .Where(pcq => pcq.IsActive && pcq.ConfigurationQuestion != null)
                .Select(pcq => new ProductConfigQuestionInfo(
                    pcq.Id, 
                    pcq.ConfigurationQuestionId, 
                    pcq.ConfigurationQuestion!.Question))]
        );

        return TypedResults.Ok(response);
    }
}
