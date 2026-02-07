namespace Api.Features.Products;

public record CreateProductRequest(string Name, string? Description);

public record CreateProductResponse(Guid Id, string Name, string? Description);
