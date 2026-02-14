using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Products;

public static class GetProductConfigQuestionDisplayRulesEndpoint
{
    public static void MapGetProductConfigQuestionDisplayRulesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/product-config-question-display-rules", HandleAsync)
            .WithName("GetProductConfigQuestionDisplayRules")
            .WithSummary("Get Product Config Question Display Rules")
            .WithTags("ProductConfigQuestionDisplayRules");
    }

    public static async Task<Ok<List<ProductConfigQuestionDisplayRuleInfo>>> HandleAsync(
        ApplicationDbContext db,
        Guid? productConfigQuestionId,
        CancellationToken cancellationToken)
    {
        var query = db.ProductConfigQuestionDisplayRules
            .Include(dr => dr.TriggeringConfigurationQuestion)
            .Include(dr => dr.TriggeringAnswer)
            .AsQueryable();

        if (productConfigQuestionId.HasValue)
        {
            query = query.Where(dr => dr.ProductConfigQuestionId == productConfigQuestionId.Value);
        }

        var displayRules = await query
            .OrderBy(dr => dr.CreatedOn)
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
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(displayRules);
    }
}
