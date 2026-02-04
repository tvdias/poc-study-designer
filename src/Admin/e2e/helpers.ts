import { Page, expect } from '@playwright/test';

/**
 * Helper class for common E2E test operations
 */
export class TestHelpers {
  constructor(private page: Page) {}

  /**
   * Wait for the page to be loaded (no loading state visible)
   */
  async waitForPageLoad() {
    await expect(this.page.getByText('Loading...')).not.toBeVisible({ timeout: 10000 });
  }

  /**
   * Click the New button in the command bar
   */
  async clickNewButton() {
    await this.page.getByRole('button', { name: /new/i }).click();
  }

  /**
   * Click the Save button in the side panel
   */
  async clickSaveButton() {
    await this.page.getByRole('button', { name: /^save$/i }).click();
  }

  /**
   * Click the Edit button in the side panel
   */
  async clickEditButton() {
    await this.page.getByRole('button', { name: /^edit$/i }).click();
  }

  /**
   * Click the Delete button in the side panel
   */
  async clickDeleteButton() {
    await this.page.getByRole('button', { name: /^delete$/i }).click();
  }

  /**
   * Confirm the deletion dialog
   */
  async confirmDelete() {
    this.page.on('dialog', dialog => dialog.accept());
  }

  /**
   * Wait for an element to be visible by text
   */
  async waitForText(text: string | RegExp) {
    await expect(this.page.getByText(text)).toBeVisible({ timeout: 5000 });
  }

  /**
   * Check if text exists on the page
   */
  async expectTextToBeVisible(text: string | RegExp) {
    await expect(this.page.getByText(text)).toBeVisible();
  }

  /**
   * Check if text does not exist on the page
   */
  async expectTextNotToBeVisible(text: string | RegExp) {
    await expect(this.page.getByText(text)).not.toBeVisible();
  }

  /**
   * Fill a form field by its label
   */
  async fillField(label: string, value: string) {
    await this.page.getByLabel(label).fill(value);
  }

  /**
   * Click a table row containing specific text
   */
  async clickTableRow(text: string) {
    await this.page.getByRole('row').filter({ hasText: text }).click();
  }

  /**
   * Click an action button in a table row
   */
  async clickRowAction(rowText: string, action: 'View' | 'Edit' | 'Delete') {
    const row = this.page.getByRole('row').filter({ hasText: rowText });
    await row.getByTitle(action).click();
  }

  /**
   * Wait for side panel to open with specific title
   */
  async waitForSidePanelTitle(title: string | RegExp) {
    await expect(this.page.getByRole('heading', { name: title })).toBeVisible({ timeout: 5000 });
  }

  /**
   * Generate a unique name for testing
   */
  generateUniqueName(prefix: string): string {
    return `${prefix}_${Date.now()}`;
  }
}
