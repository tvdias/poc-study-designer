namespace Api.Features.ManagedLists;

public static class ManagedListsFeatureExtensions
{
    public static IServiceCollection AddManagedListsFeature(this IServiceCollection services)
    {
        services.AddScoped<ISubsetManagementService, SubsetManagementService>();
        return services;
    }

    public static RouteGroupBuilder MapManagedListsEndpoints(this RouteGroupBuilder group)
    {
        group.MapCreateManagedListEndpoint();
        group.MapGetManagedListsEndpoint();
        group.MapGetManagedListByIdEndpoint();
        group.MapUpdateManagedListEndpoint();
        group.MapDeactivateManagedListEndpoint();
        group.MapDeleteManagedListEndpoint();

        group.MapSaveQuestionSelectionEndpoint();
        group.MapGetSubsetDetailsEndpoint();
        group.MapGetSubsetsForProjectEndpoint();
        group.MapDeleteSubsetEndpoint();
        group.MapRefreshProjectSummaryEndpoint();

        group.MapCreateManagedListItemEndpoint();
        group.MapUpdateManagedListItemEndpoint();
        group.MapDeleteManagedListItemEndpoint();
        group.MapBulkAddOrUpdateManagedListItemsEndpoint();

        group.MapAssignManagedListToQuestionEndpoint();
        group.MapUnassignManagedListFromQuestionEndpoint();

        return group;
    }
}
