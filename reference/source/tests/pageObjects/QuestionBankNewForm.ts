import { Page } from "@playwright/test";

export const save_close_Question_Button = (page: Page) =>
  page.getByRole("menuitem", { name: "Save & Close" });

export const questionName_Textbox = (page: Page) =>
  page.getByRole("textbox", { name: "Question Variable Name" });

export const questionType_Combobox = (page: Page) =>
  page.getByRole("combobox", { name: "Question Type" });

export const questionText_Textbox = (page: Page) =>
  page.getByRole("textbox", {
    name: "Rich Text Editor Control kt_questionbank kt_defaultquestiontext",
  });

export const questionTitle_Textbox = (page: Page) =>
  page.getByRole("textbox", { name: "Question Title" });

export const questionScriptorNotes_Textbox = (page: Page) =>
  page.getByRole("textbox", {
    name: "Rich Text Editor Control kt_questionbank kt_scriptornotes",
  });

export const questionRationale_Textbox = (page: Page) =>
  page.getByRole("textbox", {
    name: "Question Rationale",
  });

export const questionStandardOrCustom_Combobox = (page: Page) =>
  page.getByRole("combobox", { name: "Standard or Custom" });

export const questionSingleOrMulticode_Combobox = (page: Page) =>
  page.getByRole("combobox", { name: "Single or Multicode" });

export const questionList_Textbox = (page: Page) =>
  page.getByRole("textbox", {
    name: "Rich Text Editor Control kt_questionbank ktr_answerlist",
  });
