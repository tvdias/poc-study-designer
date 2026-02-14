namespace Api.Features.Products;

public record CreateProductConfigQuestionRequest(Guid ProductId, Guid ConfigurationQuestionId);

public record CreateProductConfigQuestionResponse(Guid Id, Guid ProductId, Guid ConfigurationQuestionId);
