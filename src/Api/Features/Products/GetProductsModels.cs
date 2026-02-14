namespace Api.Features.Products;

public record GetProductsResponse(Guid Id, string Name, string? Description);

public record GetProductDetailResponse(
    Guid Id, 
    string Name, 
    string? Description,
    List<ProductTemplateInfo> ProductTemplates,
    List<ProductConfigQuestionInfo> ConfigurationQuestions
);

public record ProductTemplateInfo(Guid Id, string Name, int Version);

public record ProductConfigQuestionInfo(Guid Id, Guid ConfigurationQuestionId, string Question);
