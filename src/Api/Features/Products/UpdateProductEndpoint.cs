using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.Products;

public static class UpdateProductEndpoint
{
    public static void MapUpdateProductEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/products/{id}", HandleAsync)
            .WithName("UpdateProduct")
            .WithSummary("Update Product")
            .WithTags("Products");
    }

    public static async Task<Results<Ok<UpdateProductResponse>, NotFound, ValidationProblem, Conflict<string>>> HandleAsync(
        Guid id,
        UpdateProductRequest request,
        ApplicationDbContext db,
        IValidator<UpdateProductRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var product = await db.Products.FindAsync([id], cancellationToken);

        if (product == null)
        {
            return TypedResults.NotFound();
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.IsActive = request.IsActive;
        product.ModifiedOn = DateTime.UtcNow;
        product.ModifiedBy = "System"; // TODO: Replace with real user when auth is available

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
                || ex.InnerException?.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) == true)
            {
                return TypedResults.Conflict($"Product '{request.Name}' already exists.");
            }

            throw;
        }

        return TypedResults.Ok(new UpdateProductResponse(product.Id, product.Name, product.Description, product.IsActive));
    }
}
