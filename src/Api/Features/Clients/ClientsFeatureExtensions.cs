namespace Api.Features.Clients;

public static class ClientsFeatureExtensions
{
    public static IServiceCollection AddClientsFeature(this IServiceCollection services)
    {
        services.AddScoped<IClientService, ClientService>();
        return services;
    }

    public static RouteGroupBuilder MapClientsEndpoints(this RouteGroupBuilder group)
    {
        group.MapCreateClientEndpoint();
        group.MapGetClientsEndpoint();
        group.MapGetClientByIdEndpoint();
        group.MapUpdateClientEndpoint();
        group.MapDeleteClientEndpoint();
        return group;
    }
}
