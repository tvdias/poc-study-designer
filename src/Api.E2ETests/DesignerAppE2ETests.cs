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
            var heading = page.GetByRole(AriaRole.Heading, new() { Name = "Study Designer" });
            await heading.WaitForAsync(new() { Timeout = 10000 });
            Assert.True(await heading.IsVisibleAsync(), "Expected 'Study Designer' heading to be visible");

            var welcomeMessage = page.Locator("text=Welcome to the PoC Study Designer application.");
            Assert.True(await welcomeMessage.IsVisibleAsync(), "Expected welcome message to be visible");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task DesignerApp_ShouldHaveWorkingNavigation()
    {
        // Arrange
        var page = await fixture.CreatePageAsync();
        var designerUrl = fixture.GetDesignerAppUrl();

        try
        {
            // Act
            await page.GotoAsync(designerUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Assert - verify footer navigation elements exist
            var footerNav = page.Locator("footer nav");
            await footerNav.WaitForAsync(new() { Timeout = 10000 });
            Assert.True(await footerNav.IsVisibleAsync(), "Expected footer navigation to be visible");

            // Verify the Aspire link is present
            var aspireLink = footerNav.GetByRole(AriaRole.Link, new() { Name = "Learn more about Aspire" });
            Assert.True(await aspireLink.IsVisibleAsync(), "Expected 'Learn more about Aspire' link to be visible");
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

            // Wait for the app to fully render
            var heading = page.GetByRole(AriaRole.Heading, new() { Name = "Study Designer" });
            await heading.WaitForAsync(new() { Timeout = 10000 });

            // Assert - no uncaught page errors
            Assert.Empty(pageErrors);
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
