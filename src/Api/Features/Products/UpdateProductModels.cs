namespace Api.Features.Products;

public record UpdateProductRequest(string Name, string? Description, bool IsActive);

public record UpdateProductResponse(Guid Id, string Name, string? Description, bool IsActive);
