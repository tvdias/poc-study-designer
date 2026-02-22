namespace Api.Features.ConfigurationQuestions;

public static class ConfigurationQuestionsFeatureExtensions
{
    public static RouteGroupBuilder MapConfigurationQuestionsEndpoints(this RouteGroupBuilder group)
    {
        group.MapCreateConfigurationAnswerEndpoint();
        group.MapGetConfigurationAnswersEndpoint();
        group.MapGetConfigurationAnswerByIdEndpoint();
        group.MapUpdateConfigurationAnswerEndpoint();
        group.MapDeleteConfigurationAnswerEndpoint();

        group.MapCreateConfigurationQuestionEndpoint();
        group.MapGetConfigurationQuestionsEndpoint();
        group.MapGetConfigurationQuestionByIdEndpoint();
        group.MapUpdateConfigurationQuestionEndpoint();
        group.MapDeleteConfigurationQuestionEndpoint();

        group.MapCreateDependencyRuleEndpoint();
        group.MapGetDependencyRulesEndpoint();
        group.MapGetDependencyRuleByIdEndpoint();
        group.MapUpdateDependencyRuleEndpoint();
        group.MapDeleteDependencyRuleEndpoint();

        return group;
    }
}
