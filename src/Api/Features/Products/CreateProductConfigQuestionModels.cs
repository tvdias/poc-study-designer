namespace Api.Features.Products;

public record CreateProductConfigQuestionRequest(Guid ProductId, Guid ConfigurationQuestionId, string? StatusReason);

public record CreateProductConfigQuestionResponse(Guid Id, Guid ProductId, Guid ConfigurationQuestionId, string? StatusReason);
