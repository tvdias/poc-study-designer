import { Products } from '../selectors/ProductsSelectors.json';
import { ConfigQuestions } from '../selectors/ConfigQuestionsSelector.json';
import { Common } from '../selectors/CommonSelector.json';
import { WebHelper } from '../utils/WebHelper';
import { Page, expect } from '@playwright/test';
import { waitUntilAppIdle } from '../utils/Login';
import { DropDownList } from '../constants/DropDownList.json';


export class ProductsService {
    protected page: Page;
    private webHelper: WebHelper;

    constructor(page: Page) {
        this.page = page;
        this.webHelper = new WebHelper(this.page);
    }

    async createProduct(productName: string): Promise<void> {
        try {
            await this.webHelper.goToEntityAndClickNewButton(Common.Entity.Products);
            await this.webHelper.clickOnHideFormButton();
            await this.webHelper.validateMandatoryField(Products.Title.ProductName);
            await this.webHelper.validateOptionalField(Products.Title.ProductDescription);
            await this.webHelper.enterTextByRoleTextbox(Products.AriaLabel.ProductName, productName);
            await this.webHelper.saveRecord();

            await this.webHelper.validateStatusReasonInHeader(DropDownList.Status.Draft);

            await this.webHelper.updateStatusFromDraftToActive();
            await this.webHelper.validateStatusReasonInHeader(DropDownList.Status.Active);
            console.log(`Product created : ${productName}`);
        } catch (e) {
            console.log(`Error while creating a products record : ${(e as Error).message}`);
            throw e;
        }
    }

    async createProductTemplate(templateName: string, version: string): Promise<void> {
        try {
            await this.webHelper.clickOnTab(Products.Tabs.ProductTemplates);
            await this.webHelper.clickOnCommandBarBtn(Products.Buttons.NewProductTemplate);
            await this.page.waitForTimeout(2000);
            await this.webHelper.clickOnHideFormButton();
            await this.webHelper.closeAIForm();
            await this.webHelper.closeAIAlerts();
            await this.webHelper.closeAIForm();
            await this.webHelper.enterTextByLabel(Products.AriaLabel.ProductTemplateName, templateName);
            await this.webHelper.closeAIForm();
            await this.webHelper.closeAIAlerts();
            await this.webHelper.enterTextByLabel(Products.AriaLabel.Version, version);
            await this.webHelper.closeAIForm();
            await this.webHelper.closeAIAlerts();
            await this.webHelper.saveRecord();
            await this.webHelper.updateStatusFromDraftToActive();
            await this.webHelper.saveAndCloseRecord();
            console.log(`Created Product template : ${templateName}`);
        } catch (e) {
            console.log(`Error while creating a products record : ${(e as Error).message}`);
            throw e;
        }
    }

    async addConfigQuestions(configQuestion: string): Promise<void> {
        try {
            await this.webHelper.clickOnTab(Products.Tabs.ConfigurationQuestions);
            await this.webHelper.clickOnCommandBarBtn(Products.Buttons.NewProductConfigQuestion);

            await this.webHelper.selectLookupByAriaLabel(Products.AriaLabel.ConfigurationQuestionLookup, configQuestion);
            await this.webHelper.saveAndCloseQuickCreateRecord();

            console.log(`Added config question to product : ${configQuestion}`);
        } catch (e) {
            console.log(`Error while adding config questions : ${(e as Error).message}`);
            throw e;
        }
    }

    async openRecordFromGrid(recordName: string): Promise<void> {
        try {
            let locator = "//*[text()='" + recordName + "']";
            await expect(this.page.locator(locator).first()).toBeVisible();
            await this.page.locator(locator).first().waitFor({ state: "visible" });
            await this.page.waitForSelector(locator);
            await this.page.locator(locator).first().waitFor();
            await this.page.waitForTimeout(2000);
            await this.page.locator(locator).first().click();

        } catch (e) {
            console.log(`Error while opening record : ${(e as Error).message}`);
            throw e;
        }
    }

    async openRecordFromGridWithDbClick(recordName: string): Promise<void> {
        try {
            let locator = "//*[text()='" + recordName + "']";
            await expect(this.page.locator(locator).first()).toBeEnabled();
            await this.page.locator(locator).first().dblclick();

        } catch (e) {
            console.log(`Error while opening record : ${(e as Error).message}`);
            throw e;
        }
    }

    async addProductTemplateLines(recordName: string, type: string, includeByDefault: string, typeValue: string): Promise<void> {
        try {
            await this.openRecordFromGrid(recordName);

            await this.webHelper.clickOnTab(Products.Tabs.TemplateLines);
            await this.webHelper.clickOnCommandBarBtn(Products.Buttons.NewProductTemplateLine);


            await this.webHelper.selectOptionSet(Products.AriaLabel.Type, type);

            await this.webHelper.selectOptionSet(Products.AriaLabel.IncludeByDefault, includeByDefault);

            if (type == 'Module') {
                await this.webHelper.selectLookupByAriaLabel(ConfigQuestions.AriaLabel.ModuleLookup, typeValue);
            } else if (type == 'Question') {
                await this.webHelper.selectLookupByAriaLabel(Products.AriaLabel.QuestionLookUp, typeValue);
            }


            await this.webHelper.saveAndCloseQuickCreateRecord();


            await this.openRecordFromGrid(typeValue);
            await this.webHelper.updateStatusFromDraftToActive();
            await this.webHelper.saveAndCloseRecord();
            console.log(`Added question/module to product template: ${typeValue}`);
        } catch (e) {
            console.log(`Error while creating a products record : ${(e as Error).message}`);
            throw e;
        }
    }

    async validateProductConfigDisplayRuleFormFields(): Promise<void> {
        try {
            await this.webHelper.validateLockedField(Products.AriaLabel.LockedProductConfigQuestion);
            await this.webHelper.validateLockedField(Products.AriaLabel.LockedRuleConfigQuestion);
            await this.webHelper.validateFieldEnabled(Products.AriaLabel.Type);
            await this.webHelper.validateEditableField(Products.AriaLabel.RuleConfigAnswerLookup);
            await this.webHelper.validateFieldEnabled(Products.AriaLabel.DisplaySettings);
            await this.webHelper.validateEditableField(Products.AriaLabel.ImpactedConfigQuestionLookup);
        } catch (e) {
            console.log(`Error while validating fields : ${(e as Error).message}`);
            throw e;
        }
    }

    async compareConfigAnswers(expectedAnswers: string[]): Promise<void> {
        const configAnswers = ConfigQuestions.CSS.ConfigAnswers;
        try {
            await expect(this.page.locator(configAnswers).first()).toBeVisible();
            const element = this.page.locator(configAnswers);
            const actuaAnswers = await element.allTextContents();
            for (const answer of expectedAnswers) {
                expect(actuaAnswers).toContain(answer);
            }
        } catch (e) {
            console.log(`Error in comparing config answers : ${(e as Error).message}`);
            throw e;
        }
    }

    async verifyPopupMessageForDeactivateRecords(action: string): Promise<void> {
        const message1 = Products.AriaLabel.DeactivationMessage1;
        const message2 = Products.AriaLabel.DeactivationMessage2;

        try {
            await this.webHelper.verifyTheLabelValue(message1);
            await this.webHelper.verifyTheLabelValue(message2);
            await this.page.getByRole('button', { name: action }).click();

        } catch (e) {
            console.log(`Error in validating the Popup Message while deactivating : ${(e as Error).message}`);
            throw e;
        }
    }
}