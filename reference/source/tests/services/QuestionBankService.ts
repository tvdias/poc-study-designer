import { QuestionBank } from '../selectors/QuestionBankSelectors.json';
import { Common } from '../selectors/CommonSelector.json';
import { WebHelper } from '../utils/WebHelper';
import { Page, expect } from '@playwright/test';
import { waitUntilAppIdle } from '../utils/Login';


export class QuestionBankservice {
    protected page: Page;
    private webHelper: WebHelper;

    constructor(page: Page) {
        this.page = page;
        this.webHelper = new WebHelper(this.page);
    }

    async fillMandatoryfieldsInQuestionBank(questionbankName: string, type: string, title: string, questiontext: string): Promise<void> {
        try {
            await this.webHelper.goToEntityAndClickNewButton(Common.Entity.QuestionBank);
            //await this.webHelper.verifyAcceptSuggestion();
            await this.webHelper.clickOnHideFormButton();
            await this.webHelper.validateMandatoryField(QuestionBank.Title.QuestionVaraiableName);
            await this.webHelper.clickOnHideFormButton();
            await this.webHelper.validateMandatoryField(QuestionBank.Title.QuestionTitle);
            await this.webHelper.enterTextByRoleTextbox(QuestionBank.AriaLabel.QuestionVariableName, questionbankName);
            await this.webHelper.selectOptionSet(QuestionBank.AriaLabel.QuestionType, type);
            await this.webHelper.enterTextByRoleTextbox(QuestionBank.AriaLabel.QuestionTitle, title);
            await this.webHelper.enterTextByRoleTextbox(QuestionBank.AriaLabel.QuestionText, questiontext);
            await this.page.waitForTimeout(2000);
            console.log(`Filled all Mandatory fields : ${questionbankName}`);
        } catch (e) {
            console.log(`Error while filling the mandatory fields in Question bank form : ${(e as Error).message}`);
            throw e;
        }
    }

