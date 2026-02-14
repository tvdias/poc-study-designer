using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.Products;

public static class UpdateProductConfigQuestionDisplayRuleEndpoint
{
    public static void MapUpdateProductConfigQuestionDisplayRuleEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/product-config-question-display-rules/{id:guid}", HandleAsync)
            .WithName("UpdateProductConfigQuestionDisplayRule")
            .WithSummary("Update Product Config Question Display Rule")
            .WithTags("ProductConfigQuestionDisplayRules");
    }

    public static async Task<Results<Ok<ProductConfigQuestionDisplayRuleInfo>, ValidationProblem, NotFound, Conflict<string>>> HandleAsync(
        Guid id,
        UpdateProductConfigQuestionDisplayRuleRequest request,
        ApplicationDbContext db,
        IValidator<UpdateProductConfigQuestionDisplayRuleRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var displayRule = await db.ProductConfigQuestionDisplayRules.FindAsync([id], cancellationToken);
        if (displayRule is null)
        {
            return TypedResults.NotFound();
        }

        displayRule.TriggeringConfigurationQuestionId = request.TriggeringConfigurationQuestionId;
        displayRule.TriggeringAnswerId = request.TriggeringAnswerId;
        displayRule.DisplayCondition = request.DisplayCondition;
        displayRule.IsActive = request.IsActive;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("foreign key", StringComparison.OrdinalIgnoreCase) == true)
            {
                return TypedResults.Conflict("Referenced configuration question or answer does not exist.");
            }

            throw;
        }

        var updatedRule = await db.ProductConfigQuestionDisplayRules
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
            .FirstAsync(cancellationToken);

        return TypedResults.Ok(updatedRule);
    }
}
