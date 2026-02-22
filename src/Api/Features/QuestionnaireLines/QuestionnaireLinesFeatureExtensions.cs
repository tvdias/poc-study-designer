namespace Api.Features.QuestionnaireLines;

public static class QuestionnaireLinesFeatureExtensions
{
    public static IServiceCollection AddQuestionnaireLinesFeature(this IServiceCollection services)
    {
        services.AddScoped<IQuestionnaireLineService, QuestionnaireLineService>();
        return services;
    }

    public static RouteGroupBuilder MapQuestionnaireLinesEndpoints(this RouteGroupBuilder group)
    {
        group.MapAddQuestionnaireLineEndpoint();
        group.MapGetQuestionnaireLinesEndpoint();
        group.MapUpdateQuestionnaireLineEndpoint();
        group.MapUpdateQuestionnaireLinesSortOrderEndpoint();
        group.MapDeleteQuestionnaireLineEndpoint();
        return group;
    }
}
