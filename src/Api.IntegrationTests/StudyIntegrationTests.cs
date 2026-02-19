using System.Net;
using System.Net.Http.Json;
using Api.Features.Projects;
using Api.Features.Studies;
using Api.Features.QuestionBank;
using Api.Features.QuestionnaireLines;
using Api.Features.ManagedLists;
using Api.Features.FieldworkMarkets;
using Xunit;

namespace Api.IntegrationTests;

public class StudyIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;

    public StudyIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateStudy_V1_ShouldSucceed()
    {
        var client = _fixture.HttpClient;

        // Arrange: Create a project with questionnaire lines
        var project = await CreateTestProjectAsync(client);
        var questionBankItem = await CreateTestQuestionBankItemAsync(client);
        await AddQuestionnaireLineAsync(client, project.Id, questionBankItem.Id);

        // Act: Create Study V1
        var createRequest = new CreateStudyRequest
        {
            ProjectId = project.Id,
            Name = "Test Study V1",
            Category = "Test Category",
            MaconomyJobNumber = "JOB123",
            ProjectOperationsUrl = "http://test.com",
            FieldworkMarketId = await CreateTestFieldworkMarketAsync(client),
            ScripterNotes = "Initial version"
        };

        var response = await client.PostAsJsonAsync("/api/studies", createRequest);

        // Assert
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            Assert.Fail($"Expected Created but got {response.StatusCode}. Error: {errorMessage}");
        }

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var study = await response.Content.ReadFromJsonAsync<CreateStudyResponse>(_fixture.JsonOptions);
        Assert.NotNull(study);
        Assert.Equal("Test Study V1", study.Name);
        Assert.Equal(1, study.Version);
        Assert.Equal(StudyStatus.Draft, study.Status);
        
        // Debug: Print question count
        Assert.True(study.QuestionCount > 0, $"Expected QuestionCount > 0, but was {study.QuestionCount}");

        // Verify project counters updated
        var projectResponse = await client.GetAsync($"/api/projects/{project.Id}");
        var updatedProject = await projectResponse.Content.ReadFromJsonAsync<GetProjectByIdResponse>(_fixture.JsonOptions);
        Assert.NotNull(updatedProject);
        Assert.True(updatedProject.HasStudies, $"Expected HasStudies=true, but was {updatedProject.HasStudies}");
        Assert.Equal(1, updatedProject.StudyCount);
        Assert.NotNull(updatedProject.LastStudyModifiedOn);
    }

    [Fact]
    public async Task CreateStudyVersion_ShouldSucceed()
    {
        var client = _fixture.HttpClient;

        // Arrange: Create a project, questionnaire, and Study V1
        var project = await CreateTestProjectAsync(client);
        var questionBankItem = await CreateTestQuestionBankItemAsync(client);
        await AddQuestionnaireLineAsync(client, project.Id, questionBankItem.Id);

        var createV1Request = new CreateStudyRequest
        {
            ProjectId = project.Id,
            Name = "Test Study V1",
            Category = "Test Category",
            MaconomyJobNumber = "JOB123",
            ProjectOperationsUrl = "http://test.com",
            FieldworkMarketId = await CreateTestFieldworkMarketAsync(client)
        };

        var v1Response = await client.PostAsJsonAsync("/api/studies", createV1Request);
        var v1Study = await v1Response.Content.ReadFromJsonAsync<CreateStudyResponse>(_fixture.JsonOptions);
        Assert.NotNull(v1Study);

        // Act: Try to create V2 while V1 is still Draft
        var v2Response = await client.PostAsync($"/api/studies/{v1Study.StudyId}/versions", null);

        // Assert: Should fail because V1 is still Draft
        Assert.Equal(HttpStatusCode.Conflict, v2Response.StatusCode);
        var errorContent = await v2Response.Content.ReadAsStringAsync();
        Assert.Contains("Only one Draft version is allowed", errorContent);
    }

    [Fact]
    public async Task CreateStudyVersion_AfterMarkingParentNonDraft_ShouldSucceed()
    {
        var client = _fixture.HttpClient;

        // Arrange: Create project, questionnaire, and Study V1
        var project = await CreateTestProjectAsync(client);
        var questionBankItem = await CreateTestQuestionBankItemAsync(client);
        await AddQuestionnaireLineAsync(client, project.Id, questionBankItem.Id);

        var createV1Request = new CreateStudyRequest
        {
            ProjectId = project.Id,
            Name = "Test Study V1",
            Category = "Test",
            MaconomyJobNumber = "123",
            ProjectOperationsUrl = "http://test.com",
            FieldworkMarketId = await CreateTestFieldworkMarketAsync(client)
        };

        var v1Response = await client.PostAsJsonAsync("/api/studies", createV1Request);
        var v1Study = await v1Response.Content.ReadFromJsonAsync<CreateStudyResponse>(_fixture.JsonOptions);
        Assert.NotNull(v1Study);

        // Mark V1 as non-draft (ReadyForScripting)
        var updateRequest = new UpdateStudyRequest
        {
            Name = v1Study.Name,
            Category = "Test",
            MaconomyJobNumber = "123",
            ProjectOperationsUrl = "http://test.com",
            FieldworkMarketId = await CreateTestFieldworkMarketAsync(client),
            Status = StudyStatus.ReadyForScripting
        };

        var updateResponse = await client.PutAsJsonAsync($"/api/studies/{v1Study.StudyId}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        // Act: Create V2
        var v2Response = await client.PostAsync($"/api/studies/{v1Study.StudyId}/versions", null);

        // Assert: Should succeed
        Assert.Equal(HttpStatusCode.Created, v2Response.StatusCode);

        var v2Study = await v2Response.Content.ReadFromJsonAsync<CreateStudyVersionResponse>(_fixture.JsonOptions);
        Assert.NotNull(v2Study);
        Assert.Equal("Test Study V1", v2Study.Name);
        Assert.Equal(2, v2Study.Version);
        Assert.Equal(StudyStatus.Draft, v2Study.Status);
        Assert.Equal(v1Study.StudyId, v2Study.ParentStudyId);
    }

    [Fact]
    public async Task GetStudies_ShouldReturnStudiesForProject()
    {
        var client = _fixture.HttpClient;

        // Arrange: Create project and study
        var project = await CreateTestProjectAsync(client);
        var questionBankItem = await CreateTestQuestionBankItemAsync(client);
        await AddQuestionnaireLineAsync(client, project.Id, questionBankItem.Id);

        var createRequest = new CreateStudyRequest
        {
            ProjectId = project.Id,
            Name = "Test Study for List",
            Category = "Test",
            MaconomyJobNumber = "123",
            ProjectOperationsUrl = "http://test",
            FieldworkMarketId = await CreateTestFieldworkMarketAsync(client)
        };

        await client.PostAsJsonAsync("/api/studies", createRequest);

        // Act: Get studies for project
        var response = await client.GetAsync($"/api/studies?projectId={project.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetStudiesResponse>(_fixture.JsonOptions);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Studies);
        Assert.Contains(result.Studies, s => s.Name == "Test Study for List");
    }

    [Fact]
    public async Task GetStudyById_ShouldReturnStudyDetails()
    {
        var client = _fixture.HttpClient;

        // Arrange: Create project and study
        var project = await CreateTestProjectAsync(client);
        var questionBankItem = await CreateTestQuestionBankItemAsync(client);
        await AddQuestionnaireLineAsync(client, project.Id, questionBankItem.Id);

        var createRequest = new CreateStudyRequest
        {
            ProjectId = project.Id,
            Name = "Test Study for Details",
            Category = "Test",
            MaconomyJobNumber = "123",
            ProjectOperationsUrl = "http://test",
            FieldworkMarketId = await CreateTestFieldworkMarketAsync(client)
        };

        var createResponse = await client.PostAsJsonAsync("/api/studies", createRequest);
        var study = await createResponse.Content.ReadFromJsonAsync<CreateStudyResponse>(_fixture.JsonOptions);
        Assert.NotNull(study);

        // Act: Get study details
        var response = await client.GetAsync($"/api/studies/{study.StudyId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var details = await response.Content.ReadFromJsonAsync<GetStudyDetailsResponse>(_fixture.JsonOptions);
        Assert.NotNull(details);
        Assert.Equal(study.StudyId, details.StudyId);
        Assert.Equal("Test Study for Details", details.Name);
        Assert.Equal(1, details.Version);
        Assert.Equal(StudyStatus.Draft, details.Status);
        Assert.True(details.QuestionCount > 0);
    }

    [Fact]
    public async Task CreateStudy_WithoutQuestionnairLines_ShouldFail()
    {
        var client = _fixture.HttpClient;

        // Arrange: Create project without questionnaire lines
        var project = await CreateTestProjectAsync(client);

        var createRequest = new CreateStudyRequest
        {
            ProjectId = project.Id,
            Name = "Test Study Empty",
            Category = "Test",
            MaconomyJobNumber = "123",
            ProjectOperationsUrl = "http://test",
            FieldworkMarketId = await CreateTestFieldworkMarketAsync(client)
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/studies", createRequest);

        // Assert: Should fail because no questionnaire lines
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("has no questionnaire lines", errorContent);
    }

    [Fact]
    public async Task CreateStudy_WithManagedLists_ShouldCopyAssignments()
    {
        var client = _fixture.HttpClient;

        // Arrange: Create project with questionnaire and managed list assignment
        var project = await CreateTestProjectAsync(client);
        var questionBankItem = await CreateTestQuestionBankItemAsync(client);
        var questionnaireLineId = await AddQuestionnaireLineAsync(client, project.Id, questionBankItem.Id);
        
        // Create managed list
        var managedList = await CreateTestManagedListAsync(client, project.Id);
        
        // Assign managed list to question
        await AssignManagedListToQuestionAsync(client, questionnaireLineId, managedList.Id);

        // Act: Create Study V1
        var createRequest = new CreateStudyRequest
        {
            ProjectId = project.Id,
            Name = "Test Study with ML",
            Category = "Test",
            MaconomyJobNumber = "123",
            ProjectOperationsUrl = "http://test",
            FieldworkMarketId = await CreateTestFieldworkMarketAsync(client)
        };

        var response = await client.PostAsJsonAsync("/api/studies", createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var study = await response.Content.ReadFromJsonAsync<CreateStudyResponse>(_fixture.JsonOptions);
        Assert.NotNull(study);
        Assert.True(study.QuestionCount > 0);
        
        // TODO: Verify ML assignments were copied
        // Would require additional endpoint to get study question details
    }

    // Helper methods
    private async Task<Project> CreateTestProjectAsync(HttpClient client)
    {
        var projectRequest = new
        {
            Name = $"Test Project {Guid.NewGuid():N}",
            Description = "Test Description",
            Owner = "TestOwner",
            Status = ProjectStatus.Draft,
            // Adding likely required fields to avoid runtime error, though anonymous type compiled fine
            ClientId = (Guid?)null,
            CommissioningMarketId = (Guid?)null,
            ProductId = (Guid?)null,
            Methodology = Methodology.Online,
            CostManagementEnabled = true
        };

        var response = await client.PostAsJsonAsync("/api/projects", projectRequest);
        
        // If it fails due to FK, we'll see it at runtime. 
        if (!response.IsSuccessStatusCode)
        {
             var content = await response.Content.ReadAsStringAsync();
             // Just throw to fail test fast
             throw new Exception($"Failed to create project: {response.StatusCode} {content}");
        }
        
        var project = await response.Content.ReadFromJsonAsync<Project>(_fixture.JsonOptions);
        Assert.NotNull(project);
        return project;
    }

    private async Task<QuestionBankItem> CreateTestQuestionBankItemAsync(HttpClient client)
    {
        var questionRequest = new
        {
            VariableName = $"Q{Guid.NewGuid():N}",
            QuestionText = "Test Question",
            QuestionType = "Single",
            Version = 1,
            Status = "Active",
            // MetricGroup is optional, but if required add it here
            MetricGroupId = (Guid?)null 
        };

        var response = await client.PostAsJsonAsync("/api/question-bank", questionRequest);
        response.EnsureSuccessStatusCode();
        
        var question = await response.Content.ReadFromJsonAsync<QuestionBankItem>(_fixture.JsonOptions);
        Assert.NotNull(question);
        return question;
    }

    private async Task<Guid> AddQuestionnaireLineAsync(HttpClient client, Guid projectId, Guid questionBankItemId)
    {
        var lineRequest = new
        {
            QuestionBankItemId = questionBankItemId,
            SortOrder = 1
        };

        var response = await client.PostAsJsonAsync($"/api/projects/{projectId}/questionnairelines", lineRequest);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<QuestionnaireLine>(_fixture.JsonOptions);
        Assert.NotNull(result);
        return result.Id;
    }

    private async Task<ManagedList> CreateTestManagedListAsync(HttpClient client, Guid projectId)
    {
        var listRequest = new
        {
            ProjectId = projectId,
            Name = $"TestList{Guid.NewGuid():N}",
            Description = "Test List"
        };

        var response = await client.PostAsJsonAsync("/api/managedlists", listRequest);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create managed list: {content}");
        }
        
        var list = await response.Content.ReadFromJsonAsync<ManagedList>(_fixture.JsonOptions);
        Assert.NotNull(list);
        return list;
    }

    private async Task AssignManagedListToQuestionAsync(HttpClient client, Guid questionnaireLineId, Guid managedListId)
    {
        var assignRequest = new
        {
            QuestionnaireLineId = questionnaireLineId,
            ManagedListId = managedListId
        };

        var response = await client.PostAsJsonAsync("/api/managedlists/assign", assignRequest);
        response.EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreateTestFieldworkMarketAsync(HttpClient client)
    {
        var request = new CreateFieldworkMarketRequest($"GB-{Guid.NewGuid():N}".Substring(0, 10), "Test Market");
        var response = await client.PostAsJsonAsync("/api/fieldwork-markets", request);
        response.EnsureSuccessStatusCode();
        var market = await response.Content.ReadFromJsonAsync<CreateFieldworkMarketResponse>(_fixture.JsonOptions);
        return market.Id;
    }
}

