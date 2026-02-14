namespace Api.Features.Products;

public record UpdateProductRequest(string Name, string? Description);

public record UpdateProductResponse(Guid Id, string Name, string? Description);
