using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Api.Features.Products;

namespace Api.Features.ProductTemplates;

public static class UpdateProductTemplateEndpoint
{
    public static void MapUpdateProductTemplateEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/product-templates/{id}", HandleAsync)
            .WithName("UpdateProductTemplate")
            .WithSummary("Update Product Template")
            .WithTags("ProductTemplates");
    }

    public static async Task<Results<Ok<UpdateProductTemplateResponse>, NotFound, ValidationProblem, Conflict<string>>> HandleAsync(
        Guid id,
        UpdateProductTemplateRequest request,
        ApplicationDbContext db,
        IValidator<UpdateProductTemplateRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var template = await db.ProductTemplates.FindAsync([id], cancellationToken);

        if (template == null)
        {
            return TypedResults.NotFound();
        }

        template.Name = request.Name;
        template.Version = request.Version;
        template.ProductId = request.ProductId;
        template.IsActive = request.IsActive;
        template.ModifiedOn = DateTime.UtcNow;
        template.ModifiedBy = "System"; // TODO: Replace with real user when auth is available

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
                || ex.InnerException?.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) == true)
            {
                return TypedResults.Conflict($"Product template '{request.Name}' with version {request.Version} already exists for this product.");
            }

            throw;
        }

        return TypedResults.Ok(new UpdateProductTemplateResponse(template.Id, template.Name, template.Version, template.ProductId, template.IsActive));
    }
}
