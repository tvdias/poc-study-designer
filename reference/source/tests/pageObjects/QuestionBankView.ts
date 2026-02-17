import { Page } from "@playwright/test";

export const table_head_button = (page: Page) =>
  page.getByRole("button", { name: "Active Question Bank" });

export const new_Question_Button = (page: Page) =>
  page.getByRole("menuitem", { name: "New", exact: true });

export const edit_Question_Button = (page: Page) =>
  page.getByRole("menuitem", { name: "Edit", exact: true });

export const question_row_Checkbox = (page: Page) =>
  page.getByLabel("Press SPACE to select this").getByText("");

export const delete_question_Button = (page: Page) =>
  page.getByRole("button", { name: "Delete", exact: true });

export const delete_question_dialog_Heading = (page: Page) =>
  page.getByRole("heading", { name: "Confirm Deletion" });

export const delete_question_dialog_delete_Button = (page: Page) =>
  page.getByRole("button", { name: "Delete" });

export const questionVariableName_filter_Button = (page: Page) =>
  page.getByRole("button", { name: "Question Variable Name" });

export const questionVariableName_filter_FilterBy_MenuItem = (page: Page) =>
  page.getByRole("menuitem", { name: "Filter by" });

export const questionVariableName_filter_FilterByValue_Textbox = (page: Page) =>
  page.getByRole("textbox", { name: "Filter by value" });

export const questionVariableName_filter_FilterByValue__Apply_Button = (
  page: Page
) => page.getByRole("button", { name: "Apply" });

export const search_Searchbox = (page: Page) =>
  page.getByRole("searchbox", { name: "Question Bank Filter by" });

export const search_searchbox_Input = (page: Page) =>
  page.getByRole("searchbox", { name: "Apply begins with filter on" });
