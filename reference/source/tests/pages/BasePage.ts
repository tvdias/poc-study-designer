import { Locator, Page } from "@playwright/test";
import { question_bank_Menu_Button } from "../pageObjects/Base";

export class BasePage {
  protected page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async open(url: string): Promise<void> {
    await this.page.goto(url);
  }

  async fillTextBox(locator: string, text: string): Promise<void> {
    await this.page.fill(locator, text);
  }

  async waitForTextToBePresent(locator: string): Promise<void> {
    await this.page.waitForSelector(locator);
  }

  async clickAsync(locator: Locator): Promise<void> {
    await locator.waitFor({ state: "visible" });
    await locator.click();
  }

  async GoToQuestionBank(): Promise<void> {
    let menu = question_bank_Menu_Button;
    await menu(this.page).waitFor({ state: "visible" });
    await menu(this.page).click();
  }
}
