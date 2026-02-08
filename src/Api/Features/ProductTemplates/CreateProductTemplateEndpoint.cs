using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Api.Features.Products;

namespace Api.Features.ProductTemplates;

public static class CreateProductTemplateEndpoint
{
    public static void MapCreateProductTemplateEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/product-templates", HandleAsync)
            .WithName("CreateProductTemplate")
            .WithSummary("Create Product Template")
            .WithTags("ProductTemplates");
    }

    public static async Task<Results<CreatedAtRoute<CreateProductTemplateResponse>, ValidationProblem, Conflict<string>>> HandleAsync(
        CreateProductTemplateRequest request,
        ApplicationDbContext db,
        IValidator<CreateProductTemplateRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var productTemplate = new ProductTemplate
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Version = request.Version,
            ProductId = request.ProductId,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System" // TODO: Replace with real user when auth is available
        };

        db.ProductTemplates.Add(productTemplate);

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

        return TypedResults.CreatedAtRoute(
            new CreateProductTemplateResponse(productTemplate.Id, productTemplate.Name, productTemplate.Version, productTemplate.ProductId), 
            "GetProductTemplateById", 
            new { id = productTemplate.Id });
    }
}
