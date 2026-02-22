namespace Api.Features.QuestionBank;

public static class QuestionBankFeatureExtensions
{
    public static IServiceCollection AddQuestionBankFeature(this IServiceCollection services)
    {
        services.AddScoped<IQuestionBankService, QuestionBankService>();
        return services;
    }

    public static RouteGroupBuilder MapQuestionBankEndpoints(this RouteGroupBuilder group)
    {
        group.MapCreateQuestionAnswerEndpoint();
        group.MapUpdateQuestionAnswerEndpoint();
        group.MapDeleteQuestionAnswerEndpoint();

        group.MapCreateQuestionBankItemEndpoint();
        group.MapGetQuestionBankItemsEndpoint();
        group.MapGetQuestionBankItemByIdEndpoint();
        group.MapUpdateQuestionBankItemEndpoint();
        group.MapDeleteQuestionBankItemEndpoint();

        return group;
    }
}
