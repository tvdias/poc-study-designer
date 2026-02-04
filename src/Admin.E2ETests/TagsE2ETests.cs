using Microsoft.Playwright;

namespace Admin.E2ETests;

/// <summary>
/// E2E tests for Tags page functionality.
/// Tests the complete flow from UI to API with real database operations.
/// </summary>
[Collection("AdminE2E")]
public class TagsE2ETests : PlaywrightTestBase
{
    public TagsE2ETests(AspireAppHostFixture aspireFixture) : base(aspireFixture)
    {
    }

    [Fact]
    public async Task ShouldCompleteFullCrudFlowForTag()
    {
        Assert.NotNull(Page);
        var helpers = new TestHelpers(Page);
        var tagName = helpers.GenerateUniqueName("E2E_Tag");

        // Get the Admin app URL from Aspire
        var baseUrl = GetAdminAppUrl();

        // Step 1: Navigate to Tags page
        await Page.GotoAsync($"{baseUrl}/tags");
        await helpers.WaitForPageLoadAsync();

        // Step 2: Create a new tag
        await helpers.ClickNewButtonAsync();
        await helpers.WaitForSidePanelTitleAsync("New Tag");

        await helpers.FillFieldAsync("Name", tagName);
        await helpers.ClickSaveButtonAsync();

        // Wait for view mode (title changes to the tag name)
        await helpers.WaitForSidePanelTitleAsync(tagName);
        await Assertions.Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Edit" })).ToBeVisibleAsync();

        // Step 3: Verify the tag appears in the list
        await Page.Keyboard.PressAsync("Escape");
        await helpers.WaitForTextAsync(tagName);

        var row = Page.GetByRole(AriaRole.Row).Filter(new() { HasText = tagName });
        await Assertions.Expect(row).ToBeVisibleAsync();
        await Assertions.Expect(row.GetByText("Active")).ToBeVisibleAsync();

        // Step 4: Edit the tag
        await helpers.ClickTableRowAsync(tagName);
        await helpers.WaitForSidePanelTitleAsync(tagName);

        await helpers.ClickEditButtonAsync();
        await helpers.WaitForSidePanelTitleAsync("Edit Tag");

        var updatedTagName = $"{tagName}_Updated";
        await helpers.FillFieldAsync("Name", updatedTagName);

        // Toggle the active status
        await Page.GetByLabel("Is Active").UncheckAsync();

        await helpers.ClickSaveButtonAsync();
        await helpers.WaitForSidePanelTitleAsync(updatedTagName);

        // Step 5: Verify the tag was updated
        await Page.Keyboard.PressAsync("Escape");
        await helpers.WaitForTextAsync(updatedTagName);

        var updatedRow = Page.GetByRole(AriaRole.Row).Filter(new() { HasText = updatedTagName });
        await Assertions.Expect(updatedRow).ToBeVisibleAsync();
        await Assertions.Expect(updatedRow.GetByText("Inactive")).ToBeVisibleAsync();

        // Step 6: Delete the tag
        await helpers.ClickTableRowAsync(updatedTagName);
        await helpers.WaitForSidePanelTitleAsync(updatedTagName);

        helpers.ConfirmDelete();
        await helpers.ClickDeleteButtonAsync();

        // Step 7: Verify the tag is deleted
        await Assertions.Expect(Page.GetByRole(AriaRole.Heading, new() { Name = updatedTagName }))
            .Not.ToBeVisibleAsync(new() { Timeout = 5000 });
        await Assertions.Expect(Page.GetByRole(AriaRole.Row).Filter(new() { HasText = updatedTagName }))
            .Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task ShouldHandleValidationErrorsWhenCreatingTagWithoutName()
    {
        Assert.NotNull(Page);
        var helpers = new TestHelpers(Page);
        var baseUrl = GetAdminAppUrl();

        await Page.GotoAsync($"{baseUrl}/tags");
        await helpers.WaitForPageLoadAsync();

        await helpers.ClickNewButtonAsync();
        await helpers.WaitForSidePanelTitleAsync("New Tag");

        // Try to save without entering a name
        await helpers.ClickSaveButtonAsync();

        // Should show validation error
        await helpers.WaitForTextAsync("Name is required");
    }
}
