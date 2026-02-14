using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Products;

public static class GetProductTemplateLinesEndpoint
{
    public static void MapGetProductTemplateLinesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/product-template-lines", HandleAsync)
            .WithName("GetProductTemplateLines")
            .WithSummary("Get Product Template Lines")
            .WithTags("ProductTemplateLines");
    }

    public static async Task<Ok<List<ProductTemplateLineInfo>>> HandleAsync(
        ApplicationDbContext db,
        Guid? productTemplateId,
        CancellationToken cancellationToken)
    {
        var query = db.ProductTemplateLines
            .Include(ptl => ptl.Module)
            .Include(ptl => ptl.QuestionBankItem)
            .AsQueryable();

        if (productTemplateId.HasValue)
        {
            query = query.Where(ptl => ptl.ProductTemplateId == productTemplateId.Value);
        }

        var templateLines = await query
            .OrderBy(ptl => ptl.SortOrder)
            .ThenBy(ptl => ptl.Name)
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
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(templateLines);
    }
}
