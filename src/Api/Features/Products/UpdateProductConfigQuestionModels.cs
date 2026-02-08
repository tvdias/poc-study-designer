namespace Api.Features.Products;

public record UpdateProductConfigQuestionRequest(bool IsActive);

public record UpdateProductConfigQuestionResponse(Guid Id, Guid ProductId, Guid ConfigurationQuestionId, bool IsActive);
