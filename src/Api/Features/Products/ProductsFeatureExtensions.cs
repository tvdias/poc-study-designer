namespace Api.Features.Products;

public static class ProductsFeatureExtensions
{
    public static RouteGroupBuilder MapProductsEndpoints(this RouteGroupBuilder group)
    {
        group.MapCreateProductEndpoint();
        group.MapGetProductsEndpoint();
        group.MapGetProductByIdEndpoint();
        group.MapUpdateProductEndpoint();
        group.MapDeleteProductEndpoint();

        group.MapCreateProductConfigQuestionEndpoint();
        group.MapGetProductConfigQuestionByIdEndpoint();
        group.MapUpdateProductConfigQuestionEndpoint();
        group.MapDeleteProductConfigQuestionEndpoint();

        group.MapCreateProductConfigQuestionDisplayRuleEndpoint();
        group.MapGetProductConfigQuestionDisplayRulesEndpoint();
        group.MapGetProductConfigQuestionDisplayRuleByIdEndpoint();
        group.MapUpdateProductConfigQuestionDisplayRuleEndpoint();
        group.MapDeleteProductConfigQuestionDisplayRuleEndpoint();

        return group;
    }
}
