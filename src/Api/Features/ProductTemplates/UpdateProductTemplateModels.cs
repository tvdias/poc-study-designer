namespace Api.Features.Products;

public record UpdateProductTemplateRequest(string Name, int Version, Guid ProductId, bool IsActive);

public record UpdateProductTemplateResponse(Guid Id, string Name, int Version, Guid ProductId, bool IsActive);
