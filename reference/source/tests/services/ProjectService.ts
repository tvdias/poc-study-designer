import { Project } from '../selectors/ProjectSelectors.json';
import { Common } from '../selectors/CommonSelector.json';
import { WebHelper } from '../utils/WebHelper';
import { TestData } from '../Test Data/ProjectData.json';
import { DropDownList } from '../constants/DropDownList.json';
import { Page, expect } from '@playwright/test';

export class ProjectService {
    protected page: Page;
    private webHelper: WebHelper;

    constructor(page: Page) {
        this.page = page;
        this.webHelper = new WebHelper(this.page);
    }

    async CreateProject(projectName: string): Promise<void> {
        try {
            await this.webHelper.goToEntity(Project.Text.Projects);
            await this.webHelper.clickOnCommandBarBtn(Common.Text.New);
            await this.webHelper.verifySaveButton();
            await this.webHelper.clickOnHideFormButton()
            await this.webHelper.enterTextByLabel(Project.AriaLabel.ProjectName, projectName);
            await this.webHelper.autoSelectOptionSet(Project.AriaLabel.Methodology, DropDownList.Project.CATI)
            await this.webHelper.selectLookupByAriaLabel(Project.AriaLabel.ClientLookUp, TestData.Client)
            await this.webHelper.selectLookupByAriaLabel(Project.AriaLabel.CommissioningMarketLookUp, TestData.CommissioningMarket)
            await this.webHelper.enterTextByLabel(Project.AriaLabel.Description, projectName);
            await this.webHelper.saveRecord();
            await this.webHelper.clickonContinueAnyway();

            console.log(`New project created: ${projectName}`);
        } catch (e) {
            console.log(`Error while creating a project record : ${(e as Error).message}`);
            throw e;
        }
    }

    async CreateCustomProject(projectName: string, methodology: string, client: string, commissionMarket: string, description: string): Promise<void> {
        try {
            await this.webHelper.goToEntity(Project.Text.Projects);
            await this.webHelper.clickOnCommandBarBtn(Common.Text.New);
            await this.webHelper.verifySaveButton();
            await this.webHelper.clickOnHideFormButton();
            await this.webHelper.enterTextByLabel(Project.AriaLabel.ProjectName, projectName);
            await this.webHelper.autoSelectOptionSet(Project.AriaLabel.Methodology, methodology)
            await this.webHelper.selectLookupByAriaLabel(Project.AriaLabel.ClientLookUp, client)
            await this.webHelper.selectLookupByAriaLabel(Project.AriaLabel.CommissioningMarketLookUp, commissionMarket)
            await this.webHelper.enterTextByLabel(Project.AriaLabel.Description, description);
            await this.webHelper.saveRecord();
            await this.webHelper.clickonContinueAnyway();
            await this.webHelper.verifyTheSpantext(Project.Text.Questions);

            console.log(`New project created: ${projectName}`);
        } catch (e) {
            console.log(`Error while creating a project record : ${(e as Error).message}`);
            throw e;
        }
    }
    async searchForProject(projectName: string): Promise<void> {
        try {
            await this.webHelper.goToEntity(Project.Text.Projects);
            await this.webHelper.enterTextByPlaceHolder(Common.Placeholder.AskAboutData, projectName);
            await this.webHelper.clickonLinkText(projectName);
        } catch (e) {
            console.log(`Error while Opening the a project record : ${(e as Error).message}`);
            throw e;
        }
    }
    async enterRecordNameInFilterTextbox(projectName: string): Promise<void> {
        try {

            await this.webHelper.enterTextByPlaceHolder(Common.Placeholder.Filterbykeyword, projectName);

        } catch (e) {
            console.log(`Error while searching the a project record : ${(e as Error).message}`);
            throw e;
        }
    }

