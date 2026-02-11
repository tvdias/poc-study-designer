namespace Api.Features.Products;

public record UpdateProductTemplateRequest(string Name, int Version, Guid ProductId);

public record UpdateProductTemplateResponse(Guid Id, string Name, int Version, Guid ProductId);
