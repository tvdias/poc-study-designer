using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.Products;

public static class CreateProductConfigQuestionDisplayRuleEndpoint
{
    public static void MapCreateProductConfigQuestionDisplayRuleEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/product-config-question-display-rules", HandleAsync)
            .WithName("CreateProductConfigQuestionDisplayRule")
            .WithSummary("Create Product Config Question Display Rule")
            .WithTags("ProductConfigQuestionDisplayRules");
    }

    public static async Task<Results<CreatedAtRoute<CreateProductConfigQuestionDisplayRuleResponse>, ValidationProblem, Conflict<string>>> HandleAsync(
        CreateProductConfigQuestionDisplayRuleRequest request,
        ApplicationDbContext db,
        IValidator<CreateProductConfigQuestionDisplayRuleRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var displayRule = new ProductConfigQuestionDisplayRule
        {
            Id = Guid.NewGuid(),
            ProductConfigQuestionId = request.ProductConfigQuestionId,
            TriggeringConfigurationQuestionId = request.TriggeringConfigurationQuestionId,
            TriggeringAnswerId = request.TriggeringAnswerId,
            DisplayCondition = request.DisplayCondition,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System"
        };

        db.ProductConfigQuestionDisplayRules.Add(displayRule);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("foreign key", StringComparison.OrdinalIgnoreCase) == true)
            {
                return TypedResults.Conflict("Referenced product config question, configuration question, or answer does not exist.");
            }

            throw;
        }

        return TypedResults.CreatedAtRoute(
            new CreateProductConfigQuestionDisplayRuleResponse(
                displayRule.Id,
                displayRule.ProductConfigQuestionId,
                displayRule.TriggeringConfigurationQuestionId,
                displayRule.TriggeringAnswerId,
                displayRule.DisplayCondition),
            "GetProductConfigQuestionDisplayRuleById",
            new { id = displayRule.Id });
    }
}
