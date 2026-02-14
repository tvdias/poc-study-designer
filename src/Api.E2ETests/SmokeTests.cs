namespace Api.E2ETests;

public class SmokeTests(E2ETestFixture fixture)
{
    [Fact]
    public async Task Api_HealthCheck_ReturnsOk()
    {
        // Arrange
        using var client = fixture.CreateApiClient();

        // Act
        var response = await client.GetAsync("health", TestContext.Current.CancellationToken);

        // Assert
        Assert.True(response.IsSuccessStatusCode, $"API health check failed with status code: {response.StatusCode}");
    }

    [Fact]
    public async Task AdminApp_LoadsHomePage()
    {
        // Arrange
        var page = await fixture.CreatePageAsync();
        var adminUrl = fixture.GetAdminAppUrl();

        try
        {
            // Act
            await page.GotoAsync(adminUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Assert - verify the sidebar navigation rendered (proves React app hydrated)
            var sidebar = page.GetByRole(AriaRole.Heading, new() { Name = "Admin Center" });
            await sidebar.WaitForAsync(new() { Timeout = 10000 });
            Assert.True(await sidebar.IsVisibleAsync(), "Admin Center sidebar heading was not visible.");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task AdminApp_CanLoadTagsPage()
    {
        // Arrange
        var page = await fixture.CreatePageAsync();
        var adminUrl = fixture.GetAdminAppUrl();

        try
        {
            // Act - navigate directly to the tags page
            await page.GotoAsync($"{adminUrl}/tags", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Assert - the tags table is rendered after the API call to /api/tags completes.
            // This proves: React routing works, the Vite proxy forwards /api requests to the API,
            // and the API responds successfully.
            var table = page.Locator("table.details-list");
            await table.WaitForAsync(new() { Timeout = 15000 });
            Assert.True(await table.IsVisibleAsync(), "Tags table was not visible on the Tags page.");

            // Also verify the search input rendered (part of the command bar)
            var searchInput = page.GetByPlaceholder("Search tags...");
            Assert.True(await searchInput.IsVisibleAsync(), "Search input was not visible on the Tags page.");
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
