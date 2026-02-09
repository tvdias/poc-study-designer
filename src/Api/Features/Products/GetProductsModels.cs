namespace Api.Features.Products;

public record GetProductsResponse(Guid Id, string Name, string? Description, bool IsActive);

public record GetProductDetailResponse(
    Guid Id, 
    string Name, 
    string? Description, 
    bool IsActive,
    List<ProductTemplateInfo> ProductTemplates,
    List<ProductConfigQuestionInfo> ConfigurationQuestions
);

public record ProductTemplateInfo(Guid Id, string Name, int Version);

public record ProductConfigQuestionInfo(Guid Id, Guid ConfigurationQuestionId, string Question);
