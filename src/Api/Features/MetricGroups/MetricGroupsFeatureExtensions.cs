namespace Api.Features.MetricGroups;

public static class MetricGroupsFeatureExtensions
{
    public static RouteGroupBuilder MapMetricGroupsEndpoints(this RouteGroupBuilder group)
    {
        group.MapCreateMetricGroupEndpoint();
        group.MapGetMetricGroupsEndpoint();
        group.MapGetMetricGroupByIdEndpoint();
        group.MapUpdateMetricGroupEndpoint();
        group.MapDeleteMetricGroupEndpoint();
        return group;
    }
}
