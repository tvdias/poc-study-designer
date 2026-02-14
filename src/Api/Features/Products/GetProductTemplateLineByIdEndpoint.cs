using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Products;

public static class GetProductTemplateLineByIdEndpoint
{
    public static void MapGetProductTemplateLineByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/product-template-lines/{id:guid}", HandleAsync)
            .WithName("GetProductTemplateLineById")
            .WithSummary("Get Product Template Line by ID")
            .WithTags("ProductTemplateLines");
    }

    public static async Task<Results<Ok<ProductTemplateLineInfo>, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var templateLine = await db.ProductTemplateLines
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
            .FirstOrDefaultAsync(cancellationToken);

        return templateLine is not null
            ? TypedResults.Ok(templateLine)
            : TypedResults.NotFound();
    }
}
