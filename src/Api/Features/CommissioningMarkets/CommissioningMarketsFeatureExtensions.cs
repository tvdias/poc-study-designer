namespace Api.Features.CommissioningMarkets;

public static class CommissioningMarketsFeatureExtensions
{
    public static RouteGroupBuilder MapCommissioningMarketsEndpoints(this RouteGroupBuilder group)
    {
        group.MapCreateCommissioningMarketEndpoint();
        group.MapGetCommissioningMarketsEndpoint();
        group.MapGetCommissioningMarketByIdEndpoint();
        group.MapUpdateCommissioningMarketEndpoint();
        group.MapDeleteCommissioningMarketEndpoint();
        return group;
    }
}
