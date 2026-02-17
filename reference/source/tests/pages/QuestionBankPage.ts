import { expect, Locator, Page } from "@playwright/test";
import { BasePage } from "./BasePage";
import {
  delete_question_Button,
  delete_question_dialog_delete_Button,
  delete_question_dialog_Heading,
  edit_Question_Button,
  new_Question_Button,
  question_row_Checkbox,
  questionVariableName_filter_Button,
  questionVariableName_filter_FilterBy_MenuItem,
  questionVariableName_filter_FilterByValue__Apply_Button,
  questionVariableName_filter_FilterByValue_Textbox,
  search_Searchbox,
  search_searchbox_Input,
} from "../pageObjects/QuestionBankView";
import {
  questionList_Textbox,
  questionName_Textbox,
  questionRationale_Textbox,
  questionScriptorNotes_Textbox,
  questionSingleOrMulticode_Combobox,
  questionStandardOrCustom_Combobox,
  questionText_Textbox,
  questionTitle_Textbox,
  questionType_Combobox,
  save_close_Question_Button,
} from "../pageObjects/QuestionBankNewForm";
import { QuestionCreationForm } from "../models/questionCreationForm";

export class QuestionBankPage extends BasePage {
  readonly page: Page;

  constructor(page: Page) {
    super(page);
    this.page = page;
  }

  async clickNewButton() {
    const adminButton = new_Question_Button(this.page);
    await adminButton.waitFor({ state: "visible" });
    await adminButton.click();
  }
  async clickEditButton() {
    const adminButton = edit_Question_Button(this.page);
    await adminButton.waitFor({ state: "visible" });
    await adminButton.click();
  }

  async clicksaveCloseButton() {
    const adminButton = save_close_Question_Button(this.page);
    await adminButton.waitFor({ state: "visible" });
    await adminButton.click();
  }

  async fillQuestionName(text: string) {
    await questionName_Textbox(this.page).fill(text);
  }

  async fillQuestionText(text: string) {
    await questionText_Textbox(this.page).fill(text);
  }

  async fillQuestionList(text: string) {
    await questionList_Textbox(this.page).fill(text);
  }

  async fillQuestionRationale(text: string) {
    await questionRationale_Textbox(this.page).fill(text);
  }

  async ChooseTypeCombobox(text: string) {
    await questionType_Combobox(this.page).click();
    await this.page.getByRole("option", { name: text }).click();
  }

  async ChooseStandardOrCustomCombobox(text: string) {
    await questionStandardOrCustom_Combobox(this.page).click();
    await this.page.getByRole("option", { name: text }).click();
  }

  async fillQuestionTitle(text: string) {
    await questionTitle_Textbox(this.page).fill(text);
  }

  async fillScriptorNotes(text: string) {
    await questionScriptorNotes_Textbox(this.page).fill(text);
  }

  async ChooseSingleOrMulticodeCombobox(text: string) {
    await questionSingleOrMulticode_Combobox(this.page).click();
    await this.page.getByRole("option", { name: text, exact: true }).click();
  }

  async FilterByQuestionNameRowAsync(questionName: string) {
    await super.clickAsync(questionVariableName_filter_Button(this.page));
    await super.clickAsync(questionVariableName_filter_Button(this.page));
    await super.clickAsync(
      questionVariableName_filter_FilterBy_MenuItem(this.page)
    );
    await questionVariableName_filter_FilterByValue_Textbox(this.page).fill(
      questionName
    );
    await questionVariableName_filter_FilterByValue__Apply_Button(
      this.page
    ).click();
  }

  async DeleteFilteredRowAsync() {
    await question_row_Checkbox(this.page).click();
    await delete_question_Button(this.page).click();
    await delete_question_dialog_Heading(this.page).waitFor({
      state: "visible",
    });
    await delete_question_dialog_delete_Button(this.page).click();
  }

  async SearchAsync(searchText: string) {
    await search_Searchbox(this.page).waitFor({ state: "visible" });
    await search_Searchbox(this.page).click();
    await search_searchbox_Input(this.page).fill(searchText);
    await search_searchbox_Input(this.page).press("Enter");
  }

