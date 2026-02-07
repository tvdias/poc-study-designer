namespace Api.Features.Products;

public record UpdateProductConfigQuestionRequest(string? StatusReason, bool IsActive);

public record UpdateProductConfigQuestionResponse(Guid Id, Guid ProductId, Guid ConfigurationQuestionId, string? StatusReason, bool IsActive);
