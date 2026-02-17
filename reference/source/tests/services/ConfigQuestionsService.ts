import { ConfigQuestions } from '../selectors/ConfigQuestionsSelector.json'
import { Common } from '../selectors/CommonSelector.json'
import { WebHelper } from '../utils/WebHelper';
import { Page, expect } from '@playwright/test';
import { waitUntilAppIdle } from '../utils/Login';
import { DropDownList } from '../constants/DropDownList.json';


export class ConfigQuestionsService {
    protected page: Page;
    private webHelper: WebHelper;

    constructor(page: Page) {
        this.page = page;
        this.webHelper = new WebHelper(this.page);
    }

    async createConfigQuestions(question: string, option: string): Promise<void> {
        try {
            await this.webHelper.goToEntity(Common.Entity.ConfigurationQuestions);
            await this.webHelper.clickOnCommandBarBtn(Common.Text.New);
            await this.webHelper.clickOnHideFormButton();
            await this.webHelper.enterTextByRoleTextbox(ConfigQuestions.AriaLabel.Question, question);
            //await this.webHelper.selectOptionSet(ConfigQuestions.AriaLabel.Rule, option);
            await this.webHelper.saveRecord();
            await this.webHelper.updateStatusFromDraftToActive();

        } catch (e) {
            console.log(`Error while creating a Config Question : ${(e as Error).message}`);
            throw e;
        }
    }

    async addConfigAnswers(answers: string[]): Promise<void> {

        for (let answer of answers) {
            try {
                await this.webHelper.verifySaveButton();
                await this.page.getByRole('menuitem', { name: ConfigQuestions.Buttons.AddNewConfigurationAnswer, exact: true }).first().waitFor({ state: "visible" });
                await this.webHelper.clickOnNewConfigurationAnswer(ConfigQuestions.AriaLabel.RichTextEditor);
                await this.webHelper.clickOnHideFormButton();
                await this.webHelper.enterTextByTitleTextbox(ConfigQuestions.Title.SelectToEnterData, answer);
                await this.webHelper.updateStatusFromDraftToActive();
                await this.webHelper.saveAndCloseRecord();

            } catch (e) {
                console.log(`Error while adding Config answer : ${(e as Error).message}`);
                throw e;
            }
        }
    }

    async addDependencyRule(ruleName: string, triggerAnswer: string, type: string, classification: string,
        contentType: string, contentTypeValue: string): Promise<void> {
        try {
            await this.webHelper.clickOnMenuBtn(ConfigQuestions.Buttons.AddNewDependencyRule);
            await this.webHelper.verifySaveButton();
            await this.webHelper.clickOnHideFormButton();
            await this.webHelper.enterTextByRoleTextbox(ConfigQuestions.AriaLabel.Name, ruleName);
            await this.webHelper.clickOnHideFormButton();
            await this.webHelper.selectLookupByAriaLabel(ConfigQuestions.AriaLabel.TriggeringAnswerLookup, triggerAnswer);
            await this.webHelper.saveRecord();
            await this.webHelper.selectOptionSet(ConfigQuestions.AriaLabel.Type, type);
            await this.webHelper.selectOptionSet(ConfigQuestions.AriaLabel.ContentType, contentType);

            if (contentType == 'Module') {
                await this.webHelper.selectLookupByAriaLabel(ConfigQuestions.AriaLabel.ModuleLookup, contentTypeValue);
            } else if (contentType == 'Question') {
                await this.webHelper.selectLookupByAriaLabel(ConfigQuestions.AriaLabel.QuestionBankLookup, contentTypeValue);
            }

            await this.webHelper.autoSelectOptionSet(ConfigQuestions.AriaLabel.Classification, classification);
            await this.webHelper.saveAndCloseRecord();

        } catch (e) {
            console.log(`Error while creating a dependency rule record : ${(e as Error).message}`);
            throw e;
        }
    }

    async activateDependencyRules(): Promise<void> {
        const dependencyRuleAllCheckbox = ConfigQuestions.CSS.DependencyRuleAllCheckbox
        try {

            await this.webHelper.clickOnByRoleTextbox(ConfigQuestions.AriaLabel.RichTextEditor);

            await this.page.keyboard.press('Tab');


            await this.page.keyboard.press('PageDown');

            await expect(this.page.locator(dependencyRuleAllCheckbox).first()).toBeVisible()
            await this.page.locator(dependencyRuleAllCheckbox).first().focus();
            await this.page.locator(dependencyRuleAllCheckbox).first().click();



            await this.webHelper.clickOnCommandBarBtn(Common.Text.Activate);


            await this.webHelper.selectOptionSet(Common.ByRole.StatusReason, DropDownList.Status.Active);
            await this.page.getByRole('button', { name: Common.Text.Activate }).click();

        } catch (e) {
            console.log(`Error while activating dependency rules : ${(e as Error).message}`);
            throw e;
        }
    }
}