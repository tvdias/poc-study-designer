using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Products;

public static class GetProductConfigQuestionDisplayRuleByIdEndpoint
{
    public static void MapGetProductConfigQuestionDisplayRuleByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/product-config-question-display-rules/{id:guid}", HandleAsync)
            .WithName("GetProductConfigQuestionDisplayRuleById")
            .WithSummary("Get Product Config Question Display Rule by ID")
            .WithTags("ProductConfigQuestionDisplayRules");
    }

    public static async Task<Results<Ok<ProductConfigQuestionDisplayRuleInfo>, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var displayRule = await db.ProductConfigQuestionDisplayRules
            .Include(dr => dr.TriggeringConfigurationQuestion)
            .Include(dr => dr.TriggeringAnswer)
            .Where(dr => dr.Id == id)
            .Select(dr => new ProductConfigQuestionDisplayRuleInfo(
                dr.Id,
                dr.ProductConfigQuestionId,
                dr.TriggeringConfigurationQuestionId,
                dr.TriggeringAnswerId,
                dr.DisplayCondition,
                dr.IsActive,
                dr.CreatedOn,
                dr.TriggeringConfigurationQuestion != null ? dr.TriggeringConfigurationQuestion.Question : null,
                dr.TriggeringAnswer != null ? dr.TriggeringAnswer.Name : null))
            .FirstOrDefaultAsync(cancellationToken);

        return displayRule is not null
            ? TypedResults.Ok(displayRule)
            : TypedResults.NotFound();
    }
}
