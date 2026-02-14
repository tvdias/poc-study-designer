namespace Api.Features.Products;

public record GetProductTemplatesResponse(Guid Id, string Name, int Version, Guid ProductId, string ProductName);
