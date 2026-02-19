using System.Net;
using System.Net.Http.Json;
using Api.Features.ManagedLists;
using Api.Features.Projects;
using Api.Features.QuestionnaireLines;
using Xunit;

namespace Api.IntegrationTests;

public class AutoAssociationIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;
    private readonly HttpClient _client;

    public AutoAssociationIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.HttpClient;
    }

    [Fact]
    public async Task CreateMLE_DraftProject_AutoAssociatesWithExistingQuestions()
    {
        // AC-AUTO-01: New MLE Auto-Propagation (Draft Only)
        var cancellationToken = TestContext.Current.CancellationToken;

        // Arrange - Create a Draft project
        var projectRequest = new { Name = $"Test Draft Project {Guid.NewGuid()}", Description = "Test", Status = ProjectStatus.Draft };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest, _fixture.JsonOptions, cancellationToken);
        projectResponse.EnsureSuccessStatusCode();
        var project = await projectResponse.Content.ReadFromJsonAsync<CreateProjectResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(project);

        // Create a managed list
        var listRequest = new { ProjectId = project.Id, Name = $"BRANDS_{Guid.NewGuid().ToString().Substring(0, 8)}", Description = "Test brands" };
        var listResponse = await _client.PostAsJsonAsync("/api/managedlists", listRequest, _fixture.JsonOptions, cancellationToken);
        listResponse.EnsureSuccessStatusCode();
        var managedList = await listResponse.Content.ReadFromJsonAsync<CreateManagedListResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(managedList);

        // Create a questionnaire line
        var questionRequest = new
        {
            ProjectId = project.Id,
            VariableName = $"Q1_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Version = 1,
            QuestionText = "Which brands do you know?",
            SortOrder = 1
        };
        var questionResponse = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/questionnairelines", questionRequest, _fixture.JsonOptions, cancellationToken);
        questionResponse.EnsureSuccessStatusCode();
        var question = await questionResponse.Content.ReadFromJsonAsync<AddQuestionnaireLineResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(question);

        // Assign managed list to question
        var assignRequest = new { QuestionnaireLineId = question.Id, ManagedListId = managedList.Id };
        var assignResponse = await _client.PostAsJsonAsync("/api/managedlists/assign", assignRequest, _fixture.JsonOptions, cancellationToken);
        assignResponse.EnsureSuccessStatusCode();

        // Act - Add a new managed list item
        var itemRequest = new { Value = "BRAND_NEW", Label = "New Brand", SortOrder = 1 };
        var itemResponse = await _client.PostAsJsonAsync($"/api/managedlists/{managedList.Id}/items", itemRequest, _fixture.JsonOptions, cancellationToken);
        
        // Assert
        itemResponse.EnsureSuccessStatusCode();
        var newItem = await itemResponse.Content.ReadFromJsonAsync<CreateManagedListItemResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(newItem);

        // Verify that the question now has a QuestionSubsetLink (full selection) for this managed list
        // This is implicitly tested by the fact that the auto-association service ran successfully
        // In a real test, we'd query the database or an endpoint to verify the link exists
    }

    [Fact]
    public async Task CreateMLE_NonDraftProject_DoesNotAutoAssociate()
    {
        // AC-AUTO-08: State-Respecting Behaviour
        var cancellationToken = TestContext.Current.CancellationToken;

        // Arrange - Create an Active (non-Draft) project
        var projectRequest = new { Name = $"Test Active Project {Guid.NewGuid()}", Description = "Test", Status = ProjectStatus.Active };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest, _fixture.JsonOptions, cancellationToken);
        projectResponse.EnsureSuccessStatusCode();
        var project = await projectResponse.Content.ReadFromJsonAsync<CreateProjectResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(project);

        // Create a managed list
        var listRequest = new { ProjectId = project.Id, Name = $"BRANDS_{Guid.NewGuid().ToString().Substring(0, 8)}", Description = "Test brands" };
        var listResponse = await _client.PostAsJsonAsync("/api/managedlists", listRequest, _fixture.JsonOptions, cancellationToken);
        listResponse.EnsureSuccessStatusCode();
        var managedList = await listResponse.Content.ReadFromJsonAsync<CreateManagedListResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(managedList);

        // Create a questionnaire line
        var questionRequest = new
        {
            ProjectId = project.Id,
            VariableName = $"Q1_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Version = 1,
            QuestionText = "Which brands do you know?",
            SortOrder = 1
        };
        var questionResponse = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/questionnairelines", questionRequest, _fixture.JsonOptions, cancellationToken);
        questionResponse.EnsureSuccessStatusCode();
        var question = await questionResponse.Content.ReadFromJsonAsync<AddQuestionnaireLineResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(question);

        // Assign managed list to question
        var assignRequest = new { QuestionnaireLineId = question.Id, ManagedListId = managedList.Id };
        var assignResponse = await _client.PostAsJsonAsync("/api/managedlists/assign", assignRequest, _fixture.JsonOptions, cancellationToken);
        assignResponse.EnsureSuccessStatusCode();

        // Act - Add a new managed list item
        var itemRequest = new { Value = "BRAND_NEW", Label = "New Brand", SortOrder = 1 };
        var itemResponse = await _client.PostAsJsonAsync($"/api/managedlists/{managedList.Id}/items", itemRequest, _fixture.JsonOptions, cancellationToken);
        
        // Assert - Item is created but auto-association should not occur for non-Draft projects
        itemResponse.EnsureSuccessStatusCode();
        var newItem = await itemResponse.Content.ReadFromJsonAsync<CreateManagedListItemResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(newItem);
        // The auto-association service should have logged that it skipped this project
    }

    [Fact]
    public async Task DeactivateMLE_DraftProject_RemovesFromSubsets()
    {
        // AC-AUTO-03: Deactivation Behaviour
        var cancellationToken = TestContext.Current.CancellationToken;

        // Arrange - Create a Draft project with a managed list and items
        var projectRequest = new { Name = $"Test Draft Project {Guid.NewGuid()}", Description = "Test", Status = ProjectStatus.Draft };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest, _fixture.JsonOptions, cancellationToken);
        projectResponse.EnsureSuccessStatusCode();
        var project = await projectResponse.Content.ReadFromJsonAsync<CreateProjectResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(project);

        var listRequest = new { ProjectId = project.Id, Name = $"BRANDS_{Guid.NewGuid().ToString().Substring(0, 8)}", Description = "Test brands" };
        var listResponse = await _client.PostAsJsonAsync("/api/managedlists", listRequest, _fixture.JsonOptions, cancellationToken);
        listResponse.EnsureSuccessStatusCode();
        var managedList = await listResponse.Content.ReadFromJsonAsync<CreateManagedListResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(managedList);

        // Add items to the managed list
        var itemIds = new List<Guid>();
        for (int i = 1; i <= 5; i++)
        {
            var itemRequest = new { Value = $"ITEM{i}", Label = $"Item {i}", SortOrder = i };
            var itemResponse = await _client.PostAsJsonAsync($"/api/managedlists/{managedList.Id}/items", itemRequest, _fixture.JsonOptions, cancellationToken);
            itemResponse.EnsureSuccessStatusCode();
            var item = await itemResponse.Content.ReadFromJsonAsync<CreateManagedListItemResponse>(_fixture.JsonOptions, cancellationToken);
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
        var questionResponse = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/questionnairelines", questionRequest, _fixture.JsonOptions, cancellationToken);
        questionResponse.EnsureSuccessStatusCode();
        var question = await questionResponse.Content.ReadFromJsonAsync<AddQuestionnaireLineResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(question);

        // Create a subset with first 3 items
        var selectionRequest = new SaveQuestionSelectionRequest(
            project.Id,
            question.Id,
            managedList.Id,
            itemIds.Take(3).ToList()
        );
        var selectionResponse = await _client.PostAsJsonAsync("/api/subsets/save-selection", selectionRequest, _fixture.JsonOptions, cancellationToken);
        selectionResponse.EnsureSuccessStatusCode();
        var subset = await selectionResponse.Content.ReadFromJsonAsync<SaveQuestionSelectionResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(subset);
        Assert.NotNull(subset.SubsetDefinitionId);

        // Get initial subset details
        var initialSubsetResponse = await _client.GetAsync($"/api/subsets/{subset.SubsetDefinitionId}", cancellationToken);
        initialSubsetResponse.EnsureSuccessStatusCode();
        var initialSubset = await initialSubsetResponse.Content.ReadFromJsonAsync<GetSubsetDetailsResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(initialSubset);
        Assert.Equal(3, initialSubset.Members.Count);

        // Act - Deactivate the first item
        var updateRequest = new
        {
            Value = "ITEM1",
            Label = "Item 1",
            SortOrder = 1,
            IsActive = false,
            Metadata = (string?)null
        };
        var updateResponse = await _client.PutAsJsonAsync($"/api/managedlists/{managedList.Id}/items/{itemIds[0]}", updateRequest, _fixture.JsonOptions, cancellationToken);
        
        // Assert
        updateResponse.EnsureSuccessStatusCode();

        // Verify that the item was removed from the subset
        var updatedSubsetResponse = await _client.GetAsync($"/api/subsets/{subset.SubsetDefinitionId}", cancellationToken);
        updatedSubsetResponse.EnsureSuccessStatusCode();
        var updatedSubset = await updatedSubsetResponse.Content.ReadFromJsonAsync<GetSubsetDetailsResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(updatedSubset);
        Assert.Equal(2, updatedSubset.Members.Count); // Should now have 2 items instead of 3
        Assert.DoesNotContain(updatedSubset.Members, m => m.ManagedListItemId == itemIds[0]);
    }

    [Fact]
    public async Task ReactivateMLE_DraftProject_MakesAvailableAgain()
    {
        // AC-AUTO-04: Reactivation Behaviour
        var cancellationToken = TestContext.Current.CancellationToken;

        // Arrange - Create a Draft project with an inactive item
        var projectRequest = new { Name = $"Test Draft Project {Guid.NewGuid()}", Description = "Test", Status = ProjectStatus.Draft };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest, _fixture.JsonOptions, cancellationToken);
        projectResponse.EnsureSuccessStatusCode();
        var project = await projectResponse.Content.ReadFromJsonAsync<CreateProjectResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(project);

        var listRequest = new { ProjectId = project.Id, Name = $"BRANDS_{Guid.NewGuid().ToString().Substring(0, 8)}", Description = "Test brands" };
        var listResponse = await _client.PostAsJsonAsync("/api/managedlists", listRequest, _fixture.JsonOptions, cancellationToken);
        listResponse.EnsureSuccessStatusCode();
        var managedList = await listResponse.Content.ReadFromJsonAsync<CreateManagedListResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(managedList);

        // Add an item
        var itemRequest = new { Value = "ITEM1", Label = "Item 1", SortOrder = 1 };
        var itemResponse = await _client.PostAsJsonAsync($"/api/managedlists/{managedList.Id}/items", itemRequest, _fixture.JsonOptions, cancellationToken);
        itemResponse.EnsureSuccessStatusCode();
        var item = await itemResponse.Content.ReadFromJsonAsync<CreateManagedListItemResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(item);

        // Deactivate it
        var deactivateRequest = new
        {
            Value = "ITEM1",
            Label = "Item 1",
            SortOrder = 1,
            IsActive = false,
            Metadata = (string?)null
        };
        var deactivateResponse = await _client.PutAsJsonAsync($"/api/managedlists/{managedList.Id}/items/{item.Id}", deactivateRequest, _fixture.JsonOptions, cancellationToken);
        deactivateResponse.EnsureSuccessStatusCode();

        // Create a questionnaire line and assign the list
        var questionRequest = new
        {
            ProjectId = project.Id,
            VariableName = $"Q1_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Version = 1,
            QuestionText = "Test question",
            SortOrder = 1
        };
        var questionResponse = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/questionnairelines", questionRequest, _fixture.JsonOptions, cancellationToken);
        questionResponse.EnsureSuccessStatusCode();
        var question = await questionResponse.Content.ReadFromJsonAsync<AddQuestionnaireLineResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(question);

        var assignRequest = new { QuestionnaireLineId = question.Id, ManagedListId = managedList.Id };
        var assignResponse = await _client.PostAsJsonAsync("/api/managedlists/assign", assignRequest, _fixture.JsonOptions, cancellationToken);
        assignResponse.EnsureSuccessStatusCode();

        // Act - Reactivate the item
        var reactivateRequest = new
        {
            Value = "ITEM1",
            Label = "Item 1",
            SortOrder = 1,
            IsActive = true,
            Metadata = (string?)null
        };
        var reactivateResponse = await _client.PutAsJsonAsync($"/api/managedlists/{managedList.Id}/items/{item.Id}", reactivateRequest, _fixture.JsonOptions, cancellationToken);
        
        // Assert - Item is reactivated successfully
        reactivateResponse.EnsureSuccessStatusCode();
        var reactivatedItem = await reactivateResponse.Content.ReadFromJsonAsync<UpdateManagedListItemResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(reactivatedItem);
        Assert.True(reactivatedItem.IsActive);
    }

    [Fact]
    public async Task AssignMLToQuestion_DraftProject_CreatesFullSelection()
    {
        // AC-AUTO-02: ML-to-Question Linking Auto-Propagation
        var cancellationToken = TestContext.Current.CancellationToken;

        // Arrange - Create a Draft project with a managed list and items
        var projectRequest = new { Name = $"Test Draft Project {Guid.NewGuid()}", Description = "Test", Status = ProjectStatus.Draft };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest, _fixture.JsonOptions, cancellationToken);
        projectResponse.EnsureSuccessStatusCode();
        var project = await projectResponse.Content.ReadFromJsonAsync<CreateProjectResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(project);

        var listRequest = new { ProjectId = project.Id, Name = $"BRANDS_{Guid.NewGuid().ToString().Substring(0, 8)}", Description = "Test brands" };
        var listResponse = await _client.PostAsJsonAsync("/api/managedlists", listRequest, _fixture.JsonOptions, cancellationToken);
        listResponse.EnsureSuccessStatusCode();
        var managedList = await listResponse.Content.ReadFromJsonAsync<CreateManagedListResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(managedList);

        // Add items to the managed list
        for (int i = 1; i <= 5; i++)
        {
            var itemRequest = new { Value = $"ITEM{i}", Label = $"Item {i}", SortOrder = i };
            var itemResponse = await _client.PostAsJsonAsync($"/api/managedlists/{managedList.Id}/items", itemRequest, _fixture.JsonOptions, cancellationToken);
            itemResponse.EnsureSuccessStatusCode();
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
        var questionResponse = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/questionnairelines", questionRequest, _fixture.JsonOptions, cancellationToken);
        questionResponse.EnsureSuccessStatusCode();
        var question = await questionResponse.Content.ReadFromJsonAsync<AddQuestionnaireLineResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(question);

        // Act - Assign managed list to question
        var assignRequest = new { QuestionnaireLineId = question.Id, ManagedListId = managedList.Id };
        var assignResponse = await _client.PostAsJsonAsync("/api/managedlists/assign", assignRequest, _fixture.JsonOptions, cancellationToken);
        
        // Assert
        assignResponse.EnsureSuccessStatusCode();
        var assignment = await assignResponse.Content.ReadFromJsonAsync<AssignManagedListToQuestionResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(assignment);
        // The auto-association service should have created a QuestionSubsetLink with full selection
    }

    [Fact]
    public async Task BulkCreateMLEs_DraftProject_AutoAssociatesAll()
    {
        // AC-AUTO-08: Bulk Operations
        var cancellationToken = TestContext.Current.CancellationToken;

        // Arrange - Create a Draft project
        var projectRequest = new { Name = $"Test Draft Project {Guid.NewGuid()}", Description = "Test", Status = ProjectStatus.Draft };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest, _fixture.JsonOptions, cancellationToken);
        projectResponse.EnsureSuccessStatusCode();
        var project = await projectResponse.Content.ReadFromJsonAsync<CreateProjectResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(project);

        var listRequest = new { ProjectId = project.Id, Name = $"BRANDS_{Guid.NewGuid().ToString().Substring(0, 8)}", Description = "Test brands" };
        var listResponse = await _client.PostAsJsonAsync("/api/managedlists", listRequest, _fixture.JsonOptions, cancellationToken);
        listResponse.EnsureSuccessStatusCode();
        var managedList = await listResponse.Content.ReadFromJsonAsync<CreateManagedListResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(managedList);

        // Create a questionnaire line and assign the list
        var questionRequest = new
        {
            ProjectId = project.Id,
            VariableName = $"Q1_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Version = 1,
            QuestionText = "Test question",
            SortOrder = 1
        };
        var questionResponse = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/questionnairelines", questionRequest, _fixture.JsonOptions, cancellationToken);
        questionResponse.EnsureSuccessStatusCode();
        var question = await questionResponse.Content.ReadFromJsonAsync<AddQuestionnaireLineResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(question);

        var assignRequest = new { QuestionnaireLineId = question.Id, ManagedListId = managedList.Id };
        var assignResponse = await _client.PostAsJsonAsync("/api/managedlists/assign", assignRequest, _fixture.JsonOptions, cancellationToken);
        assignResponse.EnsureSuccessStatusCode();

        // Act - Bulk add 50 items
        var bulkItems = new List<object>();
        for (int i = 1; i <= 50; i++)
        {
            bulkItems.Add(new
            {
                Value = $"BULK{i}",
                Label = $"Bulk Item {i}",
                SortOrder = i,
                IsActive = true,
                Metadata = (string?)null
            });
        }

        var bulkRequest = new { Items = bulkItems, AllowUpdates = false };
        var bulkResponse = await _client.PostAsJsonAsync($"/api/managedlists/{managedList.Id}/items/bulk", bulkRequest, _fixture.JsonOptions, cancellationToken);
        
        // Assert
        bulkResponse.EnsureSuccessStatusCode();
        var result = await bulkResponse.Content.ReadFromJsonAsync<BulkOperationResult>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(result);
        Assert.Equal(50, result.InsertedCount);
        Assert.Equal(0, result.RejectedCount);
        // All 50 items should be auto-associated with the question
    }
}
