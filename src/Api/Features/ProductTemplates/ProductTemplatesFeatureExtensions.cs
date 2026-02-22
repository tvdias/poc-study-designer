namespace Api.Features.ProductTemplates;

public static class ProductTemplatesFeatureExtensions
{
    public static RouteGroupBuilder MapProductTemplatesEndpoints(this RouteGroupBuilder group)
    {
        group.MapCreateProductTemplateEndpoint();
        group.MapGetProductTemplatesEndpoint();
        group.MapGetProductTemplateByIdEndpoint();
        group.MapUpdateProductTemplateEndpoint();
        group.MapDeleteProductTemplateEndpoint();

        return group;
    }
}
