extern alias AppHostAssembly;
using Api.Features.ConfigurationQuestions;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

public class ConfigurationQuestionTests(BoxedAppHostFixture fixture) : IClassFixture<BoxedAppHostFixture>
{
    [Fact]
    public async Task CreateAndGetConfigurationQuestions_WorksCorrectly()
    {
        // Arrange
        var appName = "api";
        var client = fixture.App.CreateHttpClient(appName);

        // Act - Create
        var newQuestion = new CreateConfigurationQuestionRequest("Test Config Question", "Test Prompt", RuleType.SingleCoded);
        var createResponse = await client.PostAsJsonAsync("/api/configuration-questions", newQuestion, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - Create
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        var createdQuestion = await createResponse.Content.ReadFromJsonAsync<CreateConfigurationQuestionResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(createdQuestion);
        Assert.Equal(newQuestion.Question, createdQuestion.Question);
        Assert.Equal(newQuestion.RuleType, createdQuestion.RuleType);
        Assert.NotEqual(Guid.Empty, createdQuestion.Id);

        // Act - Get
        var getResponse = await client.GetAsync("/api/configuration-questions", TestContext.Current.CancellationToken);
        
        // Assert - Get
        getResponse.EnsureSuccessStatusCode();
        var questions = await getResponse.Content.ReadFromJsonAsync<List<GetConfigurationQuestionsResponse>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(questions);
        Assert.Contains(questions, q => q.Id == createdQuestion.Id && q.Question == newQuestion.Question);
    }
}
