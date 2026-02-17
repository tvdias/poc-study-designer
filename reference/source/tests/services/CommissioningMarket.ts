import { ConfigQuestions } from '../selectors/ConfigQuestionsSelector.json'
import { Common } from '../selectors/CommonSelector.json'
import { WebHelper } from '../utils/WebHelper';
import { Page, expect } from '@playwright/test';
import { waitUntilAppIdle } from '../utils/Login';
import { DropDownList } from '../constants/DropDownList.json';


export class CommissioningMarket {
    protected page: Page;
    private webHelper: WebHelper;

    constructor(page: Page) {
        this.page = page;
        this.webHelper = new WebHelper(this.page);
    }

    async fillTheName(name: string): Promise<void> {
        try {
            await this.webHelper.verifySaveButton();
            await this.webHelper.enterTextByRoleTextbox(ConfigQuestions.AriaLabel.Name, name);
            await this.webHelper.saveRecord();

        } catch (e) {
            console.log(`Error while creating a Commissioning Market : ${(e as Error).message}`);
            throw e;
        }
    }
    async getTheName(): Promise<string> {
        try {
            await this.webHelper.verifySaveButton();
            const name = await this.webHelper.getTextValue(ConfigQuestions.AriaLabel.Name);
            return name;

        } catch (e) {
            console.log(`Error while getting the Commissioning Market name : ${(e as Error).message}`);
            throw e;
        }
    }


    async verifyTheNameFieldIsReadOnly(): Promise<void> {
        try {
            await this.webHelper.VerifyInputFieldsReadonly(ConfigQuestions.AriaLabel.Name);

        } catch (e) {
            console.log(`Error while verifying the Name field : ${(e as Error).message}`);
            throw e;
        }
    }
    async searchForData(data: string): Promise<void> {
        try {
            await this.webHelper.enterTextByPlaceHolder(Common.Placeholder.AskAboutData, data);
            await this.webHelper.clickonLinkText(data);
        } catch (e) {
            console.log(`Error while Opening the a record : ${(e as Error).message}`);
            throw e;
        }
    }
}