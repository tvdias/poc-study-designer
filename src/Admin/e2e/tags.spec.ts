import { test, expect } from '@playwright/test';
import { TestHelpers } from './helpers';

test.describe('Tags Page E2E Tests', () => {
  let helpers: TestHelpers;
  let tagName: string;

  test.beforeEach(async ({ page }) => {
    helpers = new TestHelpers(page);
    tagName = helpers.generateUniqueName('E2E_Tag');
    
    // Navigate to the Tags page
    await page.goto('/tags');
    await helpers.waitForPageLoad();
  });

  test('should complete full CRUD flow for a tag', async ({ page }) => {
    // Step 1: Create a new tag
    await helpers.clickNewButton();
    await helpers.waitForSidePanelTitle(/New Tag/i);

    // Fill in the tag name
    await helpers.fillField('Name', tagName);
    
    // Save the tag
    await helpers.clickSaveButton();

    // Wait for the view mode (title changes to the tag name)
    await helpers.waitForSidePanelTitle(tagName);
    await expect(page.getByRole('button', { name: /^edit$/i })).toBeVisible();

    // Step 2: Verify the tag appears in the list
    // Close the side panel by clicking outside or using ESC
    await page.keyboard.press('Escape');
    await helpers.waitForText(tagName);
    
    // Verify the tag is in the list with Active status
    const row = page.getByRole('row').filter({ hasText: tagName });
    await expect(row).toBeVisible();
    await expect(row.getByText('Active')).toBeVisible();

    // Step 3: Edit the tag
    await helpers.clickTableRow(tagName);
    await helpers.waitForSidePanelTitle(tagName);
    
    // Click Edit button
    await helpers.clickEditButton();
    await helpers.waitForSidePanelTitle(/Edit Tag/i);

    // Update the name
    const updatedTagName = `${tagName}_Updated`;
    await helpers.fillField('Name', updatedTagName);
    
    // Toggle the active status
    await page.getByLabel(/Is Active/i).uncheck();

    // Save the changes
    await helpers.clickSaveButton();

    // Wait for view mode
    await helpers.waitForSidePanelTitle(updatedTagName);

    // Step 4: Verify the tag was updated
    await page.keyboard.press('Escape');
    await helpers.waitForText(updatedTagName);
    
    const updatedRow = page.getByRole('row').filter({ hasText: updatedTagName });
    await expect(updatedRow).toBeVisible();
    await expect(updatedRow.getByText('Inactive')).toBeVisible();
    
    // Verify old name is not in the list
    await expect(page.getByRole('row').filter({ hasText: new RegExp(`^${tagName}$`) })).not.toBeVisible();

    // Step 5: Delete the tag
    await helpers.clickTableRow(updatedTagName);
    await helpers.waitForSidePanelTitle(updatedTagName);
    
    // Set up dialog handler to confirm deletion
    helpers.confirmDelete();
    
    // Click Delete button
    await helpers.clickDeleteButton();

    // Step 6: Verify the tag is deleted
    // Wait for the panel to close and the tag to disappear from the list
    await expect(page.getByRole('heading', { name: updatedTagName })).not.toBeVisible({ timeout: 5000 });
    await expect(page.getByRole('row').filter({ hasText: updatedTagName })).not.toBeVisible();
  });

  test('should handle validation errors when creating a tag without a name', async ({ page }) => {
    // Open create form
    await helpers.clickNewButton();
    await helpers.waitForSidePanelTitle(/New Tag/i);

    // Try to save without entering a name
    await helpers.clickSaveButton();

    // Should show validation error
    await helpers.waitForText(/Name is required|cannot be empty/i);
  });

  test('should handle duplicate tag creation', async ({ page }) => {
    // Create first tag
    await helpers.clickNewButton();
    await helpers.waitForSidePanelTitle(/New Tag/i);
    await helpers.fillField('Name', tagName);
    await helpers.clickSaveButton();
    await helpers.waitForSidePanelTitle(tagName);
    
    // Close the panel
    await page.keyboard.press('Escape');

    // Try to create another tag with the same name
    await helpers.clickNewButton();
    await helpers.waitForSidePanelTitle(/New Tag/i);
    await helpers.fillField('Name', tagName);
    await helpers.clickSaveButton();

    // Should show conflict error
    await helpers.waitForText(/already exists/i);

    // Cleanup: Close the create panel and delete the tag
    await page.keyboard.press('Escape');
    await helpers.clickTableRow(tagName);
    helpers.confirmDelete();
    await helpers.clickDeleteButton();
  });
});
