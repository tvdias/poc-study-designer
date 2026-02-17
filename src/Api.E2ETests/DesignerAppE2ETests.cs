namespace Api.E2ETests;

/// <summary>
/// E2E tests for the Designer app basic functionality.
/// These tests verify the Designer frontend loads and renders correctly.
/// </summary>
public class DesignerAppE2ETests(E2ETestFixture fixture)
{
    [Fact]
    public async Task DesignerApp_ShouldLoadSuccessfully()
    {
        // Arrange
        var page = await fixture.CreatePageAsync();
        var designerUrl = fixture.GetDesignerAppUrl();

        try
        {
            // Act
            await page.GotoAsync(designerUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Assert - verify the React app rendered with expected content
            // Check for "Study Designer" text in the header (not necessarily a heading)
            var serviceNameElement = page.Locator("text=Study Designer").First;
            await serviceNameElement.WaitForAsync(new() { Timeout = 10000 });
            Assert.True(await serviceNameElement.IsVisibleAsync(), "Expected 'Study Designer' text to be visible");

            // Verify the projects list page loads (search input should be present)
            var searchInput = page.GetByPlaceholder("Search projects...");
            Assert.True(await searchInput.IsVisibleAsync(), "Expected search input to be visible");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task DesignerApp_ShouldHaveCreateProjectButton()
    {
        // Arrange
        var page = await fixture.CreatePageAsync();
        var designerUrl = fixture.GetDesignerAppUrl();

        try
        {
            // Act
            await page.GotoAsync(designerUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Assert - verify the Create Project button is visible
            var createButton = page.Locator("text=Create Project").First;
            await createButton.WaitForAsync(new() { Timeout = 10000 });
            Assert.True(await createButton.IsVisibleAsync(), "Expected 'Create Project' button to be visible");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task DesignerApp_ShouldRenderWithoutErrors()
    {
        // Arrange
        var page = await fixture.CreatePageAsync();
        var designerUrl = fixture.GetDesignerAppUrl();
        var pageErrors = new List<string>();

        // Listen for page errors (uncaught exceptions)
        page.PageError += (_, error) =>
        {
            pageErrors.Add(error);
        };

        try
        {
            // Act
            await page.GotoAsync(designerUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Wait for the app to fully render by checking for the search input
            var searchInput = page.GetByPlaceholder("Search projects...");
            await searchInput.WaitForAsync(new() { Timeout = 10000 });

            // Assert - no uncaught page errors
            Assert.Empty(pageErrors);
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