    async fillOptionalfieldsInQuestionBank(rowOrder: string, colomnOrder: string, minlength: string, maxlength: string, formatdetails: string, rationale: string, scriptnotes: string, methodlogy: string, customnotes: string, singlemulticode: string): Promise<void> {
        try {

            await this.webHelper.validateOptionalField(QuestionBank.Title.RowSortOrder);
            await this.webHelper.selectOptionSet(QuestionBank.AriaLabel.RowSortOrder, rowOrder);
            await this.webHelper.validateOptionalField(QuestionBank.Title.ColumnSortOrder);
            await this.webHelper.selectOptionSet(QuestionBank.AriaLabel.ColumnSortOrder, colomnOrder);
            await this.webHelper.validateOptionalField(QuestionBank.Title.AnswerMin);
            await this.webHelper.validateEditableField(QuestionBank.AriaLabel.AnswerMinimum);
            await this.webHelper.enterTextByRoleTextbox(QuestionBank.AriaLabel.AnswerMinimum, minlength);
            await this.webHelper.validateOptionalField(QuestionBank.Title.AnswerMax);
            await this.webHelper.validateEditableField(QuestionBank.AriaLabel.AnswerMaxmum);
            await this.webHelper.enterTextByRoleTextbox(QuestionBank.AriaLabel.AnswerMaxmum, maxlength);
            await this.webHelper.validateOptionalField(QuestionBank.Title.QuestionFormatDetails);
            await this.webHelper.validateEditableField(QuestionBank.AriaLabel.QuestionFormatDetails);
            await this.webHelper.enterTextByRoleTextbox(QuestionBank.AriaLabel.QuestionFormatDetails, formatdetails);

            await this.webHelper.validateEditableField(QuestionBank.AriaLabel.QuestionRationale);
            await this.webHelper.enterTextByRoleTextbox(QuestionBank.AriaLabel.QuestionRationale, rationale);

            await this.webHelper.validateOptionalField(QuestionBank.Title.ScripterNotes);
            await this.webHelper.validateEditableField(QuestionBank.AriaLabel.ScripterNotes);
            await this.webHelper.enterTextByRoleTextbox(QuestionBank.AriaLabel.ScripterNotes, scriptnotes);

            await this.webHelper.validateOptionalField(QuestionBank.Title.CustomNotes);
            await this.webHelper.validateEditableField(QuestionBank.AriaLabel.CustomeNotes);
            await this.webHelper.enterTextByRoleTextbox(QuestionBank.AriaLabel.CustomeNotes, customnotes);

            await this.webHelper.validateOptionalField(QuestionBank.Title.SingleorMulticode);
            await this.webHelper.selectOptionSet(QuestionBank.AriaLabel.SingleorMulticode, singlemulticode);
            await this.webHelper.enterTextByRoleTextbox(QuestionBank.AriaLabel.Methodology, methodlogy)
            await this.webHelper.selectcheckbox(methodlogy)



            console.log(`Filled all optional fields : `);
        } catch (e) {
            console.log(`Error while filling the optional fields : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifySelectiveOptionalfieldsInQuestionBank(): Promise<void> {
        try {

            await this.webHelper.verifySwitchbutton(QuestionBank.AriaLabel.IsDummyQuestionFalse);
            await this.webHelper.validateEditableField(QuestionBank.AriaLabel.MetricGroupLookup);
            await this.webHelper.validateOptionalField(QuestionBank.Title.TableTitle);
            await this.webHelper.validateTextboxFieldVisible(QuestionBank.AriaLabel.ManagedListReferences);

            console.log(`verified all optional fields : `);
        } catch (e) {
            console.log(`Error while verifying the optional fields : ${(e as Error).message}`);
            throw e;
        }
    }
    async fillManagedListReferences(data: string): Promise<void> {
        try {
            await this.page.waitForTimeout(2000);
            await this.page.getByRole('textbox', { name: QuestionBank.AriaLabel.AnswerMinimum, exact: true }).focus();
            await this.page.getByRole('textbox', { name: QuestionBank.AriaLabel.ScripterNotes, exact: true }).focus();
            await this.page.getByRole('textbox', { name: QuestionBank.AriaLabel.ManagedListReferences, exact: true }).scrollIntoViewIfNeeded();
            await this.page.getByRole('textbox', { name: QuestionBank.AriaLabel.ManagedListReferences, exact: true }).focus();
            await this.webHelper.enterTextByRoleTextbox(QuestionBank.AriaLabel.ManagedListReferences, data)

            console.log(`Filled the data in Managed List References: `);
        } catch (e) {
            console.log(`Error while filling the  Managed List References : ${(e as Error).message}`);
            throw e;
        }
    }

    async clickNewQuestionAnswer(): Promise<void> {
        try {
            await this.webHelper.clickOnTab(QuestionBank.Tabs.Answers);
            await this.webHelper.closeAIAlerts();
            await this.webHelper.clickonButtonByLabel(QuestionBank.Buttons.NewQuestionAnswerList);

            console.log(`Clicked on New Question Answers List button`);
        } catch (e) {
            console.log(`Error while select the New Question Answers List button : ${(e as Error).message}`);
            throw e;
        }
    }

    async changeStatusReason(): Promise<void> {
        try {

            await this.webHelper.updateStatusReasonFromDraftToActive();
            await this.webHelper.clickOnButton(QuestionBank.Buttons.Yes);
            await this.webHelper.saveRecord();
            console.log(`Changed the Status from Draft to Active`);

        } catch (e) {
            console.log(`Error while update the status from Draft to Active : ${(e as Error).message}`);
            throw e;
        }
    }

    async FillandCreateQuestionAnswsersList(text: string, code: string, location: string, option: string, property: string, version: string): Promise<void> {
        try {
            await this.webHelper.validateMandatoryField(QuestionBank.Title.AnswerText)
            await this.webHelper.validateMandatoryField(QuestionBank.Title.AnswerCode)
            await this.page.waitForTimeout(1000);
            await this.webHelper.enterTextByRoleTextbox(QuestionBank.AriaLabel.AnswerText, text);
            await this.webHelper.enterTextByRoleTextbox(QuestionBank.AriaLabel.AnswerCode, code);
            await this.webHelper.validateOptionalField(QuestionBank.Title.AnswerLocation)
            await this.webHelper.selectOptionSet(QuestionBank.AriaLabel.AnswerLocation, location);

            await this.webHelper.validateRequiredField(QuestionBank.Title.IsOpen)
            await this.webHelper.selectOptionSet(QuestionBank.AriaLabel.IsOpen, option);
            await this.webHelper.validateRequiredField(QuestionBank.Title.IsFixed)
            await this.webHelper.selectOptionSet(QuestionBank.AriaLabel.IsFixed, option);

            await this.webHelper.validateRequiredField(QuestionBank.Title.IsExclusive)
            await this.webHelper.selectOptionSet(QuestionBank.AriaLabel.IsExclusive, option);
            await this.webHelper.validateRequiredField(QuestionBank.Title.IsActive)
            await this.webHelper.selectOptionSet(QuestionBank.AriaLabel.IsActive, option);

            await this.webHelper.enterTextByRoleTextbox(QuestionBank.AriaLabel.CustomProperty, property);
            await this.webHelper.enterTextByRoleTextbox(QuestionBank.AriaLabel.Version, version);
            await this.webHelper.clickonButtonByLabel(QuestionBank.AriaLabel.SaveandClose);
            console.log(`Question Answer List is created :`);
        } catch (e) {
            console.log(`Error while creating a Question Answer List record : ${(e as Error).message}`);
            throw e;
        }
    }
    async FillAdminFields(isTranslatable: string, option: string, minlength: string, maxlength: string, property: string, sortOrder: string): Promise<void> {
        try {
            await this.webHelper.selectOptionSet(QuestionBank.AriaLabel.IsTranslatable, isTranslatable);

            await this.webHelper.selectOptionSet(QuestionBank.AriaLabel.IsHidden, isTranslatable);
            await this.webHelper.selectOptionSet(QuestionBank.AriaLabel.IsQuestionActive, option);
            await this.webHelper.selectOptionSet(QuestionBank.AriaLabel.IsQuestionOutofUse, isTranslatable);

            await this.webHelper.enterTextByRoleTextbox(QuestionBank.AriaLabel.AnswerRestrictionMin, minlength);
            await this.webHelper.enterTextByRoleTextbox(QuestionBank.AriaLabel.AnswerRestrictionMax, maxlength);
            await this.webHelper.enterTextByRoleTextbox(QuestionBank.AriaLabel.RestrictionDataType, property);
            await this.webHelper.enterTextByRoleTextbox(QuestionBank.AriaLabel.RestrictedToClient, property);
            console.log(`Fields are filled in Question Admin Form:`);

        } catch (e) {
            console.log(`Error while Filling the data in Question Admin Form : ${(e as Error).message}`);
            throw e;
        }
    }
    async FillAnswerFields(sortOrder: string, isTranslatable: string, id: string, sourceName: string, effectiveDate: string, endDate: string): Promise<void> {
        const answerText = QuestionBank.CSS.AnswerText;
        try {
            await this.page.locator(answerText).first().waitFor();
            await this.page.locator(answerText).first().click();

            await this.webHelper.enterTextByRoleTextbox(QuestionBank.AriaLabel.DisplayOrder, sortOrder);
            await this.webHelper.selectOptionSet(QuestionBank.AriaLabel.IsTranslatable, isTranslatable);
            await this.webHelper.enterTextByRoleTextbox(QuestionBank.AriaLabel.SourceId, id);
            await this.webHelper.enterTextByRoleTextbox(QuestionBank.AriaLabel.SourceName, sourceName);
            await this.webHelper.enterDate(QuestionBank.AriaLabel.DateofEffectiveDate, effectiveDate);
            await this.webHelper.enterDate(QuestionBank.AriaLabel.DateofEndDate, endDate);

            console.log(`Fields are filled in Question Admin Form:`);
        } catch (e) {
            console.log(`Error while Filling the data in Question Admin Form : ${(e as Error).message}`);
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
    async searchDraftQuestionBank(questionName: string): Promise<void> {
        try {
            await this.page.waitForTimeout(2000); //wait for save to complete
            await this.webHelper.clickonContinueAnyway();
            if (!await this.webHelper.isElementTextIsVisible(Common.Text.Saved)) {
                await this.webHelper.switchToAllQuestionBanks();
                await this.webHelper.searchAndOpenRecord(questionName);
                await this.webHelper.verifyText(Common.Text.Saved);
            }
        } catch (e) {
            console.log(`Error while selecting the Question Name : ${(e as Error).message}`);
            throw e;
        }
    }
    async selectQuestionBank(questionName: string): Promise<void> {
        try {
            await this.webHelper.clickonContinueAnyway();
            if (!await this.webHelper.isElementTextIsVisible(Common.Text.Saved)) {
                await this.page.getByText(questionName, { exact: true }).first().click();
                await this.webHelper.verifyText(Common.Text.Saved);
            }
        } catch (e) {
            console.log(`Error while selecting the Question Name : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyNewVersion(questionName: string): Promise<void> {
        try {
          await this.page.waitForTimeout(3000);  
          const title=  await this.webHelper.getHeaderTitle();
          await expect(title).toContain(questionName+" - V52");

        } catch (e) {
            console.log(`Error while validating the new version of Question : ${(e as Error).message}`);
            throw e;
        }
    }
}