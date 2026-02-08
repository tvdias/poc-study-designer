using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.Products;

public static class CreateProductConfigQuestionEndpoint
{
    public static void MapCreateProductConfigQuestionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/product-config-questions", HandleAsync)
            .WithName("CreateProductConfigQuestion")
            .WithSummary("Create Product Config Question")
            .WithTags("ProductConfigQuestions");
    }

    public static async Task<Results<CreatedAtRoute<CreateProductConfigQuestionResponse>, ValidationProblem, Conflict<string>>> HandleAsync(
        CreateProductConfigQuestionRequest request,
        ApplicationDbContext db,
        IValidator<CreateProductConfigQuestionRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var productConfigQuestion = new ProductConfigQuestion
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            ConfigurationQuestionId = request.ConfigurationQuestionId,
            StatusReason = request.StatusReason,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System" // TODO: Replace with real user when auth is available
        };

        db.ProductConfigQuestions.Add(productConfigQuestion);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
                || ex.InnerException?.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) == true)
            {
                return TypedResults.Conflict("This configuration question is already associated with this product.");
            }

            throw;
        }

        return TypedResults.CreatedAtRoute(
            new CreateProductConfigQuestionResponse(
                productConfigQuestion.Id, 
                productConfigQuestion.ProductId, 
                productConfigQuestion.ConfigurationQuestionId,
                productConfigQuestion.StatusReason), 
            "GetProductConfigQuestionById", 
            new { id = productConfigQuestion.Id });
    }
}
