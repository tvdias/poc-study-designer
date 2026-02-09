using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.Products;

public static class UpdateProductTemplateLineEndpoint
{
    public static void MapUpdateProductTemplateLineEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/product-template-lines/{id:guid}", HandleAsync)
            .WithName("UpdateProductTemplateLine")
            .WithSummary("Update Product Template Line")
            .WithTags("ProductTemplateLines");
    }

    public static async Task<Results<Ok<ProductTemplateLineInfo>, ValidationProblem, NotFound, Conflict<string>>> HandleAsync(
        Guid id,
        UpdateProductTemplateLineRequest request,
        ApplicationDbContext db,
        IValidator<UpdateProductTemplateLineRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var templateLine = await db.ProductTemplateLines.FindAsync([id], cancellationToken);
        if (templateLine is null)
        {
            return TypedResults.NotFound();
        }

        templateLine.Name = request.Name;
        templateLine.Type = request.Type;
        templateLine.IncludeByDefault = request.IncludeByDefault;
        templateLine.SortOrder = request.SortOrder;
        templateLine.ModuleId = request.ModuleId;
        templateLine.QuestionBankItemId = request.QuestionBankItemId;
        templateLine.IsActive = request.IsActive;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("foreign key", StringComparison.OrdinalIgnoreCase) == true)
            {
                return TypedResults.Conflict("Referenced module or question bank item does not exist.");
            }

            throw;
        }

        var updatedLine = await db.ProductTemplateLines
            .Include(ptl => ptl.Module)
            .Include(ptl => ptl.QuestionBankItem)
            .Where(ptl => ptl.Id == id)
            .Select(ptl => new ProductTemplateLineInfo(
                ptl.Id,
                ptl.ProductTemplateId,
                ptl.Name,
                ptl.Type,
                ptl.IncludeByDefault,
                ptl.SortOrder,
                ptl.ModuleId,
                ptl.QuestionBankItemId,
                ptl.IsActive,
                ptl.CreatedOn,
                ptl.Module != null ? ptl.Module.Label : null,
                ptl.QuestionBankItem != null ? ptl.QuestionBankItem.VariableName : null))
            .FirstAsync(cancellationToken);

        return TypedResults.Ok(updatedLine);
    }
}
