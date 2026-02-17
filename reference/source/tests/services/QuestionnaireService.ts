import { Project } from '../selectors/ProjectSelectors.json';
import { Questionnaire } from '../selectors/QuestionnaireSelector.json';
import { QuestionBank } from '../selectors/QuestionBankSelectors.json';
import { Common } from '../selectors/CommonSelector.json';
import { WebHelper } from '../utils/WebHelper';
import { Page, expect } from '@playwright/test';
import { waitUntilAppIdle } from '../utils/Login';

export class Questionnaireservice {
    protected page: Page;
    private webHelper: WebHelper;

    constructor(page: Page) {
        this.page = page;
        this.webHelper = new WebHelper(this.page);
    }

    async validateQuestionsAddedInQuestionnaire(questionName: string, type: string, text: string): Promise<void> {
        try {
            await this.webHelper.clickOnTab(Questionnaire.Tabs.Questionnaire);
            await this.verifyQuestionsCollapsed(0);
            await this.webHelper.verifyTheSpantext(text);
            await this.webHelper.verifyTheLabeltext(type);
            await this.webHelper.verifyTheSpantext(questionName);

        } catch (e) {
            console.log(`Error while validating the Questions in Questionnaire : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyDummyQuestionBadgeIsDisplayed(dummy: string): Promise<void> {
        try {
            await this.webHelper.verifyTheSpantext(dummy);

        } catch (e) {
            console.log(`Error while validating the Dummy Question in Questionnaire : ${(e as Error).message}`);
            throw e;
        }
    }

    async getCountOfColumnsInAnswerGrid(): Promise<number> {
        try {
            return await this.webHelper.getCountOfColumns(Common.CSS.RowsColumns);

        } catch (e) {
            console.log(`Error while getting Coloumn count Under Rows in Questionnaire: ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyQuestionnairebutton(button: string, isVisible: boolean = true): Promise<void> {
        try {
            await this.webHelper.verifytheButton(button, isVisible);
        } catch (e) {
            console.log(`Error while verifying the buttons in Questionnaire: ${(e as Error).message}`);
            throw e;
        }
    }
    async deleteTheQuestionsInQuestionnaire(): Promise<void> {
        try {
            await this.webHelper.clickOnButtonByDataTestID(Questionnaire.TestDataId.DeleteButton);

            await this.webHelper.clickOnButton(Common.Text.Delete);


        } catch (e) {
            console.log(`Error while deleting the Questions in Questionnaire : ${(e as Error).message}`);
            throw e;
        }
    }
    async validateIconsForQuestionsAddedInQuestionnaire(questionName: string): Promise<void> {
        try {

            await this.verifyiconsForQuestion(questionName, 1);
            await this.verifyiconsForQuestion(questionName, 2);
            await this.verifyiconsForQuestion(questionName, 3);


        } catch (e) {
            console.log(`Error while validating icons for the the Questions in Questionnaire : ${(e as Error).message}`);
            throw e;
        }
    }

    async validateTheDataAddedInQuestionnaire(questionName: string, type: string, title: string, text: string, AnswerCode: string, AnswerText: string, scripterNotes: string, formateDetails: string, SortOrder: string, minLength: string, maxLength: string, answerType: string): Promise<void> {
        try {

            await this.webHelper.verifyTheSpantext(questionName);
            await this.webHelper.verifyTheParagraphText(text);
            await expect(this.page.getByLabel('New Section').getByText(`${title}`)).toBeVisible();
            await this.webHelper.verifyTheSpantext(type);
            await this.webHelper.verifyTheCellText(AnswerCode);
            await this.webHelper.verifyTheCellText(AnswerText);
            await this.webHelper.verifyTheCellText(answerType);
            await this.webHelper.verifyTheEntity(SortOrder);
            await this.webHelper.verifyTheEntity(scripterNotes);
            await this.webHelper.verifyTheEntity(formateDetails);
            if (minLength !== "")
                await this.webHelper.verifyTheParagraphText(maxLength);


        } catch (e) {
            console.log(`Error while validating the Data in Questionnaire : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyQuestionDataInQuestionnaire(text: string, formatdetails: string): Promise<void> {
        try {

            await this.webHelper.verifyTheSpantext(text);
            await this.webHelper.verifyTheEntity(formatdetails);

        } catch (e) {
            console.log(`Error while validating the Questions in Questionnaire : ${(e as Error).message}`);
            throw e;
        }
    }
    async validateTheAnswersInQuestionnaire(AnswerCode: string, AnswerText: string, answerType: string, isVisible: boolean = true): Promise<void> {
        try {

            await this.webHelper.verifyTheCellText(AnswerCode, isVisible);
            await this.webHelper.verifyTheCellText(AnswerText, isVisible);
            await this.webHelper.verifyTheCellText(answerType, isVisible);

        } catch (e) {
            console.log(`Error while validating the Answers data in Questionnaire : ${(e as Error).message}`);
            throw e;
        }
    }
    async validateTheAnswerText(AnswerText: string, isVisible: boolean = true): Promise<void> {
        try {
            await this.webHelper.verifyTheCellText(AnswerText, isVisible);

        } catch (e) {
            console.log(`Error while validating the Answers Text in Questionnaire : ${(e as Error).message}`);
            throw e;
        }
    }
    async validateTheAnswersPropertiesInQuestionnaire(answerText: string, answerCode: string, answerType: string, isVisible: boolean = true): Promise<void> {
        try {

            //await this.webHelper.verifyTheCellText(answerCode);
            await this.webHelper.verifyTheCellText(answerText);
            await this.webHelper.verifyTheCellText(answerType, isVisible);


        } catch (e) {
            console.log(`Error while validating the Answers Properties in Questionnaire : ${(e as Error).message}`);
            throw e;
        }
    }
    async expandQuestionnaire(type: string): Promise<void> {
        try {

            await this.webHelper.clickOnButton(type);

        } catch (e) {
            console.log(`Error while expanding the Question : ${(e as Error).message}`);
            throw e;
        }
    }
    async checkForDuplicateRecords(questions: string[]): Promise<void> {
        try {

            const uniqueRows = new Set(questions);
            await expect(uniqueRows.size).toBe(questions.length);

        } catch (e) {
            console.log(`Error while verifying the dupcplicate Questions : ${(e as Error).message}`);
            throw e;
        }
    }
    async validateModulesAddedInQuestionnaire(modulename: string, questionName: string, type: string, text: string, position: number = 1): Promise<void> {
        try {

            await this.webHelper.clickOnTab(Questionnaire.Tabs.Questionnaire);

            await this.verifyQuestionsCollapsed(position);


            await this.webHelper.verifyTheSpantext(text);
            await this.webHelper.verifyTheLabeltext(type);
            await this.webHelper.verifyTheSpantext(modulename);
            await this.webHelper.verifyTheSpantext(questionName);

        } catch (e) {
            console.log(`Error while validating the Module in Questionnaire : ${(e as Error).message}`);
            throw e;
        }
    }
    async validateQuestion(questionName: string): Promise<void> {
        try {
            await this.webHelper.verifyTheSpantext(questionName);

        } catch (e) {
            console.log(`Error while validating the Questions in Questionnaire : ${(e as Error).message}`);
            throw e;
        }
    }
    async removeModule(moduleName1: string, moduleName2: string): Promise<void> {
        try {
            await this.webHelper.clickOnButton(Questionnaire.Text.RemoveModules);
            await this.webHelper.verifyTheEntity(Questionnaire.Text.RemoveModules)
            await this.webHelper.selectcheckbox("Select " + moduleName1);
            await this.webHelper.selectcheckbox("Select " + moduleName2);

            await this.webHelper.clickOnButton(Questionnaire.Text.RemoveSelected);

            await this.webHelper.clickOnConfirmationPopup(Common.Text.Yes);

            await this.webHelper.clickOnConfirmationPopup(Common.Text.OK);



        } catch (e) {
            console.log(`Error while removing the modules in Questionnaire : ${(e as Error).message}`);
            throw e;
        }
    }
    async clickOnRemoveModule(): Promise<void> {
        try {
            await this.webHelper.clickOnButton(Questionnaire.Text.RemoveModules);
            await this.webHelper.verifyTheEntity(Questionnaire.Text.RemoveModules)

        } catch (e) {
            console.log(`Error while removing the modules in Questionnaire : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyRemoveModule(isVisible: boolean = true): Promise<void> {
        try {
            await this.webHelper.verifyButtonText(Questionnaire.Text.RemoveModules, isVisible);
        } catch (e) {
            console.log(`Error while verifying the removing the module button in Questionnaire : ${(e as Error).message}`);
            throw e;
        }
    }

    async expandModuleInRemoveModuleSection(module: string): Promise<void> {
        try {
            await this.webHelper.clickOnSpantext(module);
        } catch (e) {
            console.log(`Error while Expanding the the module : ${(e as Error).message}`);
            throw e;
        }
    }
    async getCountofQuestionsInRemoveModule(): Promise<number> {
        try {
            return await this.webHelper.getCountofQuestionsRemoveModule();
        } catch (e) {
            console.log(`Error while Expanding the the module : ${(e as Error).message}`);
            throw e;
        }
    }
    async clickonSaveRecord(): Promise<void> {
        try {

            await this.webHelper.saveRecord();


            console.log(`Clicked on Save button`);
        } catch (e) {
            console.log(`Error while click on Save button : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyQuestionsCollapsed(position: number): Promise<void> {
        const locator = Questionnaire.CSS.QuestionnaireCollapsedRecord;
        try {
            await expect(this.page.locator(locator).nth(position)).toBeVisible();



        } catch (e) {
            console.log(`Error while validating Question in collapsed view : ${(e as Error).message}`);
            throw e;
        }
    }

    async verifyQuestionsStrikeout(text: string): Promise<void> {
        const locator = Questionnaire.CSS.StrikeOutQuestion;
        try {
            await expect(this.page.locator(locator)).toBeVisible();
            const variableName = await this.page.locator(locator).textContent();
            await expect(variableName).toBe(text);


        } catch (e) {
            console.log(`Error while validating strikeout Question in Inactive Questions : ${(e as Error).message}`);
            throw e;
        }
    }

    async verifyiconsForQuestion(questionName: string, position: number): Promise<void> {
        try {
            await expect(this.page.getByRole('button', { name: `${questionName}` }).getByRole('button').nth(position)).toBeVisible();

        } catch (e) {
            console.log(`Error while validating icons for the Question added in questionnaire  : ${(e as Error).message}`);
            throw e;
        }
    }

    async addQuestion(question: string, questionTitle: string, isVisible: boolean = true): Promise<void> {
        try {
            await this.webHelper.clickOnTab(Questionnaire.Tabs.Questionnaire);
            await this.webHelper.verifyTheSpantext(Questionnaire.Tabs.Questions);
            await this.webHelper.clickOnAddicon();
            await this.webHelper.enterTextByPlaceHolder(Questionnaire.placeholder.SearchQuestionModule, question);
            await this.webHelper.selectcheckbox(questionTitle);
            await this.webHelper.clickOnButton(Questionnaire.ByRole.Save);
            await this.webHelper.clickOnConfirmationPopup(Common.Text.OK);
            if (isVisible) {
                await this.webHelper.saveRecord();
            }


        } catch (e) {
            console.log(`Error while adding question: ${(e as Error).message}`);
            throw e;
        }
    }

    async verifyAddIconinQuestionnaire(isVisible: boolean = true): Promise<void> {
        try {

            await this.webHelper.validateAddicon(isVisible);

        } catch (e) {
            console.log(`Error while verifying the Add Icon: ${(e as Error).message}`);
            throw e;
        }
    }
    async clickonAddIconinQuestionnaire(): Promise<void> {
        try {
            await this.webHelper.clickOnAddicon();
        } catch (e) {
            console.log(`Error while Clicking the Add Icon in Questionnaire: ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyTabsinImportFromLibrary(tab: string): Promise<void> {
        try {
            await this.page.waitForTimeout(2000);
            await this.webHelper.verifyLocatorvalue(tab);
            console.log(`Verified the ${tab} tab in Import from Library`);
        } catch (e) {
            console.log(`Error while verifying the tabs in Questionnaire: ${(e as Error).message}`);
            throw e;
        }
    }
    async searchCustomQuestionFromStandardQuestionTab(question: string): Promise<void> {
        try {

            await this.webHelper.clickOnTab(Questionnaire.Tabs.Questionnaire);
            await this.page.waitForTimeout(1000);
            await this.webHelper.clickOnAddicon();
            await this.webHelper.enterTextinSearchbox(Questionnaire.placeholder.SearchQuestionModule, question);
        } catch (e) {
            console.log(`Error while search the question: ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyNoResultsMatchMessage(): Promise<void> {
        try {
            await this.webHelper.verifyText(Questionnaire.Text.NoResultsMatching);
            await this.webHelper.clickOnButton(Common.Text.QCCancel);
        } catch (e) {
            console.log(`Error while verifying the No Results Match Message ${(e as Error).message}`);
            throw e;
        }
    }
    async searchStandardQuestionFromCustomQuestionTab(question: string): Promise<void> {
        try {
            await this.webHelper.clickOnTab(Questionnaire.Tabs.Questionnaire);
            await this.page.waitForTimeout(1000);
            await this.webHelper.clickOnAddicon();
            await this.webHelper.clickOnTab(Questionnaire.Tabs.Custom);
            await this.webHelper.enterTextinSearchbox(Questionnaire.placeholder.SearchQuestionModule, question);
        } catch (e) {
            console.log(`Error while search the question: ${(e as Error).message}`);
            throw e;
        }
    }
    async validateAllTabinSingleView(isVisible: boolean = true): Promise<void> {
        try {

            await this.webHelper.verifyTheTab(Questionnaire.Tabs.Questionnaire);
            await this.webHelper.verifyTheTab(Common.Tabs.Related);
            await this.webHelper.verifyTheTab(Project.Tabs.ProjectDetails);
            await this.webHelper.verifyTheTab(Project.Tabs.Studies);
            await this.webHelper.verifyTheTab(Project.Tabs.UserManagement);
            if (isVisible) {
                await this.webHelper.verifyTheTab(Common.Tabs.StudyQuestions);
            }
            await this.webHelper.verifyTheTab(Common.Tabs.ManagedLists);

        } catch (e) {
            console.log(`Error while validating the tab in a single view ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyQuestionInImportFromLibrary(question: string, questionTitle: string, isVisible: boolean = true): Promise<void> {
        try {
            await this.webHelper.clickOnTab(Questionnaire.Tabs.Questionnaire);

            await this.webHelper.clickOnAddicon();
            await this.webHelper.enterTextByPlaceHolder(Questionnaire.placeholder.SearchQuestionModule, question);

            await this.webHelper.verifyThecheckbox(questionTitle, isVisible);

        } catch (e) {
            console.log(`Error while verifying the question: ${(e as Error).message}`);
            throw e;
        }
    }
    async addModule(modulename: string): Promise<void> {
        try {

            await this.webHelper.clickOnTab(Questionnaire.Tabs.Questionnaire);
            await this.webHelper.clickOnAddicon();
            await this.webHelper.clickOnTab(Common.Entity.Modules);
            await this.webHelper.enterTextinSearchbox(Questionnaire.placeholder.SearchQuestionModule, modulename);

            await this.webHelper.selectcheckbox(modulename);
            await this.webHelper.clickOnButton(Questionnaire.ByRole.Save);

            await this.webHelper.clickOnConfirmationPopup(Common.Text.OK);
            await this.webHelper.saveRecord();

        } catch (e) {
            console.log(`Error while adding Module: ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyModuleInImportFromLibrary(modulename: string, isVisible: boolean = true): Promise<void> {
        try {

            await this.webHelper.clickOnTab(Questionnaire.Tabs.Questionnaire);
            await this.webHelper.clickOnAddicon();
            await this.webHelper.clickOnTab(Common.Entity.Modules);
            await this.webHelper.enterTextinSearchbox(Questionnaire.placeholder.SearchQuestionModule, modulename);

            await this.webHelper.verifyThecheckbox(modulename, isVisible);

        } catch (e) {
            console.log(`Error while verifying the Module: ${(e as Error).message}`);
            throw e;
        }
    }

    async navigateToTheTab(tab: string): Promise<void> {
        try {
            await this.webHelper.clickOnTab(tab);
        } catch (e) {
            console.log(`Error while navigating to the Tab ${(e as Error).message}`);
            throw e;
        }
    }
    async addCustomQuestion(customqn: string): Promise<void> {
        try {
            await this.webHelper.clickOnTab(Questionnaire.Tabs.Questionnaire);
            await this.webHelper.clickOnAddicon();
            await this.webHelper.clickOnTab(Questionnaire.Tabs.Custom);
            await this.webHelper.enterTextinSearchbox(Questionnaire.placeholder.SearchQuestionModule, customqn);

            await this.webHelper.selectcheckbox(customqn);
            await this.webHelper.clickOnButton(Questionnaire.ByRole.Save);

            await this.webHelper.clickOnConfirmationPopup(Common.Text.OK);
            await this.webHelper.saveRecord();

        } catch (e) {
            console.log(`Error while adding Custom Question: ${(e as Error).message}`);
            throw e;
        }
    }

    async ModifyQuestionDetails(text: string, formatdetails: string): Promise<void> {
        try {
            await this.webHelper.enterTextByRoleTextbox(Questionnaire.AriaLabel.QuestionText, text);
            await this.webHelper.enterTextByRoleTextbox(Questionnaire.AriaLabel.QuestionFormatDetails, formatdetails);
            await this.webHelper.saveRecord();
        } catch (e) {
            console.log(`Error while Modifiying the Question details : ${(e as Error).message}`);
            throw e;
        }
    }
    async ModifyQuestionScripterNotes(scripterNotes: string): Promise<void> {
        try {
            await this.webHelper.enterTextByRoleTextbox(Questionnaire.AriaLabel.ScripterNotes, scripterNotes);
            await this.webHelper.saveRecord();
        } catch (e) {
            console.log(`Error while Modifiying Scripter Notes : ${(e as Error).message}`);
            throw e;
        }
    }
    async removeModuleFromQuestionnaire(moduleName1: string): Promise<void> {
        try {
            await this.webHelper.clickOnButton(Questionnaire.Text.RemoveModules);
            await this.webHelper.verifyTheEntity(Questionnaire.Text.RemoveModules)
            await this.webHelper.selectcheckboxByLocator(moduleName1);
            await this.webHelper.clickOnButton(Questionnaire.Text.RemoveSelected);
            await this.webHelper.clickOnConfirmationPopup(Common.Text.Yes);
            await this.webHelper.clickOnConfirmationPopup(Common.Text.OK);

        } catch (e) {
            console.log(`Error while removing the modules in Questionnaire : ${(e as Error).message}`);
            throw e;
        }
    }
    async filterQuestionsInQuestionnaire(question: string): Promise<void> {
        try {
            await this.webHelper.searchrecordInQuestionnaire(question);

        } catch (e) {
            console.log(`Error while filtering the Question in Questionnaire : ${(e as Error).message}`);
            throw e;
        }
    }
    async getAICodeForAnswerText(key: string): Promise<string[]> {
        try {
            await this.webHelper.verifySaveButton();
            await this.webHelper.saveRecord();
            await this.page.waitForTimeout(7000);
            await this.webHelper.saveRecord();
            await this.page.reload();
            await this.webHelper.verifySaveButton();
            await this.webHelper.clickOnTab(Project.Tabs.Answers);
            await this.webHelper.saveRecord();
            await this.page.reload();
            await this.webHelper.verifySaveButton();
            await this.webHelper.clickOnTab(Project.Tabs.Answers);
            await this.webHelper.saveRecord();
            return await this.webHelper.getAutomationKeyGridCellValue(key);

        } catch (e) {
            console.log(`Error while getting the AI Answer Code : ${(e as Error).message}`);
            throw e;
        }
    }
    async addCustomQuestionToQuestionnaire(customqn: string, qnTitle: string): Promise<void> {
        try {

            await this.webHelper.clickOnTab(Questionnaire.Tabs.Questionnaire);
            await this.webHelper.clickOnAddicon();
            await this.webHelper.clickOnTab(Questionnaire.Tabs.Custom);
            await this.webHelper.enterTextinSearchbox(Questionnaire.placeholder.SearchQuestionModule, customqn);
            await this.webHelper.selectcheckbox(qnTitle);
            await this.webHelper.clickOnButton(Questionnaire.ByRole.Save);
            await this.webHelper.clickOnConfirmationPopup(Common.Text.OK);
            await this.webHelper.saveRecord();

        } catch (e) {
            console.log(`Error while adding Custom Question: ${(e as Error).message}`);
            throw e;
        }
    }
    async toggleIsDummyQuestion(): Promise<void> {
        try {

            await this.webHelper.clickOnTab(Questionnaire.Tabs.QuestionProperties);
            await this.webHelper.verifySwitchbutton(QuestionBank.AriaLabel.IsDummyQuestionFalse);
            await this.webHelper.clickOnSwitchbutton(QuestionBank.AriaLabel.IsDummyQuestionFalse);
            await this.webHelper.saveRecord();
            await this.webHelper.saveAndCloseRecord();

        } catch (e) {
            console.log(`Error while  toggle the Dummy Question: ${(e as Error).message}`);
            throw e;
        }
    }

    async clickonEditbutton(): Promise<void> {
        try {
            await this.webHelper.clickOnButtonByDataTestID(Questionnaire.TestDataId.EditButton);
        } catch (e) {
            console.log(`Error while  click on Edit button from Questionnaire: ${(e as Error).message}`);
            throw e;
        }
    }

    async updateAnswerText(updatedText: string): Promise<void> {
        try {
            await this.webHelper.enterTextByRoleTextbox(Project.AriaLabel.AnswerText, updatedText);
            await this.webHelper.saveRecord();
            await this.webHelper.saveAndCloseRecord();
            await this.webHelper.verifyButtonText(updatedText);

        } catch (e) {
            console.log(`Error while updating the Answer Text: ${(e as Error).message}`);
            throw e;
        }
    }
    async clickOnAnswerText(isVisible: boolean = true): Promise<void> {
        const answerText = QuestionBank.CSS.AnswerText;
        try {
            await this.page.locator(answerText).first().waitFor();
            await this.page.locator(answerText).first().click();
            if (isVisible)
                await this.webHelper.verifySaveButton();
        } catch (e) {
            console.log(`Error while click on the Answer Text: ${(e as Error).message}`);
            throw e;
        }
    }

    async deactivateTheAnswer(): Promise<void> {
        try {
            await this.clickOnAnswerText();
            await this.webHelper.clickOnCommandBarBtn(Common.Text.Deactivate);
            await this.webHelper.verifySaveButton();
            await this.webHelper.clickOnButton(Common.Text.Deactivate);
            await this.page.waitForTimeout(3000);
            await this.webHelper.verifyNewButton();
        } catch (e) {
            console.log(`Error while deactivating the Answer ${(e as Error).message}`);
            throw e;
        }
    }
    async getAllAnswerTextsfromAnswerTab(): Promise<string[]> {
        var answerTexts: string[];
        try {
            await this.page.waitForTimeout(2000);
            await this.webHelper.verifySaveButton();
            answerTexts = await this.webHelper.getAllAnswerTexts(QuestionBank.CSS.AnswerText);
            return answerTexts;
        } catch (e) {
            console.log(`Error while  getting the Answer Text values in Answer Tab: ${(e as Error).message}`);
            throw e;
        }
    }

    async getAllAnswerTextsfromQuestionnaireTab(): Promise<string[]> {
        var answerTexts: string[];
        try {
            await this.page.waitForTimeout(2000);
            await this.webHelper.verifySaveButton();
            answerTexts = await this.webHelper.getAllAnswerTexts(Questionnaire.CSS.AnswerTextInQuestionnaire);
            return answerTexts;
        } catch (e) {
            console.log(`Error while  getting the Answer Text values in Questionnaire Tab: ${(e as Error).message}`);
            throw e;
        }
    }

    async addManageListInQuestionnaire(managedListName: string): Promise<void> {
        try {
            await this.webHelper.clickOnTab(Questionnaire.Tabs.ManagedLists);
            await this.webHelper.clickByAriaLabel(Questionnaire.AriaLabel.AddManagedListInQuestion);
            await this.webHelper.selectLookupByAriaLabel(Questionnaire.AriaLabel.ManagedListLookup, managedListName);
            await this.webHelper.saveAndCloseQuickCreateRecord();
            await this.webHelper.saveRecord();

        } catch (e) {
            console.log(`Error while adding Managed List in Questionnaire: ${(e as Error).message}`);
            throw e;
        }
    }
    async activateTheAnswer(): Promise<void> {
        try {
            await this.clickOnAnswerText(false);
            await this.webHelper.clickOnCommandBarBtn(Common.Text.Activate);
            await this.webHelper.clickOnButton(Common.Text.Activate);
        } catch (e) {
            console.log(`Error while Activating the Answer ${(e as Error).message}`);
            throw e;
        }
    }
    async updateAnswerCode(updatedcode: string, isVisible: boolean = true): Promise<void> {
        try {
            await this.webHelper.enterTextByRoleTextbox(Project.AriaLabel.AnswerCode, updatedcode);
            await this.webHelper.saveRecord();
            await this.webHelper.clickOnPopupButton(Common.Text.OK);
            await this.webHelper.saveAndCloseRecord();
            if (isVisible)
                await this.webHelper.verifyTheSpantext(updatedcode);

        } catch (e) {
            console.log(`Error while updating the Answer Code: ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyAndHovertheInfoIcon(questionName: string): Promise<void> {
        try {

            await this.verifyiconsForQuestion(questionName, 1);
            await this.webHelper.mouseHoverTheIcons(questionName, 1);

        } catch (e) {
            console.log(`Error while validating info icon for the the Questions in Questionnaire : ${(e as Error).message}`);
            throw e;
        }
    }
    async validatingToolTipText(questionName: string): Promise<void> {
        try {
            const text = await this.webHelper.getToolTipText(questionName, 1);

        } catch (e) {
            console.log(`Error while validating info icon tool tip text for the the Questions in Questionnaire : ${(e as Error).message}`);
            throw e;
        }
    }

}