    async addProductTemplate(productName: string): Promise<void> {
        try {
            await this.webHelper.clickOnTab(Project.Tabs.ProjectDetails);
            await this.webHelper.selectLookupByAriaLabel(Project.AriaLabel.ProductLookUp, productName)
            await this.webHelper.saveRecord();
            await this.page.waitForLoadState("load");
            await this.page.waitForLoadState();
        } catch (e) {
            console.log(`Error while adding product: ${(e as Error).message}`);
            throw e;
        }
    }
    async deleteProductTemplate(btnName: string): Promise<void> {
        try {
            await this.webHelper.clickOnTab(Project.Tabs.ProjectDetails);
            await this.webHelper.clickOnButton(btnName)
            await this.webHelper.saveRecord();
            await this.page.waitForLoadState("load");
            await this.page.waitForLoadState();
        } catch (e) {
            console.log(`Error while removing the product: ${(e as Error).message}`);
            throw e;
        }
    }
    async addQuestion(question: string): Promise<void> {
        try {
            await this.webHelper.clickOnCommandBarBtn(Project.AriaLabel.AddQuestions);
            await this.webHelper.selectLookupByAriaLabel(Project.AriaLabel.AddExistingUserLookUp, question)
            await this.webHelper.clickOnButton(Project.ByRole.Add);
            await this.webHelper.clickOnConfirmationPopup(Common.Text.OK);
            await this.webHelper.saveRecord();

        } catch (e) {
            console.log(`Error while adding question: ${(e as Error).message}`);
            throw e;
        }
    }
    async addModule(module: string): Promise<void> {
        try {

            await this.webHelper.clickOnCommandBarBtn(Project.AriaLabel.AddModules);

            await this.webHelper.selectLookupByAriaLabel(Project.AriaLabel.AddExistingUserLookUp, module)

            await this.webHelper.clickOnButton(Project.ByRole.Add);
            await this.webHelper.saveRecord();

        } catch (e) {
            console.log(`Error while adding Module: ${(e as Error).message}`);
            throw e;
        }
    }


    async applyProductTemplate(): Promise<void> {
        try {
            await this.webHelper.saveRecord();
            await this.webHelper.clickOnCommandBarBtn(Project.Button.ApplyProductTemplate);
            await this.webHelper.clickOnConfirmationPopup(Common.Text.Yes);
            await this.page.waitForTimeout(7000); //wait for applying product template
            await this.webHelper.clickOnConfirmationPopup(Common.Text.OK);
            await this.webHelper.saveRecord();

        } catch (e) {
            console.log(`Error while applying product template : ${(e as Error).message}`);
            throw e;
        }
    }

    async validateNoOfMasterQuestionnaire(NoOfQuestions: number): Promise<void> {
        try {
            await this.webHelper.clickOnCommandBarBtn(Common.AriaLabel.Refresh);
            await expect(this.page.locator(Project.CSS.MasterQuestionsCount)).toHaveCount(NoOfQuestions);
        } catch (e) {
            console.log(`Error while validating master questionnaire : ${(e as Error).message}`);
            throw e;
        }
    }

    async waitUntilConfigQuestionsVisible(): Promise<void> {
        try {
            await this.webHelper.clickOnTab(Project.Tabs.ProjectDetails);
            const canvasAppIframe1 = this.page.frameLocator(Project.CSS.ConfigQuestionsAppIframe1)
            const canvasAppIframe2 = canvasAppIframe1.frameLocator(Project.CSS.ConfigQuestionsAppIframe2);
            const configQustions = canvasAppIframe2.locator(Project.CSS.ConfigQuestions);
            await configQustions.first().waitFor({ state: "visible" });
            await configQustions.first().waitFor({ state: "attached" })
            await expect(configQustions.first()).toBeVisible();

        } catch (e) {
            console.log(`Error while fetching configuration questions : ${(e as Error).message}`);
            throw e;
        }
    }

