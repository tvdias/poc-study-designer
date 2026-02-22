namespace Api.Features.Projects;

public static class ProjectsFeatureExtensions
{
    public static IServiceCollection AddProjectsFeature(this IServiceCollection services)
    {
        services.AddScoped<IProjectService, ProjectService>();
        return services;
    }

    public static RouteGroupBuilder MapProjectsEndpoints(this RouteGroupBuilder group)
    {
        group.MapCreateProjectEndpoint();
        group.MapGetProjectsEndpoint();
        group.MapGetProjectByIdEndpoint();
        group.MapUpdateProjectEndpoint();
        group.MapDeleteProjectEndpoint();
        return group;
    }
}
