namespace Api.Features.FieldworkMarkets;

public static class FieldworkMarketsFeatureExtensions
{
    public static RouteGroupBuilder MapFieldworkMarketsEndpoints(this RouteGroupBuilder group)
    {
        group.MapCreateFieldworkMarketEndpoint();
        group.MapGetFieldworkMarketsEndpoint();
        group.MapGetFieldworkMarketByIdEndpoint();
        group.MapUpdateFieldworkMarketEndpoint();
        group.MapDeleteFieldworkMarketEndpoint();
        return group;
    }
}
