import { test, expect } from '@playwright/test';
import { TestHelpers } from './helpers';

test.describe('Fieldwork Markets Page E2E Tests', () => {
  let helpers: TestHelpers;
  let marketName: string;
  let marketIsoCode: string;

  test.beforeEach(async ({ page }) => {
    helpers = new TestHelpers(page);
    const uniqueId = helpers.generateUniqueName('E2E');
    marketName = `${uniqueId}_Market`;
    marketIsoCode = `E2E${Date.now().toString().slice(-6)}`;
    
    // Navigate to the Fieldwork Markets page
    await page.goto('/fieldwork-markets');
    await helpers.waitForPageLoad();
  });

  test('should complete full CRUD flow for a fieldwork market', async ({ page }) => {
    // Step 1: Create a new fieldwork market
    await helpers.clickNewButton();
    await helpers.waitForSidePanelTitle(/New Fieldwork Market/i);

    // Fill in the market details
    await helpers.fillField('ISO Code', marketIsoCode);
    await helpers.fillField('Name', marketName);
    
    // Save the market
    await helpers.clickSaveButton();

    // Wait for the view mode
    await helpers.waitForSidePanelTitle(marketName);
    await expect(page.getByRole('button', { name: /^edit$/i })).toBeVisible();

    // Step 2: Verify the market appears in the list
    await page.keyboard.press('Escape');
    await helpers.waitForText(marketName);
    
    // Verify the market is in the list with Active status
    const row = page.getByRole('row').filter({ hasText: marketName });
    await expect(row).toBeVisible();
    await expect(row.getByText(marketIsoCode)).toBeVisible();
    await expect(row.getByText('Active')).toBeVisible();

    // Step 3: Edit the market
    await helpers.clickTableRow(marketName);
    await helpers.waitForSidePanelTitle(marketName);
    
    // Click Edit button
    await helpers.clickEditButton();
    await helpers.waitForSidePanelTitle(/Edit Fieldwork Market/i);

    // Update the details
    const updatedMarketName = `${marketName}_Updated`;
    const updatedIsoCode = `${marketIsoCode}U`;
    await helpers.fillField('ISO Code', updatedIsoCode);
    await helpers.fillField('Name', updatedMarketName);
    
    // Toggle the active status
    await page.getByLabel(/Is Active/i).uncheck();

    // Save the changes
    await helpers.clickSaveButton();

    // Wait for view mode
    await helpers.waitForSidePanelTitle(updatedMarketName);

    // Step 4: Verify the market was updated
    await page.keyboard.press('Escape');
    await helpers.waitForText(updatedMarketName);
    
    const updatedRow = page.getByRole('row').filter({ hasText: updatedMarketName });
    await expect(updatedRow).toBeVisible();
    await expect(updatedRow.getByText(updatedIsoCode)).toBeVisible();
    await expect(updatedRow.getByText('Inactive')).toBeVisible();
    
    // Verify old name is not in the list
    await expect(page.getByRole('row').filter({ hasText: new RegExp(`^${marketName}$`) })).not.toBeVisible();

    // Step 5: Delete the market
    await helpers.clickTableRow(updatedMarketName);
    await helpers.waitForSidePanelTitle(updatedMarketName);
    
    // Set up dialog handler to confirm deletion
    helpers.confirmDelete();
    
    // Click Delete button
    await helpers.clickDeleteButton();

    // Step 6: Verify the market is deleted
    await expect(page.getByRole('heading', { name: updatedMarketName })).not.toBeVisible({ timeout: 5000 });
    await expect(page.getByRole('row').filter({ hasText: updatedMarketName })).not.toBeVisible();
  });

  test('should handle validation errors when creating a market without required fields', async ({ page }) => {
    // Open create form
    await helpers.clickNewButton();
    await helpers.waitForSidePanelTitle(/New Fieldwork Market/i);

    // Try to save without entering required fields
    await helpers.clickSaveButton();

    // Should show validation errors (at least one error should be visible)
    const errorText = page.locator('.field-error, .server-error').first();
    await expect(errorText).toBeVisible({ timeout: 5000 });
  });

  test('should handle duplicate market creation', async ({ page }) => {
    // Create first market
    await helpers.clickNewButton();
    await helpers.waitForSidePanelTitle(/New Fieldwork Market/i);
    await helpers.fillField('ISO Code', marketIsoCode);
    await helpers.fillField('Name', marketName);
    await helpers.clickSaveButton();
    await helpers.waitForSidePanelTitle(marketName);
    
    // Close the panel
    await page.keyboard.press('Escape');

    // Try to create another market with the same details
    await helpers.clickNewButton();
    await helpers.waitForSidePanelTitle(/New Fieldwork Market/i);
    await helpers.fillField('ISO Code', marketIsoCode);
    await helpers.fillField('Name', marketName);
    await helpers.clickSaveButton();

    // Should show conflict error
    await helpers.waitForText(/already exists/i);

    // Cleanup: Close the create panel and delete the market
    await page.keyboard.press('Escape');
    await helpers.clickTableRow(marketName);
    helpers.confirmDelete();
    await helpers.clickDeleteButton();
  });
});
