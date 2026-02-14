using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Products;

public static class DeleteProductConfigQuestionDisplayRuleEndpoint
{
    public static void MapDeleteProductConfigQuestionDisplayRuleEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/product-config-question-display-rules/{id:guid}", HandleAsync)
            .WithName("DeleteProductConfigQuestionDisplayRule")
            .WithSummary("Delete Product Config Question Display Rule")
            .WithTags("ProductConfigQuestionDisplayRules");
    }

    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var displayRule = await db.ProductConfigQuestionDisplayRules.FindAsync([id], cancellationToken);
        if (displayRule is null)
        {
            return TypedResults.NotFound();
        }

        db.ProductConfigQuestionDisplayRules.Remove(displayRule);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
