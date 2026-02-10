using Api.Features.ConfigurationQuestions;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

public class ConfigurationQuestionTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task ConfigurationQuestionWorkflow_CreateAndRetrieve_ExecutesSuccessfully()
    {
        // Arrange
        var httpClient = fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;

        // ===== CHECKPOINT 1: CREATE =====
        var createRequest = new CreateConfigurationQuestionRequest("Workflow Config Question", "AI prompt for workflow", RuleType.MultiCoded);
        var createResponse = await httpClient.PostAsJsonAsync("/api/configuration-questions", createRequest, cancellationToken);
        
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        var createdQuestion = await createResponse.Content.ReadFromJsonAsync<CreateConfigurationQuestionResponse>(fixture.JsonOptions, cancellationToken);
        Assert.NotNull(createdQuestion);
        Assert.Equal(createRequest.Question, createdQuestion.Question);
        Assert.Equal(createRequest.RuleType, createdQuestion.RuleType);
        Assert.NotEqual(Guid.Empty, createdQuestion.Id);
        Assert.True(createdQuestion.IsActive);

        var questionId = createdQuestion.Id;

        // ===== CHECKPOINT 2: GET ALL (verify in list) =====
        var getAllResponse = await httpClient.GetAsync("/api/configuration-questions", cancellationToken);
        
        getAllResponse.EnsureSuccessStatusCode();
        var allQuestions = await getAllResponse.Content.ReadFromJsonAsync<List<GetConfigurationQuestionsResponse>>(fixture.JsonOptions, cancellationToken);
        Assert.NotNull(allQuestions);
        Assert.Contains(allQuestions, q => q.Id == questionId && q.Question == createRequest.Question);
    }
}
