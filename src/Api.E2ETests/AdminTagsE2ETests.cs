using Api.Features.Tags;
using Microsoft.Playwright;
using System.Net.Http.Json;

namespace Api.E2ETests;

/// <summary>
/// E2E tests for Tags management in the Admin app.
/// These tests cover the full stack: Frontend UI -> API -> Database
/// </summary>
public class AdminTagsE2ETests(E2ETestFixture fixture) : IClassFixture<E2ETestFixture>
{
    [Fact]
    public async Task CreateTag_ThroughUI_ShouldPersistInDatabase()
    {
        // Arrange
        var page = await fixture.CreatePageAsync();
        var adminUrl = fixture.GetAdminAppUrl();
        var tagName = $"E2E Test Tag {Guid.NewGuid()}";

        try
        {
            // Act - Navigate to Admin app
            await page.GotoAsync(adminUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Wait for the app to load
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Navigate to Tags page
            await page.ClickAsync("text=Tags", new PageClickOptions { Timeout = 10000 });
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Click "New Tag" button
            await page.ClickAsync("button:has-text('New Tag')");

            // Fill in the tag name
            await page.FillAsync("input[placeholder*='Tag']", tagName);

            // Click Save button
            await page.ClickAsync("button:has-text('Save')");

            // Wait for the save to complete
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify the tag appears in the list
            var tagCell = page.Locator($"td:has-text('{tagName}')");
            Assert.True(await tagCell.IsVisibleAsync(), $"Expected tag '{tagName}' to be visible in the list");

            // Assert - Verify in database via API
            var apiClient = fixture.CreateApiClient();
            var response = await apiClient.GetAsync("/api/tags", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();

            var tags = await response.Content.ReadFromJsonAsync<List<GetTagsResponse>>(cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(tags);
            Assert.Contains(tags, t => t.Name == tagName);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task EditTag_ThroughUI_ShouldUpdateInDatabase()
    {
        // Arrange - Create a tag via API first
        var apiClient = fixture.CreateApiClient();
        var originalName = $"E2E Original Tag {Guid.NewGuid()}";
        var updatedName = $"E2E Updated Tag {Guid.NewGuid()}";

        var createResponse = await apiClient.PostAsJsonAsync("/api/tags",
            new CreateTagRequest(originalName),
            TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();
        var createdTag = await createResponse.Content.ReadFromJsonAsync<CreateTagResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(createdTag);

        var page = await fixture.CreatePageAsync();
        var adminUrl = fixture.GetAdminAppUrl();

        try
        {
            // Act - Navigate to Admin app
            await page.GotoAsync(adminUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Navigate to Tags page
            await page.ClickAsync("text=Tags", new PageClickOptions { Timeout = 10000 });
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Find the tag row and click Edit button
            var tagRow = page.Locator($"tr:has-text('{originalName}')");
            await tagRow.Locator("button:has-text('Edit')").ClickAsync();

            // Update the tag name
            await page.FillAsync("input[placeholder*='Tag']", updatedName);

            // Click Save button
            await page.ClickAsync("button:has-text('Save')");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify the updated tag appears in the list
            var updatedTagCell = page.Locator($"td:has-text('{updatedName}')");
            Assert.True(await updatedTagCell.IsVisibleAsync(), $"Expected updated tag '{updatedName}' to be visible in the list");

            // Assert - Verify in database via API
            var getResponse = await apiClient.GetAsync($"/api/tags/{createdTag.Id}", TestContext.Current.CancellationToken);
            getResponse.EnsureSuccessStatusCode();
            var fetchedTag = await getResponse.Content.ReadFromJsonAsync<GetTagByIdResponse>(cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(fetchedTag);
            Assert.Equal(updatedName, fetchedTag.Name);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task DeleteTag_ThroughUI_ShouldRemoveFromDatabase()
    {
        // Arrange - Create a tag via API first
        var apiClient = fixture.CreateApiClient();
        var tagName = $"E2E Delete Tag {Guid.NewGuid()}";

        var createResponse = await apiClient.PostAsJsonAsync("/api/tags",
            new CreateTagRequest(tagName),
            TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();
        var createdTag = await createResponse.Content.ReadFromJsonAsync<CreateTagResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(createdTag);

        var page = await fixture.CreatePageAsync();
        var adminUrl = fixture.GetAdminAppUrl();

        try
        {
            // Act - Navigate to Admin app
            await page.GotoAsync(adminUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Navigate to Tags page
            await page.ClickAsync("text=Tags", new PageClickOptions { Timeout = 10000 });
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Find the tag row and click Delete button
            var tagRow = page.Locator($"tr:has-text('{tagName}')");
            await tagRow.Locator("button:has-text('Delete')").ClickAsync();

            // Confirm deletion (if there's a confirmation dialog)
            var confirmButton = page.Locator("button:has-text('Confirm')");
            if (await confirmButton.IsVisibleAsync())
            {
                await confirmButton.ClickAsync();
            }

            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify the tag no longer appears in the list
            var deletedTagCell = page.Locator($"td:has-text('{tagName}')");
            Assert.False(await deletedTagCell.IsVisibleAsync(), $"Expected tag '{tagName}' to not be visible in the list");

            // Assert - Verify removed from database via API
            var getResponse = await apiClient.GetAsync($"/api/tags/{createdTag.Id}", TestContext.Current.CancellationToken);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task ViewTag_ThroughUI_ShouldDisplayCorrectData()
    {
        // Arrange - Create a tag via API first
        var apiClient = fixture.CreateApiClient();
        var tagName = $"E2E View Tag {Guid.NewGuid()}";

        var createResponse = await apiClient.PostAsJsonAsync("/api/tags",
            new CreateTagRequest(tagName),
            TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();
        var createdTag = await createResponse.Content.ReadFromJsonAsync<CreateTagResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(createdTag);

        var page = await fixture.CreatePageAsync();
        var adminUrl = fixture.GetAdminAppUrl();

        try
        {
            // Act - Navigate to Admin app
            await page.GotoAsync(adminUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Navigate to Tags page
            await page.ClickAsync("text=Tags", new PageClickOptions { Timeout = 10000 });
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Find the tag row and click View button
            var tagRow = page.Locator($"tr:has-text('{tagName}')");
            await tagRow.Locator("button:has-text('View')").ClickAsync();

            // Assert - Verify the side panel shows correct data
            var nameField = page.Locator("input[value*='" + tagName.Substring(0, 20) + "']");
            Assert.True(await nameField.IsVisibleAsync(), "Expected name field to be visible in the side panel");

            var idField = page.Locator($"text={createdTag.Id}");
            Assert.True(await idField.IsVisibleAsync(), "Expected ID field to be visible in the side panel");
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
