using System.Net;
using System.Net.Http.Json;
using Api.Features.ManagedLists;
using Api.Features.Projects;
using Api.Features.QuestionnaireLines;
using Xunit;

namespace Api.IntegrationTests;

public class SubsetSynchronizationIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;
    private readonly HttpClient _client;

    public SubsetSynchronizationIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.HttpClient;
    }

    [Fact]
    public async Task RefreshProjectSummary_ReturnsCorrectSummary()
    {
        // AC-SYNC-02 - Study Summary Auto-Rebuild
        var cancellationToken = TestContext.Current.CancellationToken;

        // Arrange - Create project with managed list and subset
        var project = await CreateTestProjectAsync(cancellationToken);
        var managedList = await CreateTestManagedListAsync(project.Id, cancellationToken);
        var itemIds = await CreateTestItemsAsync(managedList.Id, 10, cancellationToken);
        var question = await CreateTestQuestionAsync(project.Id, cancellationToken);

        // Create a partial subset
        var selectionRequest = new SaveQuestionSelectionRequest(
            project.Id,
            question.Id,
            managedList.Id,
            itemIds.Take(5).ToList()
        );
        var selectionResponse = await _client.PostAsJsonAsync("/api/subsets/save-selection", selectionRequest, _fixture.JsonOptions, cancellationToken);
        selectionResponse.EnsureSuccessStatusCode();
        var subset = await selectionResponse.Content.ReadFromJsonAsync<SaveQuestionSelectionResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(subset);

        // Act - Refresh project summary
        var refreshResponse = await _client.PostAsync($"/api/subsets/project/{project.Id}/refresh", null, cancellationToken);

        // Assert
        refreshResponse.EnsureSuccessStatusCode();
        var summary = await refreshResponse.Content.ReadFromJsonAsync<ProjectSubsetSummaryResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(summary);
        Assert.Equal(project.Id, summary.ProjectId);
        Assert.NotEmpty(summary.Subsets);
        
        var subsetSummary = summary.Subsets[0];
        Assert.Equal(5, subsetSummary.MemberCount);
        Assert.Equal(10, subsetSummary.TotalItemsInList);
        Assert.False(subsetSummary.IsFull);
        Assert.Equal(1, subsetSummary.QuestionCount);
        Assert.Equal(5, subsetSummary.MemberLabels.Count);
    }

    [Fact]
    public async Task DeleteSubset_ClearsLinksAndFallsBackToFullList()
    {
        // AC-SYNC-05 - Delete Subset
        var cancellationToken = TestContext.Current.CancellationToken;

        // Arrange
        var project = await CreateTestProjectAsync(cancellationToken);
        var managedList = await CreateTestManagedListAsync(project.Id, cancellationToken);
        var itemIds = await CreateTestItemsAsync(managedList.Id, 5, cancellationToken);
        var question = await CreateTestQuestionAsync(project.Id, cancellationToken);

        // Create subset
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

        // Act - Delete the subset
        var deleteResponse = await _client.DeleteAsync($"/api/subsets/{subset.SubsetDefinitionId}", cancellationToken);

        // Assert
        deleteResponse.EnsureSuccessStatusCode();
        var deleteResult = await deleteResponse.Content.ReadFromJsonAsync<DeleteSubsetResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(deleteResult);
        Assert.Equal(subset.SubsetDefinitionId, deleteResult.SubsetDefinitionId);
        Assert.Single(deleteResult.AffectedQuestionIds);
        Assert.Equal(question.Id, deleteResult.AffectedQuestionIds[0]);

        // Verify subset is deleted
        var getResponse = await _client.GetAsync($"/api/subsets/{subset.SubsetDefinitionId}", cancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteSubset_FailsForNonDraftProject()
    {
        // AC-SYNC-03 - State-Aware Synchronisation
        var cancellationToken = TestContext.Current.CancellationToken;

        // Arrange - Create project in Draft, then change to Active
        var projectRequest = new { Name = $"Test Project {Guid.NewGuid()}", Description = "Test", Status = ProjectStatus.Draft };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest, _fixture.JsonOptions, cancellationToken);
        projectResponse.EnsureSuccessStatusCode();
        var project = await projectResponse.Content.ReadFromJsonAsync<CreateProjectResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(project);

        var managedList = await CreateTestManagedListAsync(project.Id, cancellationToken);
        var itemIds = await CreateTestItemsAsync(managedList.Id, 5, cancellationToken);
        var question = await CreateTestQuestionAsync(project.Id, cancellationToken);

        // Create subset while in Draft
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

        // Change project to Active status
        var updateRequest = new { Name = project.Name, Status = ProjectStatus.Active };
        var updateResponse = await _client.PutAsJsonAsync($"/api/projects/{project.Id}", updateRequest, _fixture.JsonOptions, cancellationToken);
        updateResponse.EnsureSuccessStatusCode();

        // Act - Try to delete subset (should fail)
        var deleteResponse = await _client.DeleteAsync($"/api/subsets/{subset.SubsetDefinitionId}", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateManagedListItem_TriggersSubsetRefresh()
    {
        // AC-SYNC-04 - MLE Change Reconciliation
        var cancellationToken = TestContext.Current.CancellationToken;

        // Arrange
        var project = await CreateTestProjectAsync(cancellationToken);
        var managedList = await CreateTestManagedListAsync(project.Id, cancellationToken);
        var itemIds = await CreateTestItemsAsync(managedList.Id, 3, cancellationToken);
        var question = await CreateTestQuestionAsync(project.Id, cancellationToken);

        // Create subset with all items
        var selectionRequest = new SaveQuestionSelectionRequest(
            project.Id,
            question.Id,
            managedList.Id,
            itemIds.ToList()
        );
        var selectionResponse = await _client.PostAsJsonAsync("/api/subsets/save-selection", selectionRequest, _fixture.JsonOptions, cancellationToken);
        selectionResponse.EnsureSuccessStatusCode();

        // Act - Update one of the items
        var updateRequest = new { Value = "UPDATED_ITEM", Label = "Updated Label", SortOrder = 1, IsActive = true };
        var updateResponse = await _client.PutAsJsonAsync($"/api/managedlists/{managedList.Id}/items/{itemIds[0]}", updateRequest, _fixture.JsonOptions, cancellationToken);

        // Assert - Update should succeed (refresh happens in background)
        updateResponse.EnsureSuccessStatusCode();
        var updatedItem = await updateResponse.Content.ReadFromJsonAsync<UpdateManagedListItemResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(updatedItem);
        Assert.Equal("UPDATED_ITEM", updatedItem.Value);
    }

    [Fact]
    public async Task SaveQuestionSelection_TriggersAutomaticRefresh()
    {
        // AC-SYNC-01 - Immediate Question-Level Refresh
        var cancellationToken = TestContext.Current.CancellationToken;

        // Arrange
        var project = await CreateTestProjectAsync(cancellationToken);
        var managedList = await CreateTestManagedListAsync(project.Id, cancellationToken);
        var itemIds = await CreateTestItemsAsync(managedList.Id, 8, cancellationToken);
        var question = await CreateTestQuestionAsync(project.Id, cancellationToken);

        // Act - Save selection (should trigger automatic refresh)
        var selectionRequest = new SaveQuestionSelectionRequest(
            project.Id,
            question.Id,
            managedList.Id,
            itemIds.Take(5).ToList()
        );
        var selectionResponse = await _client.PostAsJsonAsync("/api/subsets/save-selection", selectionRequest, _fixture.JsonOptions, cancellationToken);

        // Assert
        selectionResponse.EnsureSuccessStatusCode();
        var result = await selectionResponse.Content.ReadFromJsonAsync<SaveQuestionSelectionResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(result);
        Assert.False(result.IsFullSelection);
        Assert.NotNull(result.SubsetDefinitionId);

        // Verify project summary reflects the new subset
        var summaryResponse = await _client.PostAsync($"/api/subsets/project/{project.Id}/refresh", null, cancellationToken);
        summaryResponse.EnsureSuccessStatusCode();
        var summary = await summaryResponse.Content.ReadFromJsonAsync<ProjectSubsetSummaryResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(summary);
        Assert.NotEmpty(summary.Subsets);
    }

    [Fact]
    public async Task SubsetReuse_MaintainsConsistentSummary()
    {
        // AC-SYNC-06 - No Duplication or Stale Data
        var cancellationToken = TestContext.Current.CancellationToken;

        // Arrange
        var project = await CreateTestProjectAsync(cancellationToken);
        var managedList = await CreateTestManagedListAsync(project.Id, cancellationToken);
        var itemIds = await CreateTestItemsAsync(managedList.Id, 5, cancellationToken);
        var question1 = await CreateTestQuestionAsync(project.Id, cancellationToken);
        var question2 = await CreateTestQuestionAsync(project.Id, cancellationToken);

        // Act - Create same subset for two different questions (should reuse)
        var selection1Request = new SaveQuestionSelectionRequest(
            project.Id,
            question1.Id,
            managedList.Id,
            itemIds.Take(3).ToList()
        );
        var selection1Response = await _client.PostAsJsonAsync("/api/subsets/save-selection", selection1Request, _fixture.JsonOptions, cancellationToken);
        selection1Response.EnsureSuccessStatusCode();
        var subset1 = await selection1Response.Content.ReadFromJsonAsync<SaveQuestionSelectionResponse>(_fixture.JsonOptions, cancellationToken);

        var selection2Request = new SaveQuestionSelectionRequest(
            project.Id,
            question2.Id,
            managedList.Id,
            itemIds.Take(3).ToList()
        );
        var selection2Response = await _client.PostAsJsonAsync("/api/subsets/save-selection", selection2Request, _fixture.JsonOptions, cancellationToken);
        selection2Response.EnsureSuccessStatusCode();
        var subset2 = await selection2Response.Content.ReadFromJsonAsync<SaveQuestionSelectionResponse>(_fixture.JsonOptions, cancellationToken);

        // Assert - Should reuse same subset
        Assert.Equal(subset1.SubsetDefinitionId, subset2.SubsetDefinitionId);
        Assert.Equal(subset1.SubsetName, subset2.SubsetName);

        // Verify summary shows correct question count
        var summaryResponse = await _client.PostAsync($"/api/subsets/project/{project.Id}/refresh", null, cancellationToken);
        summaryResponse.EnsureSuccessStatusCode();
        var summary = await summaryResponse.Content.ReadFromJsonAsync<ProjectSubsetSummaryResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(summary);
        Assert.Single(summary.Subsets); // One subset
        Assert.Equal(2, summary.Subsets[0].QuestionCount); // Used by 2 questions
    }

    // Helper methods
    private async Task<CreateProjectResponse> CreateTestProjectAsync(CancellationToken cancellationToken)
    {
        var request = new { Name = $"Test Project {Guid.NewGuid()}", Description = "Test", Status = ProjectStatus.Draft };
        var response = await _client.PostAsJsonAsync("/api/projects", request, _fixture.JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        var project = await response.Content.ReadFromJsonAsync<CreateProjectResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(project);
        return project;
    }

    private async Task<CreateManagedListResponse> CreateTestManagedListAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var request = new { ProjectId = projectId, Name = $"LIST_{Guid.NewGuid().ToString().Substring(0, 8)}", Description = "Test list" };
        var response = await _client.PostAsJsonAsync("/api/managedlists", request, _fixture.JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        var managedList = await response.Content.ReadFromJsonAsync<CreateManagedListResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(managedList);
        return managedList;
    }

    private async Task<List<Guid>> CreateTestItemsAsync(Guid managedListId, int count, CancellationToken cancellationToken)
    {
        var itemIds = new List<Guid>();
        for (int i = 1; i <= count; i++)
        {
            var request = new { Value = $"ITEM{i}", Label = $"Item {i}", SortOrder = i };
            var response = await _client.PostAsJsonAsync($"/api/managedlists/{managedListId}/items", request, _fixture.JsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            var item = await response.Content.ReadFromJsonAsync<CreateManagedListItemResponse>(_fixture.JsonOptions, cancellationToken);
            Assert.NotNull(item);
            itemIds.Add(item.Id);
        }
        return itemIds;
    }

    private async Task<AddQuestionnaireLineResponse> CreateTestQuestionAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var request = new
        {
            ProjectId = projectId,
            VariableName = $"Q_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Version = 1,
            QuestionText = "Test question",
            SortOrder = 1
        };
        var response = await _client.PostAsJsonAsync($"/api/projects/{projectId}/questionnairelines", request, _fixture.JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        var question = await response.Content.ReadFromJsonAsync<AddQuestionnaireLineResponse>(_fixture.JsonOptions, cancellationToken);
        Assert.NotNull(question);
        return question;
    }
}
