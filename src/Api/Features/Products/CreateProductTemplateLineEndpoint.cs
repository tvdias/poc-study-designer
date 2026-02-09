using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.Products;

public static class CreateProductTemplateLineEndpoint
{
    public static void MapCreateProductTemplateLineEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/product-template-lines", HandleAsync)
            .WithName("CreateProductTemplateLine")
            .WithSummary("Create Product Template Line")
            .WithTags("ProductTemplateLines");
    }

    public static async Task<Results<CreatedAtRoute<CreateProductTemplateLineResponse>, ValidationProblem, Conflict<string>>> HandleAsync(
        CreateProductTemplateLineRequest request,
        ApplicationDbContext db,
        IValidator<CreateProductTemplateLineRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var templateLine = new ProductTemplateLine
        {
            Id = Guid.NewGuid(),
            ProductTemplateId = request.ProductTemplateId,
            Name = request.Name,
            Type = request.Type,
            IncludeByDefault = request.IncludeByDefault,
            SortOrder = request.SortOrder,
            ModuleId = request.ModuleId,
            QuestionBankItemId = request.QuestionBankItemId,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System"
        };

        db.ProductTemplateLines.Add(templateLine);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("foreign key", StringComparison.OrdinalIgnoreCase) == true)
            {
                return TypedResults.Conflict("Referenced product template, module, or question bank item does not exist.");
            }

            throw;
        }

        return TypedResults.CreatedAtRoute(
            new CreateProductTemplateLineResponse(
                templateLine.Id,
                templateLine.ProductTemplateId,
                templateLine.Name,
                templateLine.Type,
                templateLine.IncludeByDefault,
                templateLine.SortOrder,
                templateLine.ModuleId,
                templateLine.QuestionBankItemId),
            "GetProductTemplateLineById",
            new { id = templateLine.Id });
    }
}
