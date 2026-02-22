namespace Api.Features.Modules;

public static class ModulesFeatureExtensions
{
    public static RouteGroupBuilder MapModulesEndpoints(this RouteGroupBuilder group)
    {
        group.MapCreateModuleEndpoint();
        group.MapGetModulesEndpoint();
        group.MapGetModuleByIdEndpoint();
        group.MapUpdateModuleEndpoint();
        group.MapDeleteModuleEndpoint();

        group.MapCreateModuleQuestionEndpoint();
        group.MapGetModuleQuestionsEndpoint();
        group.MapGetModuleQuestionByIdEndpoint();
        group.MapUpdateModuleQuestionEndpoint();
        group.MapDeleteModuleQuestionEndpoint();

        return group;
    }
}
