namespace Api.Features.Tags;

public static class TagsFeatureExtensions
{
    public static IServiceCollection AddTagsFeature(this IServiceCollection services)
    {
        services.AddScoped<ITagService, TagService>();
        return services;
    }

    public static RouteGroupBuilder MapTagsEndpoints(this RouteGroupBuilder group)
    {
        group.MapCreateTagEndpoint();
        group.MapGetTagsEndpoint();
        group.MapGetTagByIdEndpoint();
        group.MapUpdateTagEndpoint();
        group.MapDeleteTagEndpoint();
        return group;
    }
}
