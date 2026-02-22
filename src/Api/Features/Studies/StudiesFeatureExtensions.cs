namespace Api.Features.Studies;

public static class StudiesFeatureExtensions
{
    public static IServiceCollection AddStudiesFeature(this IServiceCollection services)
    {
        services.AddScoped<IStudyService, StudyService>();
        return services;
    }

    public static RouteGroupBuilder MapStudiesEndpoints(this RouteGroupBuilder group)
    {
        group.MapCreateStudyEndpoint();
        group.MapCreateStudyVersionEndpoint();
        group.MapGetStudiesEndpoint();
        group.MapGetStudyByIdEndpoint();
        group.MapGetStudyQuestionsEndpoint();
        group.MapUpdateStudyEndpoint();
        return group;
    }
}
