using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using FluentValidation;

namespace Api.Features.Products;

public static class UpdateProductConfigQuestionEndpoint
{
    public static void MapUpdateProductConfigQuestionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/product-config-questions/{id}", HandleAsync)
            .WithName("UpdateProductConfigQuestion")
            .WithSummary("Update Product Config Question")
            .WithTags("ProductConfigQuestions");
    }

    public static async Task<Results<Ok<UpdateProductConfigQuestionResponse>, NotFound, ValidationProblem>> HandleAsync(
        Guid id,
        UpdateProductConfigQuestionRequest request,
        ApplicationDbContext db,
        IValidator<UpdateProductConfigQuestionRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var pcq = await db.ProductConfigQuestions.FindAsync([id], cancellationToken);

        if (pcq == null)
        {
            return TypedResults.NotFound();
        }

        pcq.StatusReason = request.StatusReason;
        pcq.IsActive = request.IsActive;
        pcq.ModifiedOn = DateTime.UtcNow;
        pcq.ModifiedBy = "System"; // TODO: Replace with real user when auth is available

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(new UpdateProductConfigQuestionResponse(
            pcq.Id, 
            pcq.ProductId, 
            pcq.ConfigurationQuestionId, 
            pcq.StatusReason, 
            pcq.IsActive));
    }
}
