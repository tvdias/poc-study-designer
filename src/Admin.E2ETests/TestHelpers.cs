using Microsoft.Playwright;

namespace Admin.E2ETests;

/// <summary>
/// Helper class for common E2E test operations
/// </summary>
public class TestHelpers
{
    private readonly IPage _page;

    public TestHelpers(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Wait for the page to be loaded (no loading state visible)
    /// </summary>
    public async Task WaitForPageLoadAsync()
    {
        await Assertions.Expect(_page.GetByText("Loading...")).Not.ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    /// <summary>
    /// Click the New button in the command bar
    /// </summary>
    public async Task ClickNewButtonAsync()
    {
        await _page.GetByRole(AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex("new", System.Text.RegularExpressions.RegexOptions.IgnoreCase) }).ClickAsync();
    }

    /// <summary>
    /// Click the Save button in the side panel
    /// </summary>
    public async Task ClickSaveButtonAsync()
    {
        await _page.GetByRole(AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex("^save$", System.Text.RegularExpressions.RegexOptions.IgnoreCase) }).ClickAsync();
    }

    /// <summary>
    /// Click the Edit button in the side panel
    /// </summary>
    public async Task ClickEditButtonAsync()
    {
        await _page.GetByRole(AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex("^edit$", System.Text.RegularExpressions.RegexOptions.IgnoreCase) }).ClickAsync();
    }

    /// <summary>
    /// Click the Delete button in the side panel
    /// </summary>
    public async Task ClickDeleteButtonAsync()
    {
        await _page.GetByRole(AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex("^delete$", System.Text.RegularExpressions.RegexOptions.IgnoreCase) }).ClickAsync();
    }

    /// <summary>
    /// Set up handler to confirm the deletion dialog (call this before clicking delete)
    /// </summary>
    public void ConfirmDelete()
    {
        _page.Dialog += (_, dialog) =>
        {
            dialog.AcceptAsync();
        };
    }

    /// <summary>
    /// Wait for an element to be visible by text
    /// </summary>
    public async Task WaitForTextAsync(string text)
    {
        await Assertions.Expect(_page.GetByText(text)).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    /// <summary>
    /// Check if text exists on the page
    /// </summary>
    public async Task ExpectTextToBeVisibleAsync(string text)
    {
        await Assertions.Expect(_page.GetByText(text)).ToBeVisibleAsync();
    }

    /// <summary>
    /// Check if text does not exist on the page
    /// </summary>
    public async Task ExpectTextNotToBeVisibleAsync(string text)
    {
        await Assertions.Expect(_page.GetByText(text)).Not.ToBeVisibleAsync();
    }

    /// <summary>
    /// Fill a form field by its label
    /// </summary>
    public async Task FillFieldAsync(string label, string value)
    {
        await _page.GetByLabel(label).FillAsync(value);
    }

    /// <summary>
    /// Click a table row containing specific text
    /// </summary>
    public async Task ClickTableRowAsync(string text)
    {
        await _page.GetByRole(AriaRole.Row).Filter(new() { HasText = text }).ClickAsync();
    }

    /// <summary>
    /// Wait for side panel to open with specific title
    /// </summary>
    public async Task WaitForSidePanelTitleAsync(string title)
    {
        await Assertions.Expect(_page.GetByRole(AriaRole.Heading, new() { Name = title })).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    /// <summary>
    /// Generate a unique name for testing
    /// </summary>
    public string GenerateUniqueName(string prefix)
    {
        return $"{prefix}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
    }
}
