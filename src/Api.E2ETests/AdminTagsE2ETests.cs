using Api.Features.Tags;
using System.Net.Http.Json;

namespace Api.E2ETests;

/// <summary>
/// E2E tests for Tags management in the Admin app.
/// These tests cover the full stack: Frontend UI -> API -> Database
/// </summary>
public class AdminTagsE2ETests(E2ETestFixture fixture)
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
            // Navigate directly to the Tags page
            await page.GotoAsync($"{adminUrl}/tags", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Wait for the table to render (proves API call completed)
            await page.Locator("table.details-list").WaitForAsync(new() { Timeout = 15000 });

            // Click "New" button in the command bar
            await page.Locator("button.cmd-btn.primary:has-text('New')").ClickAsync();

            // Wait for the side panel to open and fill in the tag name
            var nameInput = page.Locator("#tagName");
            await nameInput.WaitForAsync(new() { Timeout = 5000 });
            await nameInput.FillAsync(tagName);

            // Click Save button
            await page.Locator("button:has-text('Save')").ClickAsync();

            // Wait for the save to complete and panel to close
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify the tag appears in the list
            var tagCell = page.Locator($"td:has-text('{tagName}')");
            await tagCell.WaitForAsync(new() { Timeout = 10000 });
            Assert.True(await tagCell.IsVisibleAsync(), $"Expected tag '{tagName}' to be visible in the list");

            // Verify in database via API
            using var apiClient = fixture.CreateApiClient();
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
        using var apiClient = fixture.CreateApiClient();
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
            // Navigate directly to the Tags page
            await page.GotoAsync($"{adminUrl}/tags", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Wait for the table to render
            await page.Locator("table.details-list").WaitForAsync(new() { Timeout = 15000 });

            // Find the tag row and click Edit button (icon button with title="Edit")
            var tagRow = page.Locator($"tr:has-text('{originalName}')");
            await tagRow.Locator("button[title='Edit']").ClickAsync();

            // Wait for the side panel form and update the tag name
            var nameInput = page.Locator("#tagName");
            await nameInput.WaitForAsync(new() { Timeout = 5000 });
            await nameInput.ClearAsync();
            await nameInput.FillAsync(updatedName);

            // Click Save button
            await page.Locator("button:has-text('Save')").ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify the updated tag appears in the list
            var updatedTagCell = page.Locator($"td:has-text('{updatedName}')");
            await updatedTagCell.WaitForAsync(new() { Timeout = 10000 });
            Assert.True(await updatedTagCell.IsVisibleAsync(), $"Expected updated tag '{updatedName}' to be visible in the list");

            // Verify in database via API
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
        using var apiClient = fixture.CreateApiClient();
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
            // Navigate directly to the Tags page
            await page.GotoAsync($"{adminUrl}/tags", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Wait for the table to render
            await page.Locator("table.details-list").WaitForAsync(new() { Timeout = 15000 });

            // The delete handler uses browser confirm() dialog — accept it automatically
            page.Dialog += async (_, dialog) => await dialog.AcceptAsync();

            // Find the tag row and click Delete button (icon button with title="Delete")
            var tagRow = page.Locator($"tr:has-text('{tagName}')");
            await tagRow.Locator("button[title='Delete']").ClickAsync();

            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify the tag no longer appears in the list
            await Expect(page.Locator($"td:has-text('{tagName}')")).ToHaveCountAsync(0);

            // Verify removed from database via API
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
        using var apiClient = fixture.CreateApiClient();
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
            // Navigate directly to the Tags page
            await page.GotoAsync($"{adminUrl}/tags", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Wait for the table to render
            await page.Locator("table.details-list").WaitForAsync(new() { Timeout = 15000 });

            // Find the tag row and click View button (icon button with title="View")
            var tagRow = page.Locator($"tr:has-text('{tagName}')");
            await tagRow.Locator("button[title='View']").ClickAsync();

            // Wait for the side panel to open (view mode shows data in .view-details divs)
            var sidePanel = page.Locator(".side-panel");
            await sidePanel.WaitForAsync(new() { Timeout = 5000 });

            // Verify the panel title shows the tag name
            var panelTitle = sidePanel.Locator("h3");
            await Expect(panelTitle).ToHaveTextAsync(tagName);

            // Verify the name value is displayed
            var nameValue = sidePanel.Locator(".detail-item .value", new() { HasText = tagName });
            Assert.True(await nameValue.IsVisibleAsync(), "Expected tag name to be visible in the side panel");

            // Verify the ID value is displayed
            var idValue = sidePanel.Locator($".detail-item .value.monospace:has-text('{createdTag.Id}')");
            Assert.True(await idValue.IsVisibleAsync(), "Expected ID to be visible in the side panel");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    private static ILocatorAssertions Expect(ILocator locator)
        => Assertions.Expect(locator);
}