    async selectConfigQuestionAnswers(answers: string[]): Promise<void> {
        try {
            const canvasAppIframe1 = this.page.frameLocator(Project.CSS.ConfigQuestionsAppIframe1)
            const canvasAppIframe2 = canvasAppIframe1.frameLocator(Project.CSS.ConfigQuestionsAppIframe2);
            for (const answer of answers) {
                const element = canvasAppIframe2.locator("//*[text()='" + answer + "']");
                await expect(element.first()).toBeVisible();
                const box = await element.boundingBox();
                if (box) {
                    await this.page.mouse.move(box.x + box.width / 2, box.y + box.height / 2);
                    await this.page.waitForTimeout(500);
                    await this.page.mouse.click(box.x + box.width / 2, box.y + box.height / 2);
                }

            }
        } catch (e) {
            console.log(`Error while selecting configuration questions : ${(e as Error).message}`);
            throw e;
        }
    }

    async getMasterQuestionnaireLinesCount(): Promise<number> {
        const questionnaireVariable = Project.CSS.QuestionnaireVariable
        try {
            await this.webHelper.saveRecord();
            await expect(this.page.locator(questionnaireVariable).nth(0)).toBeVisible();
            const masterQuestionnaireLines = this.page.locator(questionnaireVariable);
            return await masterQuestionnaireLines.count();
        } catch (e) {
            console.log(`Error while fetching master questionnaire lines count : ${(e as Error).message}`);
            throw e;
        }
    }

    async getMasterQuestionnaireLinesVariableName(): Promise<string[]> {
        const questionnaireVariable = Project.CSS.QuestionnaireVariable
        try {
            var variableNames: string[] = [];
            await expect(this.page.locator(questionnaireVariable).nth(0)).toBeVisible();
            const masterQuestionnaireLines = this.page.locator(questionnaireVariable);
            const masterQuestionsCounts = await masterQuestionnaireLines.count();
            for (let index = 0; index < masterQuestionsCounts; index++) {
                const text = await masterQuestionnaireLines.nth(index).textContent();
                if (text) {
                    variableNames.push(text.trim());
                }
            }
            return variableNames;
        } catch (e) {
            console.log(`Error while fetching master questionnaire lines variables names : ${(e as Error).message}`);
            throw e;
        }
    }

    async addScripterUserToProject(user: string = Project.ByRole.ScripterUserName): Promise<void> {
        try {
            await this.webHelper.clickOnTab(Project.Tabs.UserManagement);

            await this.webHelper.clickOnHideFormButton();
            await expect(this.page.getByRole('region', { name: Project.ByRole.TeamMembers })).toBeVisible();
            await this.page.getByRole('region', { name: Project.ByRole.TeamMembers }).waitFor({ state: "visible" });

            await this.page.getByRole('menuitem', { name: Project.ByRole.AddExistingUser }).click();
            await this.webHelper.selectLookupByAriaLabel(Project.AriaLabel.AddExistingUserLookUp, user)
            await this.page.getByRole('button', { name: Project.ByRole.Add }).waitFor({ state: "visible" });
            await this.page.getByRole('button', { name: Project.ByRole.Add }).click();
            await this.page.waitForTimeout(2000);
            await expect(this.page.getByRole('link', { name: user })).toBeVisible();

            await this.webHelper.saveRecord();

        } catch (e) {
            console.log(`Error while adding scripter to the project : ${(e as Error).message}`);
            throw e;
        }
    }

