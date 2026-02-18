using System.Net;
using System.Net.Http.Json;
using Api.Features.ManagedLists;
using Api.Features.Projects;
using Api.Features.QuestionnaireLines;
using Xunit;

namespace Api.IntegrationTests;

public class SubsetIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;
    private readonly HttpClient _client;

    public SubsetIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.HttpClient;
    }

    [Fact]
    public async Task SaveQuestionSelection_PartialSelection_CreatesSubset()
    {
        // Arrange - Create a project
        var projectRequest = new { Name = $"Test Project {Guid.NewGuid()}", Description = "Test", Status = ProjectStatus.Draft };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest, _fixture.JsonOptions);
        projectResponse.EnsureSuccessStatusCode();
        var project = await projectResponse.Content.ReadFromJsonAsync<CreateProjectResponse>(_fixture.JsonOptions);
        Assert.NotNull(project);

        // Create a managed list
        var listRequest = new { ProjectId = project.Id, Name = $"BRANDS_{Guid.NewGuid().ToString().Substring(0, 8)}", Description = "Test brands list" };
        var listResponse = await _client.PostAsJsonAsync("/api/managed-lists", listRequest, _fixture.JsonOptions);
        listResponse.EnsureSuccessStatusCode();
        var managedList = await listResponse.Content.ReadFromJsonAsync<CreateManagedListResponse>(_fixture.JsonOptions);
        Assert.NotNull(managedList);

        // Add items to the managed list
        var itemIds = new List<Guid>();
        for (int i = 1; i <= 10; i++)
        {
            var itemRequest = new { Value = $"ITEM{i}", Label = $"Item {i}", SortOrder = i };
            var itemResponse = await _client.PostAsJsonAsync($"/api/managed-lists/{managedList.Id}/items", itemRequest, _fixture.JsonOptions);
            itemResponse.EnsureSuccessStatusCode();
            var item = await itemResponse.Content.ReadFromJsonAsync<CreateManagedListItemResponse>(_fixture.JsonOptions);
            Assert.NotNull(item);
            itemIds.Add(item.Id);
        }

        // Create a questionnaire line
        var questionRequest = new
        {
            ProjectId = project.Id,
            VariableName = $"Q1_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Version = 1,
            QuestionText = "Test question",
            SortOrder = 1
        };
        var questionResponse = await _client.PostAsJsonAsync("/api/questionnairelines", questionRequest, _fixture.JsonOptions);
        questionResponse.EnsureSuccessStatusCode();
        var question = await questionResponse.Content.ReadFromJsonAsync<AddQuestionnaireLineResponse>(_fixture.JsonOptions);
        Assert.NotNull(question);

        // Act - Save a partial selection (first 5 items)
        var selectionRequest = new SaveQuestionSelectionRequest(
            project.Id,
            question.Id,
            managedList.Id,
            itemIds.Take(5).ToList()
        );
        var selectionResponse = await _client.PostAsJsonAsync("/api/subsets/save-selection", selectionRequest, _fixture.JsonOptions);

        // Assert
        selectionResponse.EnsureSuccessStatusCode();
        var result = await selectionResponse.Content.ReadFromJsonAsync<SaveQuestionSelectionResponse>(_fixture.JsonOptions);
        Assert.NotNull(result);
        Assert.False(result.IsFullSelection);
        Assert.NotNull(result.SubsetDefinitionId);
        Assert.NotNull(result.SubsetName);
        Assert.Contains("_SUB1", result.SubsetName);
    }

    [Fact]
    public async Task SaveQuestionSelection_FullSelection_ClearsSubset()
    {
        // Arrange - Create project, managed list, items, and question
        var projectRequest = new { Name = $"Test Project {Guid.NewGuid()}", Description = "Test", Status = ProjectStatus.Draft };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest, _fixture.JsonOptions);
        projectResponse.EnsureSuccessStatusCode();
        var project = await projectResponse.Content.ReadFromJsonAsync<CreateProjectResponse>(_fixture.JsonOptions);
        Assert.NotNull(project);

        var listRequest = new { ProjectId = project.Id, Name = $"BRANDS_{Guid.NewGuid().ToString().Substring(0, 8)}", Description = "Test brands list" };
        var listResponse = await _client.PostAsJsonAsync("/api/managed-lists", listRequest, _fixture.JsonOptions);
        listResponse.EnsureSuccessStatusCode();
        var managedList = await listResponse.Content.ReadFromJsonAsync<CreateManagedListResponse>(_fixture.JsonOptions);
        Assert.NotNull(managedList);

        var itemIds = new List<Guid>();
        for (int i = 1; i <= 5; i++)
        {
            var itemRequest = new { Value = $"ITEM{i}", Label = $"Item {i}", SortOrder = i };
            var itemResponse = await _client.PostAsJsonAsync($"/api/managed-lists/{managedList.Id}/items", itemRequest, _fixture.JsonOptions);
            itemResponse.EnsureSuccessStatusCode();
            var item = await itemResponse.Content.ReadFromJsonAsync<CreateManagedListItemResponse>(_fixture.JsonOptions);
            Assert.NotNull(item);
            itemIds.Add(item.Id);
        }

        var questionRequest = new
        {
            ProjectId = project.Id,
            VariableName = $"Q1_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Version = 1,
            QuestionText = "Test question",
            SortOrder = 1
        };
        var questionResponse = await _client.PostAsJsonAsync("/api/questionnairelines", questionRequest, _fixture.JsonOptions);
        questionResponse.EnsureSuccessStatusCode();
        var question = await questionResponse.Content.ReadFromJsonAsync<AddQuestionnaireLineResponse>(_fixture.JsonOptions);
        Assert.NotNull(question);

        // Act - Save full selection
        var selectionRequest = new SaveQuestionSelectionRequest(
            project.Id,
            question.Id,
            managedList.Id,
            itemIds
        );
        var selectionResponse = await _client.PostAsJsonAsync("/api/subsets/save-selection", selectionRequest, _fixture.JsonOptions);

        // Assert
        selectionResponse.EnsureSuccessStatusCode();
        var result = await selectionResponse.Content.ReadFromJsonAsync<SaveQuestionSelectionResponse>(_fixture.JsonOptions);
        Assert.NotNull(result);
        Assert.True(result.IsFullSelection);
        Assert.Null(result.SubsetDefinitionId);
        Assert.Null(result.SubsetName);
    }

    [Fact]
    public async Task SaveQuestionSelection_SameSelection_ReusesSubset()
    {
        // Arrange - Create project, managed list, items, and two questions
        var projectRequest = new { Name = $"Test Project {Guid.NewGuid()}", Description = "Test", Status = ProjectStatus.Draft };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest, _fixture.JsonOptions);
        projectResponse.EnsureSuccessStatusCode();
        var project = await projectResponse.Content.ReadFromJsonAsync<CreateProjectResponse>(_fixture.JsonOptions);
        Assert.NotNull(project);

        var listRequest = new { ProjectId = project.Id, Name = $"BRANDS_{Guid.NewGuid().ToString().Substring(0, 8)}", Description = "Test brands list" };
        var listResponse = await _client.PostAsJsonAsync("/api/managed-lists", listRequest, _fixture.JsonOptions);
        listResponse.EnsureSuccessStatusCode();
        var managedList = await listResponse.Content.ReadFromJsonAsync<CreateManagedListResponse>(_fixture.JsonOptions);
        Assert.NotNull(managedList);

        var itemIds = new List<Guid>();
        for (int i = 1; i <= 10; i++)
        {
            var itemRequest = new { Value = $"ITEM{i}", Label = $"Item {i}", SortOrder = i };
            var itemResponse = await _client.PostAsJsonAsync($"/api/managed-lists/{managedList.Id}/items", itemRequest, _fixture.JsonOptions);
            itemResponse.EnsureSuccessStatusCode();
            var item = await itemResponse.Content.ReadFromJsonAsync<CreateManagedListItemResponse>(_fixture.JsonOptions);
            Assert.NotNull(item);
            itemIds.Add(item.Id);
        }

        var question1Request = new
        {
            ProjectId = project.Id,
            VariableName = $"Q1_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Version = 1,
            QuestionText = "Test question 1",
            SortOrder = 1
        };
        var question1Response = await _client.PostAsJsonAsync("/api/questionnairelines", question1Request, _fixture.JsonOptions);
        question1Response.EnsureSuccessStatusCode();
        var question1 = await question1Response.Content.ReadFromJsonAsync<AddQuestionnaireLineResponse>(_fixture.JsonOptions);
        Assert.NotNull(question1);

        var question2Request = new
        {
            ProjectId = project.Id,
            VariableName = $"Q2_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Version = 1,
            QuestionText = "Test question 2",
            SortOrder = 2
        };
        var question2Response = await _client.PostAsJsonAsync("/api/questionnairelines", question2Request, _fixture.JsonOptions);
        question2Response.EnsureSuccessStatusCode();
        var question2 = await question2Response.Content.ReadFromJsonAsync<AddQuestionnaireLineResponse>(_fixture.JsonOptions);
        Assert.NotNull(question2);

        // Act - Save same partial selection for both questions
        var selectedItems = itemIds.Take(5).ToList();
        
        var selection1Request = new SaveQuestionSelectionRequest(project.Id, question1.Id, managedList.Id, selectedItems);
        var selection1Response = await _client.PostAsJsonAsync("/api/subsets/save-selection", selection1Request, _fixture.JsonOptions);
        selection1Response.EnsureSuccessStatusCode();
        var result1 = await selection1Response.Content.ReadFromJsonAsync<SaveQuestionSelectionResponse>(_fixture.JsonOptions);
        Assert.NotNull(result1);

        var selection2Request = new SaveQuestionSelectionRequest(project.Id, question2.Id, managedList.Id, selectedItems);
        var selection2Response = await _client.PostAsJsonAsync("/api/subsets/save-selection", selection2Request, _fixture.JsonOptions);
        selection2Response.EnsureSuccessStatusCode();
        var result2 = await selection2Response.Content.ReadFromJsonAsync<SaveQuestionSelectionResponse>(_fixture.JsonOptions);
        Assert.NotNull(result2);

        // Assert - Both questions should use the same subset
        Assert.Equal(result1.SubsetDefinitionId, result2.SubsetDefinitionId);
        Assert.Equal(result1.SubsetName, result2.SubsetName);
    }

    [Fact]
    public async Task SaveQuestionSelection_SequentialNaming_NoGapReuse()
    {
        // Arrange - Create project, managed list, items, and three questions
        var projectRequest = new { Name = $"Test Project {Guid.NewGuid()}", Description = "Test", Status = ProjectStatus.Draft };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest, _fixture.JsonOptions);
        projectResponse.EnsureSuccessStatusCode();
        var project = await projectResponse.Content.ReadFromJsonAsync<CreateProjectResponse>(_fixture.JsonOptions);
        Assert.NotNull(project);

        var listRequest = new { ProjectId = project.Id, Name = $"BRANDS_{Guid.NewGuid().ToString().Substring(0, 8)}", Description = "Test brands list" };
        var listResponse = await _client.PostAsJsonAsync("/api/managed-lists", listRequest, _fixture.JsonOptions);
        listResponse.EnsureSuccessStatusCode();
        var managedList = await listResponse.Content.ReadFromJsonAsync<CreateManagedListResponse>(_fixture.JsonOptions);
        Assert.NotNull(managedList);

        var itemIds = new List<Guid>();
        for (int i = 1; i <= 10; i++)
        {
            var itemRequest = new { Value = $"ITEM{i}", Label = $"Item {i}", SortOrder = i };
            var itemResponse = await _client.PostAsJsonAsync($"/api/managed-lists/{managedList.Id}/items", itemRequest, _fixture.JsonOptions);
            itemResponse.EnsureSuccessStatusCode();
            var item = await itemResponse.Content.ReadFromJsonAsync<CreateManagedListItemResponse>(_fixture.JsonOptions);
            Assert.NotNull(item);
            itemIds.Add(item.Id);
        }

        var question1Request = new
        {
            ProjectId = project.Id,
            VariableName = $"Q1_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Version = 1,
            QuestionText = "Test question 1",
            SortOrder = 1
        };
        var question1Response = await _client.PostAsJsonAsync("/api/questionnairelines", question1Request, _fixture.JsonOptions);
        question1Response.EnsureSuccessStatusCode();
        var question1 = await question1Response.Content.ReadFromJsonAsync<AddQuestionnaireLineResponse>(_fixture.JsonOptions);
        Assert.NotNull(question1);

        var question2Request = new
        {
            ProjectId = project.Id,
            VariableName = $"Q2_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Version = 1,
            QuestionText = "Test question 2",
            SortOrder = 2
        };
        var question2Response = await _client.PostAsJsonAsync("/api/questionnairelines", question2Request, _fixture.JsonOptions);
        question2Response.EnsureSuccessStatusCode();
        var question2 = await question2Response.Content.ReadFromJsonAsync<AddQuestionnaireLineResponse>(_fixture.JsonOptions);
        Assert.NotNull(question2);

        var question3Request = new
        {
            ProjectId = project.Id,
            VariableName = $"Q3_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Version = 1,
            QuestionText = "Test question 3",
            SortOrder = 3
        };
        var question3Response = await _client.PostAsJsonAsync("/api/questionnairelines", question3Request, _fixture.JsonOptions);
        question3Response.EnsureSuccessStatusCode();
        var question3 = await question3Response.Content.ReadFromJsonAsync<AddQuestionnaireLineResponse>(_fixture.JsonOptions);
        Assert.NotNull(question3);

        // Act - Create three different subsets
        var selection1Request = new SaveQuestionSelectionRequest(project.Id, question1.Id, managedList.Id, itemIds.Take(3).ToList());
        var selection1Response = await _client.PostAsJsonAsync("/api/subsets/save-selection", selection1Request, _fixture.JsonOptions);
        selection1Response.EnsureSuccessStatusCode();
        var result1 = await selection1Response.Content.ReadFromJsonAsync<SaveQuestionSelectionResponse>(_fixture.JsonOptions);
        Assert.NotNull(result1);

        var selection2Request = new SaveQuestionSelectionRequest(project.Id, question2.Id, managedList.Id, itemIds.Take(5).ToList());
        var selection2Response = await _client.PostAsJsonAsync("/api/subsets/save-selection", selection2Request, _fixture.JsonOptions);
        selection2Response.EnsureSuccessStatusCode();
        var result2 = await selection2Response.Content.ReadFromJsonAsync<SaveQuestionSelectionResponse>(_fixture.JsonOptions);
        Assert.NotNull(result2);

        var selection3Request = new SaveQuestionSelectionRequest(project.Id, question3.Id, managedList.Id, itemIds.Take(7).ToList());
        var selection3Response = await _client.PostAsJsonAsync("/api/subsets/save-selection", selection3Request, _fixture.JsonOptions);
        selection3Response.EnsureSuccessStatusCode();
        var result3 = await selection3Response.Content.ReadFromJsonAsync<SaveQuestionSelectionResponse>(_fixture.JsonOptions);
        Assert.NotNull(result3);

        // Assert - Names should be sequential
        Assert.Contains("_SUB1", result1.SubsetName);
        Assert.Contains("_SUB2", result2.SubsetName);
        Assert.Contains("_SUB3", result3.SubsetName);
    }

    [Fact]
    public async Task GetSubsetDetails_ReturnsSubsetWithMembers()
    {
        // Arrange - Create subset first
        var projectRequest = new { Name = $"Test Project {Guid.NewGuid()}", Description = "Test", Status = ProjectStatus.Draft };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest, _fixture.JsonOptions);
        projectResponse.EnsureSuccessStatusCode();
        var project = await projectResponse.Content.ReadFromJsonAsync<CreateProjectResponse>(_fixture.JsonOptions);
        Assert.NotNull(project);

        var listRequest = new { ProjectId = project.Id, Name = $"BRANDS_{Guid.NewGuid().ToString().Substring(0, 8)}", Description = "Test brands list" };
        var listResponse = await _client.PostAsJsonAsync("/api/managed-lists", listRequest, _fixture.JsonOptions);
        listResponse.EnsureSuccessStatusCode();
        var managedList = await listResponse.Content.ReadFromJsonAsync<CreateManagedListResponse>(_fixture.JsonOptions);
        Assert.NotNull(managedList);

        var itemIds = new List<Guid>();
        for (int i = 1; i <= 10; i++)
        {
            var itemRequest = new { Value = $"ITEM{i}", Label = $"Item {i}", SortOrder = i };
            var itemResponse = await _client.PostAsJsonAsync($"/api/managed-lists/{managedList.Id}/items", itemRequest, _fixture.JsonOptions);
            itemResponse.EnsureSuccessStatusCode();
            var item = await itemResponse.Content.ReadFromJsonAsync<CreateManagedListItemResponse>(_fixture.JsonOptions);
            Assert.NotNull(item);
            itemIds.Add(item.Id);
        }

        var questionRequest = new
        {
            ProjectId = project.Id,
            VariableName = $"Q1_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Version = 1,
            QuestionText = "Test question",
            SortOrder = 1
        };
        var questionResponse = await _client.PostAsJsonAsync("/api/questionnairelines", questionRequest, _fixture.JsonOptions);
        questionResponse.EnsureSuccessStatusCode();
        var question = await questionResponse.Content.ReadFromJsonAsync<AddQuestionnaireLineResponse>(_fixture.JsonOptions);
        Assert.NotNull(question);

        var selectionRequest = new SaveQuestionSelectionRequest(project.Id, question.Id, managedList.Id, itemIds.Take(5).ToList());
        var selectionResponse = await _client.PostAsJsonAsync("/api/subsets/save-selection", selectionRequest, _fixture.JsonOptions);
        selectionResponse.EnsureSuccessStatusCode();
        var saveResult = await selectionResponse.Content.ReadFromJsonAsync<SaveQuestionSelectionResponse>(_fixture.JsonOptions);
        Assert.NotNull(saveResult);
        Assert.NotNull(saveResult.SubsetDefinitionId);

        // Act - Get subset details
        var detailsResponse = await _client.GetAsync($"/api/subsets/{saveResult.SubsetDefinitionId}");

        // Assert
        detailsResponse.EnsureSuccessStatusCode();
        var details = await detailsResponse.Content.ReadFromJsonAsync<GetSubsetDetailsResponse>(_fixture.JsonOptions);
        Assert.NotNull(details);
        Assert.Equal(saveResult.SubsetDefinitionId, details.Id);
        Assert.Equal(project.Id, details.ProjectId);
        Assert.Equal(managedList.Id, details.ManagedListId);
        Assert.Equal(5, details.Members.Count);
    }

    [Fact]
    public async Task GetSubsetsForProject_ReturnsAllSubsets()
    {
        // Arrange - Create project with multiple subsets
        var projectRequest = new { Name = $"Test Project {Guid.NewGuid()}", Description = "Test", Status = ProjectStatus.Draft };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest, _fixture.JsonOptions);
        projectResponse.EnsureSuccessStatusCode();
        var project = await projectResponse.Content.ReadFromJsonAsync<CreateProjectResponse>(_fixture.JsonOptions);
        Assert.NotNull(project);

        var listRequest = new { ProjectId = project.Id, Name = $"BRANDS_{Guid.NewGuid().ToString().Substring(0, 8)}", Description = "Test brands list" };
        var listResponse = await _client.PostAsJsonAsync("/api/managed-lists", listRequest, _fixture.JsonOptions);
        listResponse.EnsureSuccessStatusCode();
        var managedList = await listResponse.Content.ReadFromJsonAsync<CreateManagedListResponse>(_fixture.JsonOptions);
        Assert.NotNull(managedList);

        var itemIds = new List<Guid>();
        for (int i = 1; i <= 10; i++)
        {
            var itemRequest = new { Value = $"ITEM{i}", Label = $"Item {i}", SortOrder = i };
            var itemResponse = await _client.PostAsJsonAsync($"/api/managed-lists/{managedList.Id}/items", itemRequest, _fixture.JsonOptions);
            itemResponse.EnsureSuccessStatusCode();
            var item = await itemResponse.Content.ReadFromJsonAsync<CreateManagedListItemResponse>(_fixture.JsonOptions);
            Assert.NotNull(item);
            itemIds.Add(item.Id);
        }

        // Create two different subsets
        var question1Request = new
        {
            ProjectId = project.Id,
            VariableName = $"Q1_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Version = 1,
            QuestionText = "Test question 1",
            SortOrder = 1
        };
        var question1Response = await _client.PostAsJsonAsync("/api/questionnairelines", question1Request, _fixture.JsonOptions);
        question1Response.EnsureSuccessStatusCode();
        var question1 = await question1Response.Content.ReadFromJsonAsync<AddQuestionnaireLineResponse>(_fixture.JsonOptions);
        Assert.NotNull(question1);

        var question2Request = new
        {
            ProjectId = project.Id,
            VariableName = $"Q2_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Version = 1,
            QuestionText = "Test question 2",
            SortOrder = 2
        };
        var question2Response = await _client.PostAsJsonAsync("/api/questionnairelines", question2Request, _fixture.JsonOptions);
        question2Response.EnsureSuccessStatusCode();
        var question2 = await question2Response.Content.ReadFromJsonAsync<AddQuestionnaireLineResponse>(_fixture.JsonOptions);
        Assert.NotNull(question2);

        var selection1Request = new SaveQuestionSelectionRequest(project.Id, question1.Id, managedList.Id, itemIds.Take(3).ToList());
        await _client.PostAsJsonAsync("/api/subsets/save-selection", selection1Request, _fixture.JsonOptions);

        var selection2Request = new SaveQuestionSelectionRequest(project.Id, question2.Id, managedList.Id, itemIds.Take(5).ToList());
        await _client.PostAsJsonAsync("/api/subsets/save-selection", selection2Request, _fixture.JsonOptions);

        // Act - Get all subsets for project
        var subsetsResponse = await _client.GetAsync($"/api/subsets/project/{project.Id}");

        // Assert
        subsetsResponse.EnsureSuccessStatusCode();
        var subsetsResult = await subsetsResponse.Content.ReadFromJsonAsync<GetSubsetsForProjectResponse>(_fixture.JsonOptions);
        Assert.NotNull(subsetsResult);
        Assert.Equal(2, subsetsResult.Subsets.Count);
    }

    [Fact]
    public async Task SaveQuestionSelection_NonDraftProject_ReturnsError()
    {
        // Arrange - Create project in Active status
        var projectRequest = new { Name = $"Test Project {Guid.NewGuid()}", Description = "Test", Status = ProjectStatus.Active };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest, _fixture.JsonOptions);
        projectResponse.EnsureSuccessStatusCode();
        var project = await projectResponse.Content.ReadFromJsonAsync<CreateProjectResponse>(_fixture.JsonOptions);
        Assert.NotNull(project);

        var listRequest = new { ProjectId = project.Id, Name = $"BRANDS_{Guid.NewGuid().ToString().Substring(0, 8)}", Description = "Test brands list" };
        var listResponse = await _client.PostAsJsonAsync("/api/managed-lists", listRequest, _fixture.JsonOptions);
        listResponse.EnsureSuccessStatusCode();
        var managedList = await listResponse.Content.ReadFromJsonAsync<CreateManagedListResponse>(_fixture.JsonOptions);
        Assert.NotNull(managedList);

        var itemIds = new List<Guid>();
        for (int i = 1; i <= 5; i++)
        {
            var itemRequest = new { Value = $"ITEM{i}", Label = $"Item {i}", SortOrder = i };
            var itemResponse = await _client.PostAsJsonAsync($"/api/managed-lists/{managedList.Id}/items", itemRequest, _fixture.JsonOptions);
            itemResponse.EnsureSuccessStatusCode();
            var item = await itemResponse.Content.ReadFromJsonAsync<Api.Features.ManagedLists.CreateManagedListItemResponse>(_fixture.JsonOptions);
            Assert.NotNull(item);
            itemIds.Add(item.Id);
        }

        var questionRequest = new
        {
            ProjectId = project.Id,
            VariableName = $"Q1_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Version = 1,
            QuestionText = "Test question",
            SortOrder = 1
        };
        var questionResponse = await _client.PostAsJsonAsync("/api/questionnairelines", questionRequest, _fixture.JsonOptions);
        questionResponse.EnsureSuccessStatusCode();
        var question = await questionResponse.Content.ReadFromJsonAsync<AddQuestionnaireLineResponse>(_fixture.JsonOptions);
        Assert.NotNull(question);

        // Act - Try to save selection for non-Draft project
        var selectionRequest = new SaveQuestionSelectionRequest(project.Id, question.Id, managedList.Id, itemIds.Take(3).ToList());
        var selectionResponse = await _client.PostAsJsonAsync("/api/subsets/save-selection", selectionRequest, _fixture.JsonOptions);

        // Assert - Should return BadRequest
        Assert.Equal(HttpStatusCode.BadRequest, selectionResponse.StatusCode);
        var errorMessage = await selectionResponse.Content.ReadAsStringAsync();
        Assert.Contains("read-only", errorMessage, StringComparison.OrdinalIgnoreCase);
    }
}

// Response DTOs for deserialization
public record CreateProjectResponse(Guid Id, string Name);
public record CreateManagedListResponse(Guid Id, string Name);
public record AddQuestionnaireLineResponse(Guid Id, string VariableName);
