using Api.Data;
using Api.Features.ManagedLists;
using Api.Features.Projects;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

public class ManagedListItemIntegrationTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.HttpClient;
    private readonly System.Text.Json.JsonSerializerOptions _jsonOptions = fixture.JsonOptions;

    [Fact]
    public async Task CreateManagedListItem_WithValidData_ReturnsCreated()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (projectId, managedListId) = await CreateProjectAndManagedList();
        var request = new CreateManagedListItemRequest("TEST_CODE", "Test Label", 1, "{\"key\":\"value\"}");

        // Act
        var response = await _client.PostAsJsonAsync($"/api/managedlists/{managedListId}/items", request, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CreateManagedListItemResponse>(_jsonOptions, cancellationToken);
        Assert.NotNull(result);
        Assert.Equal("TEST_CODE", result.Value);
        Assert.Equal("Test Label", result.Label);
        Assert.Equal(1, result.SortOrder);
        Assert.True(result.IsActive);
        Assert.Equal("{\"key\":\"value\"}", result.Metadata);
    }

    [Fact]
    public async Task CreateManagedListItem_WithDuplicateCode_ReturnsConflict()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (projectId, managedListId) = await CreateProjectAndManagedList();
        var request1 = new CreateManagedListItemRequest("TEST_CODE", "Test Label 1", 1);
        var request2 = new CreateManagedListItemRequest("TEST_CODE", "Test Label 2", 2);

        await _client.PostAsJsonAsync($"/api/managedlists/{managedListId}/items", request1, cancellationToken);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/managedlists/{managedListId}/items", request2, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateManagedListItem_WithDuplicateCodeCaseInsensitive_ReturnsConflict()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (projectId, managedListId) = await CreateProjectAndManagedList();
        var request1 = new CreateManagedListItemRequest("TEST_CODE", "Test Label 1", 1);
        var request2 = new CreateManagedListItemRequest("test_code", "Test Label 2", 2);

        await _client.PostAsJsonAsync($"/api/managedlists/{managedListId}/items", request1, cancellationToken);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/managedlists/{managedListId}/items", request2, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateManagedListItem_WithInvalidCodeFormat_ReturnsValidationError()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (projectId, managedListId) = await CreateProjectAndManagedList();
        var request = new CreateManagedListItemRequest("123_INVALID", "Test Label", 1);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/managedlists/{managedListId}/items", request, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateManagedListItem_WithNewCode_Success()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (projectId, managedListId) = await CreateProjectAndManagedList();
        var createRequest = new CreateManagedListItemRequest("OLD_CODE", "Old Label", 1);
        var createResponse = await _client.PostAsJsonAsync($"/api/managedlists/{managedListId}/items", createRequest, cancellationToken);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateManagedListItemResponse>(_jsonOptions, cancellationToken);
        
        var updateRequest = new UpdateManagedListItemRequest("NEW_CODE", "New Label", 2, true, "{\"updated\":true}");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/managedlists/{managedListId}/items/{created!.Id}", updateRequest, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<UpdateManagedListItemResponse>(_jsonOptions, cancellationToken);
        Assert.NotNull(result);
        Assert.Equal("NEW_CODE", result.Value);
        Assert.Equal("New Label", result.Label);
        Assert.Equal(2, result.SortOrder);
        Assert.Equal("{\"updated\":true}", result.Metadata);
    }

    [Fact]
    public async Task UpdateManagedListItem_WithDuplicateCode_ReturnsConflict()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (projectId, managedListId) = await CreateProjectAndManagedList();
        var createRequest1 = new CreateManagedListItemRequest("CODE_1", "Label 1", 1);
        var createRequest2 = new CreateManagedListItemRequest("CODE_2", "Label 2", 2);
        
        await _client.PostAsJsonAsync($"/api/managedlists/{managedListId}/items", createRequest1, cancellationToken);
        var createResponse2 = await _client.PostAsJsonAsync($"/api/managedlists/{managedListId}/items", createRequest2, cancellationToken);
        var created2 = await createResponse2.Content.ReadFromJsonAsync<CreateManagedListItemResponse>(_jsonOptions, cancellationToken);
        
        var updateRequest = new UpdateManagedListItemRequest("CODE_1", "Updated Label 2", 3, true);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/managedlists/{managedListId}/items/{created2!.Id}", updateRequest, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task BulkAddOrUpdateManagedListItems_WithMixedValidInvalid_ReturnsPartialSuccess()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (projectId, managedListId) = await CreateProjectAndManagedList();
        var bulkRequest = new BulkAddOrUpdateManagedListItemsRequest(
            new List<BulkManagedListItemInput>
            {
                new BulkManagedListItemInput("VALID_CODE_1", "Valid Label 1", 1),
                new BulkManagedListItemInput("VALID_CODE_2", "Valid Label 2", 2),
                new BulkManagedListItemInput("123_INVALID", "Invalid Label", 3), // Invalid - starts with number
                new BulkManagedListItemInput("VALID_CODE_3", "Valid Label 3", 4),
            },
            true);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/managedlists/{managedListId}/items/bulk", bulkRequest, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<BulkOperationResult>(_jsonOptions, cancellationToken);
        Assert.NotNull(result);
        Assert.Equal(4, result.TotalRows);
        Assert.Equal(3, result.InsertedCount);
        Assert.Equal(0, result.UpdatedCount);
        Assert.Equal(0, result.SkippedCount);
        Assert.Equal(1, result.RejectedCount);
        
        var rejectedRow = result.Results.FirstOrDefault(r => r.Status == "rejected");
        Assert.NotNull(rejectedRow);
        Assert.Equal("123_INVALID", rejectedRow.Value);
    }

    [Fact]
    public async Task BulkAddOrUpdateManagedListItems_WithDuplicatesInBatch_RejectsSecondOccurrence()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (projectId, managedListId) = await CreateProjectAndManagedList();
        var bulkRequest = new BulkAddOrUpdateManagedListItemsRequest(
            new List<BulkManagedListItemInput>
            {
                new BulkManagedListItemInput("DUP_CODE", "Label 1", 1),
                new BulkManagedListItemInput("DUP_CODE", "Label 2", 2), // Duplicate in batch
                new BulkManagedListItemInput("UNIQUE_CODE", "Label 3", 3),
            },
            true);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/managedlists/{managedListId}/items/bulk", bulkRequest, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<BulkOperationResult>(_jsonOptions, cancellationToken);
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalRows);
        Assert.Equal(2, result.InsertedCount);
        Assert.Equal(1, result.RejectedCount);
    }

    [Fact]
    public async Task BulkAddOrUpdateManagedListItems_WithExistingCodeAndAllowUpdatesTrue_UpdatesItem()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (projectId, managedListId) = await CreateProjectAndManagedList();
        var createRequest = new CreateManagedListItemRequest("EXISTING_CODE", "Original Label", 1);
        await _client.PostAsJsonAsync($"/api/managedlists/{managedListId}/items", createRequest, cancellationToken);
        
        var bulkRequest = new BulkAddOrUpdateManagedListItemsRequest(
            new List<BulkManagedListItemInput>
            {
                new BulkManagedListItemInput("EXISTING_CODE", "Updated Label", 5),
                new BulkManagedListItemInput("NEW_CODE", "New Label", 2),
            },
            true);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/managedlists/{managedListId}/items/bulk", bulkRequest, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<BulkOperationResult>(_jsonOptions, cancellationToken);
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalRows);
        Assert.Equal(1, result.InsertedCount);
        Assert.Equal(1, result.UpdatedCount);
        Assert.Equal(0, result.RejectedCount);
        Assert.Equal(0, result.SkippedCount);
    }

    [Fact]
    public async Task BulkAddOrUpdateManagedListItems_WithExistingCodeAndAllowUpdatesFalse_SkipsItem()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (projectId, managedListId) = await CreateProjectAndManagedList();
        var createRequest = new CreateManagedListItemRequest("EXISTING_CODE", "Original Label", 1);
        await _client.PostAsJsonAsync($"/api/managedlists/{managedListId}/items", createRequest, cancellationToken);
        
        var bulkRequest = new BulkAddOrUpdateManagedListItemsRequest(
            new List<BulkManagedListItemInput>
            {
                new BulkManagedListItemInput("EXISTING_CODE", "Updated Label", 5),
                new BulkManagedListItemInput("NEW_CODE", "New Label", 2),
            },
            false); // AllowUpdates = false

        // Act
        var response = await _client.PostAsJsonAsync($"/api/managedlists/{managedListId}/items/bulk", bulkRequest, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<BulkOperationResult>(_jsonOptions, cancellationToken);
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalRows);
        Assert.Equal(1, result.InsertedCount);
        Assert.Equal(0, result.UpdatedCount);
        Assert.Equal(1, result.SkippedCount);
        Assert.Equal(0, result.RejectedCount);
    }

    [Fact]
    public async Task GetManagedListById_ReturnsItemsOrderedBySortOrderThenLabel()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (projectId, managedListId) = await CreateProjectAndManagedList();
        
        // Create items with different sort orders
        await _client.PostAsJsonAsync($"/api/managedlists/{managedListId}/items", 
            new CreateManagedListItemRequest("CODE_C", "Zebra", 2), cancellationToken);
        await _client.PostAsJsonAsync($"/api/managedlists/{managedListId}/items", 
            new CreateManagedListItemRequest("CODE_A", "Apple", 1), cancellationToken);
        await _client.PostAsJsonAsync($"/api/managedlists/{managedListId}/items", 
            new CreateManagedListItemRequest("CODE_B", "Banana", 1), cancellationToken);

        // Act
        var response = await _client.GetAsync($"/api/managedlists/{managedListId}", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetManagedListByIdResponse>(_jsonOptions, cancellationToken);
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        
        // Should be ordered by SortOrder first, then Label (alphabetically)
        Assert.Equal("Apple", result.Items[0].Label); // SortOrder 1, alphabetically first
        Assert.Equal("Banana", result.Items[1].Label); // SortOrder 1, alphabetically second
        Assert.Equal("Zebra", result.Items[2].Label); // SortOrder 2
    }

    private async Task<(Guid projectId, Guid managedListId)> CreateProjectAndManagedList()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        
        // Create a project with unique name to avoid conflicts
        var uniqueName = $"Test Project {Guid.NewGuid().ToString()[..8]}";
        var createProjectRequest = new CreateProjectRequest(
            uniqueName,
            "Test Project Description",
            null, // ClientId
            null, // CommissioningMarketId
            null, // Methodology
            null, // ProductId
            null, // Owner
            null, // Status
            null); // CostManagementEnabled

        var projectResponse = await _client.PostAsJsonAsync("/api/projects", createProjectRequest, cancellationToken);
        var project = await projectResponse.Content.ReadFromJsonAsync<CreateProjectResponse>(_jsonOptions, cancellationToken);

        // Create a managed list
        var createManagedListRequest = new CreateManagedListRequest(
            project!.Id,
            "Test Managed List",
            "Test Description");

        var managedListResponse = await _client.PostAsJsonAsync("/api/managedlists", createManagedListRequest, cancellationToken);
        var managedList = await managedListResponse.Content.ReadFromJsonAsync<CreateManagedListResponse>(_jsonOptions, cancellationToken);

        return (project.Id, managedList!.Id);
    }
}
