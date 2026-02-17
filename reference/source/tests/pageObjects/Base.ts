import { Page } from "@playwright/test";

export const question_bank_Menu_Button = (page: Page) => page.getByText('Question Bank');