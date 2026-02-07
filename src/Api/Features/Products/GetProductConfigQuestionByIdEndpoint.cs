using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Products;

public static class GetProductConfigQuestionByIdEndpoint
{
    public static void MapGetProductConfigQuestionByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/product-config-questions/{id}", HandleAsync)
            .WithName("GetProductConfigQuestionById")
            .WithSummary("Get Product Config Question By Id")
            .WithTags("ProductConfigQuestions");
    }

    public static async Task<Results<Ok<ProductConfigQuestionInfo>, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var pcq = await db.ProductConfigQuestions
            .Include(p => p.ConfigurationQuestion)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (pcq == null || pcq.ConfigurationQuestion == null)
        {
            return TypedResults.NotFound();
        }

        var response = new ProductConfigQuestionInfo(
            pcq.Id,
            pcq.ConfigurationQuestionId,
            pcq.ConfigurationQuestion.Question,
            pcq.StatusReason
        );

        return TypedResults.Ok(response);
    }
}
