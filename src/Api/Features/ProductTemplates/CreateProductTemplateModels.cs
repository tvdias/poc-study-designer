namespace Api.Features.Products;

public record CreateProductTemplateRequest(string Name, int Version, Guid ProductId);

public record CreateProductTemplateResponse(Guid Id, string Name, int Version, Guid ProductId);
