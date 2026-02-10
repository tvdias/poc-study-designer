using Api.Features.ConfigurationQuestions;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

public class ConfigurationQuestionTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task ConfigurationQuestionCrudWorkflow_ExecutesSuccessfully()
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

        // ===== CHECKPOINT 2: GET BY ID =====
        var getByIdResponse = await httpClient.GetAsync($"/api/configuration-questions/{questionId}", cancellationToken);
        
        getByIdResponse.EnsureSuccessStatusCode();
        var fetchedQuestion = await getByIdResponse.Content.ReadFromJsonAsync<GetConfigurationQuestionByIdResponse>(fixture.JsonOptions, cancellationToken);
        Assert.NotNull(fetchedQuestion);
        Assert.Equal(questionId, fetchedQuestion.Id);
        Assert.Equal(createRequest.Question, fetchedQuestion.Question);
        Assert.Equal(createRequest.RuleType, fetchedQuestion.RuleType);

        // ===== CHECKPOINT 3: GET ALL (verify in list) =====
        var getAllResponse = await httpClient.GetAsync("/api/configuration-questions", cancellationToken);
        
        getAllResponse.EnsureSuccessStatusCode();
        var allQuestions = await getAllResponse.Content.ReadFromJsonAsync<List<GetConfigurationQuestionsResponse>>(fixture.JsonOptions, cancellationToken);
        Assert.NotNull(allQuestions);
        Assert.Contains(allQuestions, q => q.Id == questionId && q.Question == createRequest.Question);

        // ===== CHECKPOINT 4: UPDATE =====
        var updateRequest = new UpdateConfigurationQuestionRequest("Workflow Config Question (Updated)", "AI prompt (updated)", RuleType.SingleCoded, false);
        var updateResponse = await httpClient.PutAsJsonAsync($"/api/configuration-questions/{questionId}", updateRequest, cancellationToken);
        
        updateResponse.EnsureSuccessStatusCode();
        var updatedQuestion = await updateResponse.Content.ReadFromJsonAsync<UpdateConfigurationQuestionResponse>(fixture.JsonOptions, cancellationToken);
        Assert.NotNull(updatedQuestion);
        Assert.Equal(questionId, updatedQuestion.Id);
        Assert.Equal("Workflow Config Question (Updated)", updatedQuestion.Question);
        Assert.Equal(RuleType.SingleCoded, updatedQuestion.RuleType);
        Assert.False(updatedQuestion.IsActive);

        // ===== CHECKPOINT 5: VERIFY UPDATE (get by id again) =====
        var verifyUpdateResponse = await httpClient.GetAsync($"/api/configuration-questions/{questionId}", cancellationToken);
        
        verifyUpdateResponse.EnsureSuccessStatusCode();
        var verifiedQuestion = await verifyUpdateResponse.Content.ReadFromJsonAsync<GetConfigurationQuestionByIdResponse>(fixture.JsonOptions, cancellationToken);
        Assert.NotNull(verifiedQuestion);
        Assert.Equal("Workflow Config Question (Updated)", verifiedQuestion.Question);
        Assert.Equal(RuleType.SingleCoded, verifiedQuestion.RuleType);
        Assert.False(verifiedQuestion.IsActive);

        // ===== CHECKPOINT 6: DELETE =====
        var deleteResponse = await httpClient.DeleteAsync($"/api/configuration-questions/{questionId}", cancellationToken);
        
        deleteResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // ===== CHECKPOINT 7: VERIFY DELETION (should return 404) =====
        var verifyDeleteResponse = await httpClient.GetAsync($"/api/configuration-questions/{questionId}", cancellationToken);
        
        Assert.Equal(System.Net.HttpStatusCode.NotFound, verifyDeleteResponse.StatusCode);
    }
}
