namespace Api.Features.Seed;

public static class SeedFeatureExtensions
{
    public static RouteGroupBuilder MapSeedEndpoints(this RouteGroupBuilder group)
    {
        group.MapSeedDataEndpoint();
        return group;
    }
}