  async GetRowByTextAsync(searchText: string) {
    return this.page.getByText(searchText, { exact: true });
  }

  async StandardOrCustomHasValueAsync(value: string) {
    await questionStandardOrCustom_Combobox(this.page).waitFor({
      state: "visible",
    });

    expect(
      await questionStandardOrCustom_Combobox(this.page).textContent()
    ).toBe(value);
  }

  async IsRequiredAsync(locator: Locator, isRequired: boolean = true) {
    const required = (await locator.getAttribute("required")) !== null;
    const isAriaRequired =
      (await locator.getAttribute("aria-required")) === "true";
    expect(required || isAriaRequired).toBe(isRequired);
  }

  async QuestionVariableNameIsRequiredAsync(isRequired: boolean = true) {
    await this.IsRequiredAsync(questionName_Textbox(this.page), isRequired);
  }

  async QuestionTypeIsRequiredAsync(isRequired: boolean = true) {
    await this.IsRequiredAsync(questionType_Combobox(this.page), isRequired);
  }

  async QuestionTextIsRequiredAsync(isRequired: boolean = true) {
    await this.IsRequiredAsync(questionText_Textbox(this.page), isRequired);
  }

  async QuestionAnswerListIsRequiredAsync(isRequired: boolean = true) {
    await this.IsRequiredAsync(questionList_Textbox(this.page), isRequired);
  }

  async QuestionRationaleIsRequiredAsync(isRequired: boolean = true) {
    await this.IsRequiredAsync(
      questionRationale_Textbox(this.page),
      isRequired
    );
  }

  async QuestionStandardOrCustomIsRequiredAsync(isRequired: boolean = true) {
    await this.IsRequiredAsync(
      questionStandardOrCustom_Combobox(this.page),
      isRequired
    );
  }

  async QuestionTitleIsRequiredAsync(isRequired: boolean = true) {
    await this.IsRequiredAsync(questionTitle_Textbox(this.page), isRequired);
  }

  async QuestionScriptorNotesIsRequiredAsync(isRequired: boolean = true) {
    await this.IsRequiredAsync(
      questionScriptorNotes_Textbox(this.page),
      isRequired
    );
  }

  async QuestionSingleOrMulticodeIsRequiredAsync(isRequired: boolean = true) {
    await this.IsRequiredAsync(
      questionSingleOrMulticode_Combobox(this.page),
      isRequired
    );
  }

  async QuestionTypeHasExpectedOptionsAsync(expecteds: string[]) {
    await questionType_Combobox(this.page).waitFor({ state: "visible" });
    await questionType_Combobox(this.page).click();
    for (const exp of expecteds) {
      const combobox = await this.page
        .getByRole("option", {
          name: exp,
          exact: true,
        })
        .allTextContents();
      expect(combobox).toContain(exp);
    }
  }

  async CreateQuestionAsync(question: QuestionCreationForm) {
    await this.FillQuestionCreationForm(question);
  }

  async FillQuestionCreationForm(question: QuestionCreationForm) {
    await this.fillQuestionName(question.questionName);

    await this.ChooseTypeCombobox(question.questionType);

    await this.fillQuestionText(question.questionText);

    await this.fillQuestionList(question.questionAnswer);

    await this.fillQuestionRationale(question.questionRationale);

    await this.ChooseStandardOrCustomCombobox(
      question.questionStandardOrCustom
    );

    await this.fillQuestionTitle(question.questionTitle);

    await this.fillScriptorNotes(question.questionScriptorNotes);

    await this.ChooseSingleOrMulticodeCombobox(
      question.questionSingleOrMulticode
    );

    await this.clicksaveCloseButton();
  }

  async EditQuestionAsync(
    questionName: string,
    question: QuestionCreationForm
  ) {
    await this.FilterByQuestionNameRowAsync(questionName);
    await question_row_Checkbox(this.page).click();
    await this.clickEditButton();
    await this.FillQuestionCreationForm(question);
    await this.clicksaveCloseButton();
  }

  async ValidateQuestion(question: QuestionCreationForm) {
    await this.FilterByQuestionNameRowAsync(question.questionName);
    const row = await this.GetRowByTextAsync(question.questionName);
    expect(row).not.toBeNull();
  }
}