    async deactivateFirstQuestionnaireFromMasterQuestionnaireLines(): Promise<string> {
        const deactivateBtn = Common.Text.Deactivate;
        const question = Project.CSS.QuestionnaireVariable;
        try {

            await this.webHelper.clickOnButton(question);
            await this.webHelper.clickOnButton(Project.Button.Edit);

            await this.page.getByRole('menuitem', { name: Project.ByRole.SeeAssociatedRecords }).click();

            const questionVariableName = await this.page.locator(Project.CSS.FirstQuestionnaireVariableName).first().innerText();
            await this.page.locator(Project.CSS.FirstCheckBox).first().click();

            await this.webHelper.clickOnCommandBarBtn(deactivateBtn);

            await this.page.getByRole('button', { name: deactivateBtn }).click();

            return questionVariableName;
        } catch (e) {
            console.log(`Error while deactivating questionnaire line from master questionnaire lines : ${(e as Error).message}`);
            throw e;
        }
    }
    async expandFirstQuestionFromQuestionnaire(): Promise<void> {
        const question = Project.CSS.QuestionnaireVariable;
        try {

            await this.page.locator(question).first().click();
        } catch (e) {
            console.log(`Error while expanding the question from  questionnaire : ${(e as Error).message}`);
            throw e;
        }
    }
    async clickonButton(buttonName: string): Promise<string> {
        try {
            const questionVariableName = await this.webHelper.getTextValue(Project.AriaLabel.QuestionVariableName);
            await this.webHelper.clickOnCommandBarBtn(buttonName);
            await this.page.getByRole('button', { name: buttonName }).click();
            return questionVariableName;

        } catch (e) {
            console.log(`Error while clicking on Button : ${(e as Error).message}`);
            throw e;
        }
    }
    async dragAndDropTheMasterQuestionnaireLines(): Promise<string> {
        try {
            await this.webHelper.performDragAndDrop(Project.AriaLabel.DragToReorder);
            await this.webHelper.saveRecord();
            await this.webHelper.saveRecord();
            await this.page.locator(Project.CSS.QuestionnaireVariableNames).first().focus();
            const questionVariableName = await this.page.locator(Project.CSS.QuestionnaireVariableNames).first().innerText();

            return questionVariableName;
        } catch (e) {
            console.log(`Error while drag and droping the questionnaires from master questionnaire  : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifydragAndDropTheMasterQuestionnaireLines(isVisible: boolean = true): Promise<void> {
        try {
            if (isVisible) {
                await this.page.getByRole("button", { name: Project.AriaLabel.DragToReorder, exact: true }).first().waitFor();
                await this.page.getByRole("button", { name: Project.AriaLabel.DragToReorder, exact: true }).first().focus();
                console.log(` drag and drop button is displayed the questionnaires from master questionnaire `);
            }
            else {
                await this.page.getByRole("button", { name: Project.AriaLabel.DragToReorder, exact: true }).first().waitFor({ state: 'hidden' });
                console.log(` drag and drop button is not displayed the questionnaires from master questionnaire `);

            }
        } catch (e) {
            console.log(`Error while verifying the drag and drop button the questionnaires from master questionnaire  : ${(e as Error).message}`);
            throw e;
        }
    }
    async validateQuestionnairesAfterReOrder(masterVariableNames: string[], variableNames: string[]) {
        try {
            expect(masterVariableNames[1]).toEqual(variableNames[0]);
            expect(masterVariableNames[0]).toEqual(variableNames[1]);
            console.log(`master variable names:${masterVariableNames}`);

        } catch (e) {
            console.log(`Error while comparing master questionnaire lines after Reorder : ${(e as Error).message}`);
            throw e;
        }
    }
    async navigateToAnswersTabFromMasterQuestionnaireLines(): Promise<string> {
        try {
            const questionVariableName = await this.webHelper.getTextValue(Project.AriaLabel.QuestionVariableName);
            await this.webHelper.clickOnTab(Project.Tabs.Answers)
            return questionVariableName;
        } catch (e) {
            console.log(`Error while navigating Answers Tab from master questionnaire lines : ${(e as Error).message}`);
            throw e;
        }
    }
    async updateQuestionVariableName(variableName: string): Promise<void> {
        try {
            await this.webHelper.verifySaveButton();
            await this.webHelper.enterTextByRoleTextbox(Project.AriaLabel.QuestionVariableName, variableName);
            await this.webHelper.saveRecord();
        } catch (e) {
            console.log(`Error while updating the Question variable name: ${(e as Error).message}`);
            throw e;
        }
    }
    async updateQuestionTitle(title: string): Promise<void> {
        try {
            await this.webHelper.verifySaveButton();
            await this.webHelper.enterTextByRoleTextbox(Project.AriaLabel.QuestionTitle, title);
            await this.webHelper.saveRecord();
        } catch (e) {
            console.log(`Error while updating the Question Title: ${(e as Error).message}`);
            throw e;
        }
    }
    async getQuestionVariableName(): Promise<string> {
        try {
            const questionVariableName = await this.webHelper.getTextValue(Project.AriaLabel.QuestionVariableName);
            return questionVariableName;
        } catch (e) {
            console.log(`Error while getting the Question variable name : ${(e as Error).message}`);
            throw e;
        }
    }
    async AddAnswerToTheList(text: string, code: string): Promise<void> {
        try {
            await this.webHelper.clickOnCommandBarBtn(Project.AriaLabel.AddAnswer);
            await this.webHelper.closeAIAlerts();
            await this.webHelper.closeAIForm();

            await this.webHelper.enterTextByRoleTextbox(Project.AriaLabel.AnswerText, text);
            //await this.webHelper.enterTextByRoleTextbox(Project.AriaLabel.AnswerCode, code);

            await this.webHelper.saveRecord();

            await this.webHelper.clickOnConfirmationPopup(Common.Text.OK);

            await this.webHelper.saveAndCloseRecord();

            await this.webHelper.verifyButtonText(text);

        } catch (e) {
            console.log(`Error while Adding the Answers : ${(e as Error).message}`);
            throw e;
        }
    }
    async AddAnswerTextToTheList(text: string): Promise<void> {
        try {
            await this.webHelper.clickOnCommandBarBtn(Project.AriaLabel.AddAnswer);
            await this.webHelper.verifySaveButton();
            await this.webHelper.clickOnHideFormButton();
            await this.webHelper.enterTextByRoleTextbox(Project.AriaLabel.AnswerText, text);
            await this.webHelper.saveRecord();
            await this.webHelper.saveAndCloseRecord();
            await this.webHelper.verifyButtonText(text);

        } catch (e) {
            console.log(`Error while Adding the Answers : ${(e as Error).message}`);
            throw e;
        }
    }

    async openAnswersListInGrid(): Promise<void> {
        const questionvariable = Project.CSS.ConfigQuestions;
        try {
            await this.webHelper.clickOnCommandBarBtn(Project.AriaLabel.MoreCommandsForQuestionnaireLinesAnswerList);
            await this.webHelper.clickOnCommandBarBtn(Project.AriaLabel.ShowAs);
            await this.webHelper.clickOnCommandBarBtn(Project.AriaLabel.ReadOnlyGrid);


        } catch (e) {
            console.log(`Error while opening the Answers in Readonly Grid : ${(e as Error).message}`);
            throw e;
        }
    }
    async openAnswersAndChangeProperties(answer: string): Promise<void> {
        try {
            await this.webHelper.clickOnButton(answer);
            await this.webHelper.clickOnSwitchbutton(Project.AriaLabel.IsOpenYes);
            await this.webHelper.clickOnSwitchbutton(Project.AriaLabel.IsExclusiveYes);
            await this.webHelper.clickOnSwitchbutton(Project.AriaLabel.IsFixedYes);
            await this.webHelper.clickOnSwitchbutton(Project.AriaLabel.IsActiveYes);

            await this.webHelper.saveRecord();
            await this.page.waitForTimeout(2000);
            await this.webHelper.saveAndCloseRecord();

            await this.page.waitForTimeout(2000);

        } catch (e) {
            console.log(`Error while opening the Answer Form : ${(e as Error).message}`);
            throw e;
        }
    }
    async deactiavteTheAnswer(title: string): Promise<void> {
        try {
            await this.webHelper.selectTheRow();
            await this.webHelper.clickOnMenuButton(title, 1);
            await this.webHelper.clickOnConfirmationPopup(title);

        } catch (e) {
            console.log(`Error while deactivating the Answer : ${(e as Error).message}`);
            throw e;
        }
    }

    async validateQuestionAndModuleAdded(question: string, module: string): Promise<void> {
        try {
            await this.page.locator(Project.CSS.firstRowQuestionVariableName).first().focus();
            await this.webHelper.verifyGridCellText(question);
            await this.webHelper.verifyGridCellText(module);

        } catch (e) {
            console.log(`Error while verifying the Question and Module from Questionnaire : ${(e as Error).message}`);
            throw e;
        }
    }
    async signedOutTheApplication(account: string, signout: string): Promise<void> {
        try {
            await this.webHelper.clickOnButton(account);
            await this.webHelper.clickOnButton(signout);
        } catch (error) {
            console.log(`Error in signout the application: ${(error as Error).message}`);
            throw error;
        }

    }
    async verifyProjectFieldsAreNotEditable(): Promise<void> {
        try {
            await this.webHelper.clickOnTab(Project.Tabs.ProjectDetails);
            await this.webHelper.VerifyInputFieldsReadonly(Project.AriaLabel.ProjectName);

            await this.webHelper.VerifyInputFieldsReadonly(Project.AriaLabel.Description);
            await this.webHelper.verifyTheEntity(Project.Text.ReadOnly)

            console.log(`Project Fields are Readonly and Not Editable`);
        } catch (e) {
            console.log(`Error while validating project fields : ${(e as Error).message}`);
            throw e;
        }
    }

    async getQuestionnaireLinesVariableName(): Promise<string[]> {
        const questionnaireVariable = Project.CSS.QuestionnaireVariableNames
        try {
            await this.page.waitForTimeout(2000);
            var variableNames: string[] = [];
            await expect(this.page.locator(questionnaireVariable).nth(0)).toBeVisible();
            const masterQuestionnaireLines = this.page.locator(questionnaireVariable);
            const masterQuestionsCounts = await masterQuestionnaireLines.count();
            for (let index = 0; index < masterQuestionsCounts; index++) {
                const text = await masterQuestionnaireLines.nth(index).textContent();
                if (text) {
                    variableNames.push(text.trim());
                }
            }
            return variableNames;
        } catch (e) {
            console.log(`Error while fetching master questionnaire lines variables names : ${(e as Error).message}`);
            throw e;
        }
    }

    async validateConfigQuestionsInCanvas(question: string, position: number): Promise<void> {
        try {
            await this.webHelper.clickOnTab(Project.Tabs.ProjectDetails);

            const canvasAppIframe1 = this.page.frameLocator(Project.CSS.ConfigQuestionsAppIframe1)
            const canvasAppIframe2 = canvasAppIframe1.frameLocator(Project.CSS.ConfigQuestionsAppIframe2);
            const configQustions = canvasAppIframe2.locator(Project.CSS.ConfigQuestions);
            await configQustions.first().waitFor({ state: "visible" });
            await configQustions.first().waitFor({ state: "attached" });
            await configQustions.nth(position).scrollIntoViewIfNeeded();
            await configQustions.nth(position).focus();
            await expect(configQustions.nth(position)).toHaveText(question);
        } catch (e) {
            console.log(`Error while validating the configuration questions : ${(e as Error).message}`);
            throw e;
        }
    }

    async validateProductTemplatePopupmessage(): Promise<void> {
        try {
            await this.webHelper.saveRecord();
            await this.webHelper.clickOnCommandBarBtn(Project.Button.ApplyProductTemplate);
            const dialogueText = await this.webHelper.getLocatorText(Project.CSS.prodcutTemplateDialogue);
            await expect(dialogueText).toBe(TestData.prodcutTemplateDialogueText);

        } catch (e) {
            console.log(`Error while validating the applying product template Popup message : ${(e as Error).message}`);
            throw e;
        }
    }
}