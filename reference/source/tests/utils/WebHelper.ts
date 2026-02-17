import { expect, Page } from '@playwright/test';
import { Common } from '../selectors/CommonSelector.json'
import { DropDownList } from '../constants/DropDownList.json';
import { waitUntilAppIdle } from './Login';
import { ProjectService } from '../services/ProjectService';

export class WebHelper {
    protected page: Page;

    constructor(page: Page) {
        this.page = page;
    }

    async enterTextByLabel(ariaLabel: string, text: string): Promise<void> {
        try {
            await expect(this.page.getByLabel(ariaLabel).first()).toBeVisible();
            await this.page.getByLabel(ariaLabel).first().click();
            await this.page.getByLabel(ariaLabel).first().clear();
            await this.page.getByLabel(ariaLabel).first().fill(text);
        } catch (e) {
            console.log(`Error in entering text: ${(e as Error).message}`);
            throw e;
        }
    }
    async VerifyInputFieldsReadonly(ariaLabel: string): Promise<void> {
        try {
            await expect(this.page.getByLabel(ariaLabel).first()).toBeVisible();
            const hasAttr = await this.page.getByLabel(ariaLabel).evaluate(el => el.hasAttribute('readonly'));
            await expect(hasAttr).toBeTruthy();
        } catch (e) {
            console.log(`Error while  validating the Readonly fields ${(e as Error).message}`);
            throw e;
        }
    }

    async verifyFieldReadonly(locator: string): Promise<void> {
        try {
            await this.page.getByRole('textbox', { name: locator, exact: true }).waitFor({ state: "visible" });
            await expect(this.page.getByRole('textbox', { name: locator, exact: true })).not.toBeEditable();
        } catch (e) {
            console.log(`Error while  validating the Readonly field ${(e as Error).message}`);
            throw e;
        }
    }

    async verifyFieldDisabled(locator: string): Promise<void> {
        try {
            await this.page.locator(locator).waitFor({ state: "visible" });
            await expect(this.page.locator(locator).isDisabled()).toBeTruthy();
        } catch (e) {
            console.log(`Error while  validating the Disabled field ${(e as Error).message}`);
            throw e;
        }
    }

    async enterTextByPlaceHolder(placeholder: string, text: string): Promise<void> {
        try {

            await expect(this.page.getByPlaceholder(placeholder).first()).toBeVisible();
            await this.page.getByPlaceholder(placeholder).first().click();
            await this.page.getByPlaceholder(placeholder).first().clear();
            await this.page.getByPlaceholder(placeholder).first().fill(text);
            await this.page.getByPlaceholder(placeholder).first().press('Enter');


        } catch (e) {
            console.log(`Error in entering text: ${(e as Error).message}`);
            throw e;
        }
    }
    async enterTextByRoleTextbox(locator: string, text: string): Promise<void> {
        try {
            await this.closeAIForm();
            await this.closeAIAlerts();
            await this.page.getByRole('textbox', { name: locator, exact: true }).waitFor({ state: "visible" });
            await this.page.getByRole('textbox', { name: locator, exact: true }).scrollIntoViewIfNeeded();
            await this.page.getByRole('textbox', { name: locator, exact: true }).focus();
            await expect(this.page.getByRole('textbox', { name: locator, exact: true })).toBeEditable();
            await this.page.getByRole('textbox', { name: locator, exact: true }).focus();
            await this.page.getByRole('textbox', { name: locator, exact: true }).click();
            await this.page.getByRole('textbox', { name: locator, exact: true }).clear();

            await this.page.getByRole('textbox', { name: locator, exact: true }).fill(text);

        } catch (e) {
            console.log(`Error in entering text: ${(e as Error).message}`);
            throw e;
        }
    }
    async enterTextByTitleTextbox(title: string, text: string): Promise<void> {
        try {
            await this.page.getByTitle(title).waitFor({ state: "visible" });
            await expect(this.page.getByTitle(title)).toBeVisible();
            await this.page.getByTitle(title).dblclick();
            await this.page.getByTitle(title).click();
            await this.page.getByTitle(title).clear();
            await this.page.getByTitle(title).fill(text);

        } catch (e) {
            console.log(`Error in entering text: ${(e as Error).message}`);
            throw e;
        }
    }
    async performDragAndDrop(btn: string): Promise<void> {
        try {
            await this.page.getByRole("button", { name: btn, exact: true }).first().waitFor();
            await this.page.getByRole("button", { name: btn, exact: true }).first().focus();
            const src = await this.page.getByRole("button", { name: btn, exact: true }).first();
            const dest = await this.page.getByRole("button", { name: btn, exact: true }).nth(1);

            await src.dragTo(dest); // Dragging  & dropping 

            await this.page.waitForTimeout(2000);

        } catch (e) {
            console.log(`Error while dragging and dropping ${(e as Error).message}`);
            throw e;
        }
    }

    async clickOnCommandBarBtn(buttonName: string): Promise<void> {
        try {
            await this.page.getByRole('menuitem', { name: buttonName, exact: true }).first().waitFor({ state: "visible" });
            await this.page.getByRole('menuitem', { name: buttonName, exact: true }).first().waitFor({ state: "attached" });
            await expect(this.page.getByRole('menuitem', { name: buttonName, exact: true }).first()).toBeVisible();
            await expect(this.page.getByRole('menuitem', { name: buttonName, exact: true }).first()).toBeEnabled();
            await this.page.getByRole('menuitem', { name: buttonName, exact: true }).first().focus();
            await this.page.getByRole('menuitem', { name: buttonName, exact: true }).first().click();

        } catch (e) {
            console.log(`Error in clicking commandbar button : ${(e as Error).message}`);
            throw e;
        }
    }

    async validateCommandBarBtnNotVisible(buttonName: string, buttonName1: string): Promise<void> {
        try {
            await this.clickOnCommandBarBtn(buttonName1);
            await expect(this.page.getByRole('menuitem', { name: buttonName, exact: true }).first()).not.toBeVisible();
        } catch (e) {
            console.log(`Error in validating commandbar button display : ${(e as Error).message}`);
            throw e;
        }
    }

    async clickByAriaLabel(ariaLabel: string): Promise<void> {
        try {
            await this.page.getByLabel(ariaLabel, { exact: true }).first().waitFor({ state: "visible" });
            await expect(this.page.getByLabel(ariaLabel, { exact: true }).first()).toBeVisible();
            await this.page.waitForTimeout(2000);
            await this.page.getByLabel(ariaLabel, { exact: true }).first().click();

        } catch (e) {
            console.log(`Error in clicking commandbar button : ${(e as Error).message}`);
            throw e;
        }
    }

    async goToEntity(entity: string): Promise<void> {
        try {
            await expect(this.page.getByText(entity, { exact: true }).first()).toBeVisible();
            await this.page.getByText(entity, { exact: true }).first().click();

        } catch (e) {
            console.log(`Error in clicking on entity : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyTheEntity(entity: string, isVisible: boolean = true): Promise<void> {
        try {
            if (isVisible) {
                await this.page.getByText(entity, { exact: true }).first().waitFor();
                await expect(this.page.getByText(entity, { exact: true }).first()).toBeVisible();
            }
            else
                await expect(this.page.getByText(entity, { exact: true }).first()).toBeHidden();

        } catch (e) {
            console.log(`Error in verify the text : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyTheQuestionName(name: string): Promise<void> {
        try {
            await expect(this.page.locator('#fui-Tagrj').getByText(`${name}`)).toBeVisible();

        } catch (e) {
            console.log(`Error in verify the text for the QuestionName : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyTheParagraphText(name: string): Promise<void> {
        try {
            await expect(this.page.getByRole('paragraph').filter({ hasText: `${name}` }).first()).toBeVisible();

        } catch (e) {
            console.log(`Error in verify the Paragraph text : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyText(name: string, isVisible: boolean = true): Promise<void> {
        try {
            if (isVisible)
                await expect(this.page.getByText(name).first()).toBeVisible();
            else
                await expect(this.page.getByText(name)).toBeHidden();
        } catch (e) {
            console.log(`Error in verify the text : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyTheCellText(text: string, isVisible: boolean = true): Promise<void> {
        try {
            if (isVisible) {
                await expect(this.page.getByRole("cell", { name: text, exact: true }).first()).toBeVisible();
            }
            else {
                await expect(this.page.getByRole("cell", { name: text, exact: true }).first()).toBeHidden();
            }

        } catch (e) {
            console.log(`Error in verify the Cell text : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyTheSpantext(entity: string, isVisible: boolean = true): Promise<void> {
        try {
            if (isVisible) {
                await this.page.locator("span", { hasText: `${entity}` }).last().waitFor({ state: "visible" });
                await expect(this.page.locator("span", { hasText: `${entity}` }).last()).toBeVisible();
            }
            else
                await expect(this.page.locator("span", { hasText: `${entity}` }).last()).toBeHidden();

        } catch (e) {
            console.log(`Error in verify the text : ${(e as Error).message}`);
            throw e;
        }
    }
    async getCountofQuestionsRemoveModule(): Promise<number> {
        const questions = Common.CSS.QuestionsList;
        try {
            const count = await this.page.locator(questions).count();
            return count;

        } catch (e) {
            console.log(`Error in get count of the questions : ${(e as Error).message}`);
            throw e;
        }
    }
    async clickOnSpantext(entity: string): Promise<void> {
        try {

            await this.page.locator("span", { hasText: `${entity}` }).last().waitFor({ state: "visible" });
            await expect(this.page.locator("span", { hasText: `${entity}` }).last()).toBeVisible();
            await this.page.locator("span", { hasText: `${entity}` }).last().click();

        } catch (e) {
            console.log(`Error in Click on the text : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyTheLabeltext(entity: string, isVisible: boolean = true): Promise<void> {
        try {
            if (isVisible)
                await expect(this.page.locator("label", { hasText: `${entity}` }).first()).toBeVisible();
            else
                await expect(this.page.locator("label", { hasText: `${entity}` }).first()).toBeHidden();

        } catch (e) {
            console.log(`Error in verify the text : ${(e as Error).message}`);
            throw e;
        }
    }
    async doubleClickOnLabel(text: string): Promise<void> {
        try {
            await expect(this.page.getByLabel(text, { exact: true }).first()).toBeVisible();
            await this.page.getByLabel(text, { exact: true }).first().dblclick();

        } catch (e) {
            console.log(`Error in clicking on Label : ${(e as Error).message}`);
            throw e;
        }
    }

    async saveRecord(): Promise<void> {
        const save = Common.Text.Save;
        const saveMenuItem = this.page.getByRole('menuitem', { name: 'save' }).first();
        try {
            await saveMenuItem.waitFor({ state: 'visible' });
            await saveMenuItem.waitFor({ state: 'attached' });
            await expect(saveMenuItem).toBeEnabled();
            await saveMenuItem.click();


        } catch (e) {
            console.log(`Error in saving record : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifySaveButton(): Promise<void> {
        const save = Common.Text.Save;
        const saveMenuItem = this.page.getByRole('menuitem', { name: save }).first();
        try {
            await saveMenuItem.waitFor({ state: 'visible' });
            await expect(saveMenuItem).toBeEnabled();
            await expect(saveMenuItem).toBeVisible();

        } catch (e) {
            console.log(`Error in verifying the Save button : ${(e as Error).message}`);
            throw e;
        }
    }

    async saveAndCloseRecord(): Promise<void> {
        const saveAndClose = Common.Text.SaveAndClose;
        try {
            await this.page.getByRole('menuitem', { name: saveAndClose }).first().waitFor();
            await expect(this.page.getByRole('menuitem', { name: saveAndClose }).first()).toBeVisible();
            await this.page.getByRole('menuitem', { name: saveAndClose }).first().click();

        } catch (e) {
            console.log(`Error in saving & closing record : ${(e as Error).message}`);
            throw e;
        }
    }

    async saveAndCloseQuickCreateRecord(): Promise<void> {
        const quickCreateSaveAndClose = Common.Text.QCSaveAndClose;
        try {
            await expect(this.page.getByRole('button', { name: quickCreateSaveAndClose }).first()).toBeVisible();
            await this.page.getByRole('button', { name: quickCreateSaveAndClose }).first().click();

        } catch (e) {
            console.log(`Error in saving & closing record : ${(e as Error).message}`);
            throw e;
        }
    }

    async selectLookupByAriaLabel(ariaLabel: string, text: string): Promise<void> {
        try {
            await this.closeAIAlerts();
            await this.closeAIForm();
            await expect(this.page.getByLabel(ariaLabel, { exact: true }).first()).toBeVisible();
            await this.page.getByLabel(ariaLabel, { exact: true }).first().waitFor({ state: "visible" });
            await this.page.getByLabel(ariaLabel, { exact: true }).first().click();
            await this.page.getByLabel(ariaLabel, { exact: true }).first().fill(text);
            await this.page.getByText(text).first().waitFor({ state: "visible" });
            await this.page.waitForTimeout(2000);
            await this.page.getByText(text).first().click();

            await this.page.waitForTimeout(2000);
        } catch (e) {
            console.log(`Error while selecting lookup value : ${(e as Error).message}`);
            throw e;
        }
    }

    async selectOptionSet(ariaLabel: string, option: string): Promise<void> {
        try {
            await this.page.getByRole('combobox', { name: ariaLabel }).first().waitFor({ state: "visible" });
            await expect(this.page.getByRole('combobox', { name: ariaLabel }).first()).toBeVisible();
            await this.page.waitForTimeout(1000);
            await this.page.getByRole('combobox', { name: ariaLabel }).first().click();
            await this.page.getByRole('option', { name: option, exact: true }).first().waitFor({ state: "visible" });
            await expect(this.page.getByRole('option', { name: option, exact: true }).first()).toBeVisible();
            await this.page.getByRole('option', { name: option, exact: true }).first().click();
        } catch (e) {
            console.log(`Error while selecting optionset value : ${(e as Error).message}`);
            throw e;
        }
    }

    async selectByOption(locator: string, option: string): Promise<void> {

        try {
            await expect(this.page.locator(locator).first()).toBeVisible();
            await this.page.selectOption(locator, option);

        } catch (e) {
            console.log(`Error while selecting option : ${(e as Error).message}`);
            throw e;
        }
    }
    async autoSelectOptionSet(ariaLabel: string, option: string): Promise<void> {
        const btnName = Common.AriaLabel.AcceptSuggestion;
        try {
            await this.page.getByRole('combobox', { name: ariaLabel }).first().waitFor({ state: "visible" });
            await expect(this.page.getByRole('combobox', { name: ariaLabel }).first()).toBeVisible();
            await this.page.getByRole('combobox', { name: ariaLabel }).first().isVisible()
            await this.page.getByRole('combobox', { name: ariaLabel }).first().hover();
            if (await this.page.getByRole('button', { name: btnName }).first().isVisible()) {
                await expect(this.page.getByRole('button', { name: btnName }).first()).toBeVisible();
                await this.page.getByRole('button', { name: btnName }).first().waitFor({ state: "visible" });
                await this.page.getByRole('button', { name: btnName }).first().click();
            }
            else {
                await this.selectOptionSet(ariaLabel, option);
            }
        } catch (e) {
            console.log(`Error while auto selecting the optionset value : ${(e as Error).message}`);
            throw e;
        }
    }
    async updateStatusFromDraftToActive(): Promise<void> {
        const moreHeaderEditableFields = Common.ByRole.MoreHeaderEditableFields;
        const statusReason = Common.ByRole.StatusReason;
        try {
            await expect(this.page.getByRole('button', { name: moreHeaderEditableFields })).toBeVisible();
            await this.page.getByRole('button', { name: moreHeaderEditableFields }).click();

            await expect(this.page.getByRole('combobox', { name: statusReason })).toBeVisible();
            await this.page.getByRole('combobox', { name: statusReason }).click();
            await this.page.getByRole('option', { name: 'Active' }).click();

            await this.saveRecord();

        } catch (e) {
            console.log(`Error while selecting optionset value : ${(e as Error).message}`);
            throw e;
        }
    }

    async enterDate(locator: string, date: string): Promise<void> {
        try {
            await this.page.getByRole('combobox', { name: locator, exact: true }).waitFor({ state: "visible" });
            await this.page.getByRole('combobox', { name: locator, exact: true }).scrollIntoViewIfNeeded();
            await this.page.getByRole('combobox', { name: locator, exact: true }).focus();
            await expect(this.page.getByRole('combobox', { name: locator, exact: true })).toBeEditable();
            await this.page.getByRole('combobox', { name: locator, exact: true }).focus();
            await this.page.getByRole('combobox', { name: locator, exact: true }).fill(date);

        } catch (e) {
            console.log(`Error while Entering Date  : ${(e as Error).message}`);
            throw e;
        }
    }
    async updateStatusReasonFromDraftToActive(): Promise<void> {
        const statusReason = Common.ByRole.StatusReason;
        try {

            await expect(this.page.getByRole('combobox', { name: statusReason })).toBeVisible();
            await this.page.getByRole('combobox', { name: statusReason }).click();
            await this.page.getByRole('option', { name: "Active" }).click();


        } catch (e) {
            console.log(`Error while selecting optionset value : ${(e as Error).message}`);
            throw e;
        }
    }

    async validateSubGridFirstRecordIsDisplayed(viewName: string): Promise<void> {
        try {
            let baseLocator = "'] [aria-label='Press SPACE to select this row.']";
            const finalLocator = "[aria-label='" + viewName + baseLocator;
            await expect(this.page.locator(finalLocator).first()).toBeVisible();
        } catch (e) {
            console.log(`Error while validating record in subgrid : ${(e as Error).message}`);
            throw e;
        }
    }

    async changeArea(area: string): Promise<void> {
        const changeArea = Common.CSS.ChangeArea;
        try {
            await expect(this.page.locator(changeArea)).toBeVisible();
            await this.page.locator(changeArea).click();
            await this.page.getByLabel('Change area', { exact: true }).getByText(area).click();

        } catch (e) {
            console.log(`Error while changing area : ${(e as Error).message}`);
            throw e;
        }
    }

    async searchAndDeleteRecord(record: string): Promise<void> {
        const filterbykeyword = Common.Placeholder.Filterbykeyword;
        const searchTextbox = Common.ByRole.SearchTextbox;
        const deleteBtn = Common.Text.Delete;
        try {
            await this.page.getByPlaceholder(filterbykeyword, { exact: true }).first().click();
            await this.page.getByRole('searchbox', { name: searchTextbox }).fill(record);
            await this.page.getByRole('searchbox', { name: searchTextbox }).press('Enter');

            await this.page.getByRole('gridcell', { name: searchTextbox }).locator('div').nth(1).click();

            await this.page.getByRole('button', { name: searchTextbox, exact: true }).click();

            await this.page.getByRole('button', { name: deleteBtn }).click();

        } catch (e) {
            console.log(`Error while deleting record : ${(e as Error).message}`);
            throw e;
        }
    }

    async searchAndOpenRecord(record: string): Promise<void> {
        const filterbykeyword = Common.Placeholder.Filterbykeyword;
        const searchTextbox = Common.ByRole.SearchTextbox;
        try {
            await this.page.getByPlaceholder(filterbykeyword, { exact: true }).first().waitFor({ state: "visible" });
            await this.page.getByPlaceholder(filterbykeyword, { exact: true }).first().click();
            await this.page.getByRole('searchbox', { name: searchTextbox }).fill(record);
            await this.page.getByRole('searchbox', { name: searchTextbox }).press('Enter');
            await this.page.getByText(record, { exact: true }).first().waitFor();
            await expect(this.page.getByText(record, { exact: true }).first()).toBeVisible();
            await this.page.getByText(record, { exact: true }).first().click();

        } catch (e) {
            console.log(`Error while opening record : ${(e as Error).message}`);
            throw e;
        }
    }

    async searchrecordInQuestionnaire(record: string): Promise<void> {
        const filterbykeyword = Common.Placeholder.Search;
        try {
            await this.page.getByPlaceholder(filterbykeyword, { exact: true }).first().waitFor({ state: "visible" });
            await this.page.getByPlaceholder(filterbykeyword, { exact: true }).first().click();
            await this.page.getByPlaceholder(filterbykeyword, { exact: true }).first().clear();
            await this.page.getByRole('searchbox', { name: filterbykeyword }).fill(record);
            await this.page.getByRole('searchbox', { name: filterbykeyword }).press('Enter');
        } catch (e) {
            console.log(`Error while searching record : ${(e as Error).message}`);
            throw e;
        }
    }
    async goToEntityAndClickNewButton(entity: string): Promise<void> {
        const newBtn = Common.Text.New;
        try {
            await this.goToEntity(entity);

            await this.clickOnCommandBarBtn(newBtn);

        } catch (e) {
            console.log(`Error in navigating to entity : ${(e as Error).message}`);
            throw e;
        }
    }

    async clickOnTab(tabName: string): Promise<void> {
        const tab = await this.page.getByRole('tab', { name: tabName }).first();
        try {
            await tab.waitFor({ state: 'visible' });
            await expect(tab).toBeVisible();
            await expect(tab).toBeEnabled();
            await this.page.getByRole('tab', { name: tabName }).click();

        } catch (e) {
            console.log(`Error while clicking on tab : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyTheTab(tabName: string): Promise<void> {
        const tab = await this.page.getByRole('tab', { name: tabName }).first();
        try {
            console.log(tab.textContent());
            await tab.waitFor({ state: 'visible' });
            await expect(tab).toBeVisible();
            await expect(tab).toBeEnabled();

        } catch (e) {
            console.log(`Error while verifying on tab : ${(e as Error).message}`);
            throw e;
        }
    }
    async switchToAllQuestionBanks(): Promise<void> {
        try {
            const activeqn = await this.page.getByRole('button', { name: Common.AriaLabel.ActiveQuestionBank }).first();

            await activeqn.waitFor({ state: 'visible' });
            await expect(activeqn).toBeVisible();
            await activeqn.click();
            await this.page.getByRole("menuitemradio", { name: "All Question Banks" }).waitFor({ state: "visible" });
            await this.page.getByRole("menuitemradio", { name: "All Question Banks" }).click();

        } catch (e) {
            console.log(`Error while clicking on tab : ${(e as Error).message}`);
            throw e;
        }
    }

    async getTextValue(textbox: string): Promise<string> {
        try {

            await expect(this.page.getByRole('textbox', { name: textbox })).toBeVisible();
            const text = await this.page.getByRole('textbox', { name: textbox }).inputValue();
            return text;
        } catch (e) {
            console.log(`Error while clicking on tab : ${(e as Error).message}`);
            throw e;
        }
    }
    async clickOnButton(buttonName: string): Promise<void> {
        try {
            await expect(this.page.getByRole('button', { name: buttonName }).first()).toBeVisible();
            await this.page.getByRole('button', { name: buttonName }).first().click();

        } catch (e) {
            console.log(`Error while clicking on button : ${(e as Error).message}`);
            throw e;
        }
    }
    async clickOnHideFormButton(): Promise<void> {
        const hideAssistBtn = Common.CSS.HideAssist;
        try {
            await this.page.waitForTimeout(3000);
            if (await this.page.locator(hideAssistBtn).isVisible()) {
                await this.page.locator(hideAssistBtn).focus();
                await this.page.locator(hideAssistBtn).waitFor();
                await expect(this.page.locator(hideAssistBtn)).toBeEnabled();
                await this.page.locator(hideAssistBtn).click();
                console.log("Clicked on Hide form fill assist toolbar button")
            }

        } catch (e) {
            console.log(`Error while clicking on Hide Form button : ${(e as Error).message}`);
            throw e;
        }
    }
    async clickOnButtonByDataTestID(buttonName: string): Promise<void> {
        try {
            await expect(this.page.getByTestId(buttonName)).toBeVisible();
            await this.page.getByTestId(buttonName).focus();
            await this.page.getByTestId(buttonName).click();

        } catch (e) {
            console.log(`Error while clicking on button : ${(e as Error).message}`);
            throw e;
        }
    }
    async validateStatusReasonInHeader(statusReason: string): Promise<void> {
        const statusReasonElement = Common.CSS.StatusReason;
        try {
            await expect(this.page.locator(statusReasonElement).first()).toBeVisible();
            await expect(this.page.locator(statusReasonElement).first()).toContainText(statusReason);
        } catch (e) {
            console.log(`Error while validating status reason : ${(e as Error).message}`);
            throw e;
        }
    }

    async validateMandatoryField(title: string): Promise<void> {
        try {
            const element = "[title='" + title + "'] > span";
            await expect(this.page.locator(element).first()).toBeVisible();
        } catch (e) {
            console.log(`Error while validating mandatory field : ${(e as Error).message}`);
            throw e;
        }
    }
    async validateRequiredField(title: string): Promise<void> {
        try {
            const element = "[title*='" + title + "'] > span";
            await expect(this.page.locator(element).first()).toBeVisible();
        } catch (e) {
            console.log(`Error while validating mandatory field : ${(e as Error).message}`);
            throw e;
        }
    }

    async validateOptionalField(title: string): Promise<void> {
        try {
            const element = "[title='" + title + "']";
            await this.page.locator(element).first().waitFor({ state: "visible" });
            await expect(this.page.locator(element).first()).toBeVisible();
        } catch (e) {
            console.log(`Error while validating optional field : ${(e as Error).message}`);
            throw e;
        }
    }

    async openRelatedEntity(entityName: string): Promise<void> {
        const related = Common.Tabs.Related;
        try {
            await this.page.waitForTimeout(4000);
            await this.clickOnTab(related);
            await this.clickByAriaLabel(entityName);
        } catch (e) {
            console.log(`Error while validating optional field : ${(e as Error).message}`);
            throw e;
        }
    }

    async deactivateActivateAllSubgridRecords(action: string): Promise<void> {
        const allRowCheckbox = Common.ByRole.AllRowCheckbox;
        try {
            await expect(this.page.locator(allRowCheckbox).first()).toBeVisible();
            await this.page.locator(allRowCheckbox).first().click();
            await expect(this.page.getByRole('menuitem', { name: action, exact: true }).nth(1)).toBeVisible();
            await this.page.getByRole('menuitem', { name: action, exact: true }).nth(1).click();

        } catch (e) {
            console.log(`Error in deleting records : ${(e as Error).message}`);
            throw e;
        }
    }


    async selectSubgridView(viewName: string): Promise<void> {
        const viewArrow = Common.CSS.ViewArrow;
        try {
            await expect(this.page.locator(viewArrow).first()).toBeVisible();
            await this.page.locator(viewArrow).first().click();

            await expect(this.page.getByText(viewName, { exact: true }).first()).toBeVisible();
            await this.page.getByText(viewName, { exact: true }).first().click();

        } catch (e) {
            console.log(`Error while selecting view : ${(e as Error).message}`);
            throw e;
        }
    }

    async assertRowsCount(noOfRows: number): Promise<void> {
        const rows = Common.CSS.Rows;
        try {
            await expect(this.page.locator(rows)).toHaveCount(noOfRows);

        } catch (e) {
            console.log(`Error while asserting no. of rows : ${(e as Error).message}`);
            throw e;
        }
    }
    async getCountOfColumns(column: string): Promise<number> {
        try {
            await this.page.locator(column).first().waitFor({ state: "visible" });
            const coloumnCount = await this.page.locator(column).count();
            return coloumnCount;
        } catch (e) {
            console.log(`Error while getting the Column Count : ${(e as Error).message}`);
            throw e;
        }
    }

    async clickGoBackArrow(): Promise<void> {
        const backArrow = Common.AriaLabel.BackArrow;
        try {
            await expect(this.page.getByRole("button", { name: backArrow, exact: true }).first()).toBeVisible();
            await this.page.getByRole("button", { name: backArrow, exact: true }).first().click();

        } catch (e) {
            console.log(`Error in clicking back icon : ${(e as Error).message}`);
            throw e;
        }
    }

    async clickQCCancel(): Promise<void> {
        const quickCreateCancel = Common.Text.QCCancel;
        try {
            await expect(this.page.getByRole('button', { name: quickCreateCancel }).first()).toBeVisible();
            await this.page.getByRole('button', { name: quickCreateCancel }).first().click();

        } catch (e) {
            console.log(`Error while clicking cancel button : ${(e as Error).message}`);
            throw e;
        }
    }
    async selectcheckbox(label: string): Promise<void> {
        try {
            await this.page.waitForTimeout(1000);
            const locator = await this.page.getByRole('checkbox', { name: label }).first();
            await expect(locator).toBeVisible();
            await locator.scrollIntoViewIfNeeded();
            if (! await locator.isChecked())
                await locator.click({ force: true });

        } catch (e) {
            console.log(`Error while clicking checkbox for ${label} : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyThecheckbox(label: string, isVisible: boolean = true): Promise<void> {
        try {

            if (isVisible) {
                await expect(this.page.getByRole('checkbox', { name: label, exact: true }).first()).toBeVisible();
                console.log(`${label} is displayed`)
            }
            else {
                await expect(this.page.getByRole('checkbox', { name: label, exact: true }).first()).toBeHidden();
                console.log(`${label} is not displayed`)
            }


        } catch (e) {
            console.log(`Error while Verifying checkbox for ${label} : ${(e as Error).message}`);
            throw e;
        }
    }
    async selectTheRow(): Promise<void> {
        const row = Common.AriaLabel.PressSPACEToSelect;
        try {

            await expect(this.page.getByRole('row', { name: row }).first()).toBeVisible();
            await this.page.getByRole('row', { name: row }).first().click();

        } catch (e) {
            console.log(`Error while clicking  Row} : ${(e as Error).message}`);
            throw e;
        }
    }
    async clickonButtonByLabel(buttonName: string): Promise<void> {

        try {
            await expect(this.page.getByLabel(buttonName).first()).toBeVisible();
            await this.page.getByLabel(buttonName).first().focus();
            await this.page.getByLabel(buttonName).first().click();


        } catch (e) {
            console.log(`Error while clicking ${buttonName} button : ${(e as Error).message}`);
            throw e;
        }
    }

    async discardChanges(): Promise<void> {
        const discardChanges = Common.Text.DiscardChanges;
        try {
            await this.page.getByRole('button', { name: discardChanges }).first().waitFor({ state: "visible" });
            await expect(this.page.getByRole('button', { name: discardChanges }).first()).toBeVisible();
            await this.page.getByRole('button', { name: discardChanges }).first().click();


        } catch (e) {
            console.log(`Error while discard changes : ${(e as Error).message}`);
            throw e;
        }
    }
    async clickonContinueAnyway(): Promise<void> {
        const continuebtn = Common.Text.ContinueAnyway;
        try {
            await this.page.waitForTimeout(2000);
            if (await this.page.getByRole('button', { name: continuebtn }).first().isVisible()) {
                await this.page.getByRole('button', { name: continuebtn }).first().waitFor({ state: "visible" });
                await expect(this.page.getByRole('button', { name: continuebtn }).first()).toBeVisible();
                await this.page.getByRole('button', { name: continuebtn }).first().click();
            }
        } catch (e) {
            console.log(`Error while click on Continue anyway button : ${(e as Error).message}`);
            throw e;
        }
    }

    async clickOnConfirmationPopup(yesOrNo: string): Promise<void> {
        try {
            await this.page.getByRole('button', { name: yesOrNo }).first().waitFor({ state: "visible", timeout: 40000 });
            await expect(this.page.getByRole('button', { name: yesOrNo }).first()).toBeVisible();
            await this.page.getByRole('button', { name: yesOrNo }).first().waitFor();
            await this.page.getByRole('button', { name: yesOrNo }).first().click();

        } catch (e) {
            console.log(`Error while confirming popup window : ${(e as Error).message}`);
            throw e;
        }
    }

    async validateLockedField(ariaLabel: string, isVisible: boolean = true): Promise<void> {
        try {
            if (isVisible) {
                await expect(this.page.getByLabel(ariaLabel, { exact: true }).first()).toBeVisible();
                console.log(`Field ${ariaLabel} is locked`);
            }
            else {
                await expect(this.page.getByLabel(ariaLabel, { exact: true }).first()).toBeHidden();
            }

        } catch (e) {
            console.log(`Error while validating the Locked field : ${(e as Error).message}`);
            throw e;
        }
    }

    async validateEditableField(ariaLabel: string): Promise<void> {
        try {
            await this.page.getByLabel(ariaLabel, { exact: true }).first().focus();
            await expect(this.page.getByLabel(ariaLabel, { exact: true }).first()).toBeEditable();
            console.log(`Field ${ariaLabel} is editable`);
        } catch (e) {
            console.log(`Error in validating editable fields : ${(e as Error).message}`);
            throw e;
        }
    }

    async validateTextboxFieldVisible(ariaLabel: string): Promise<void> {
        try {
            await this.page.getByRole('textbox', { name: ariaLabel, exact: true }).focus();
            await this.page.getByRole('textbox', { name: ariaLabel, exact: true }).waitFor();
            await expect(this.page.getByLabel(ariaLabel, { exact: true }).first()).toBeEditable();
            console.log(`Field ${ariaLabel} is editable`);
        } catch (e) {
            console.log(`Error in validating editable fields : ${(e as Error).message}`);
            throw e;
        }
    }

    async validateFieldEnabled(ariaLabel: string): Promise<void> {
        try {
            await expect(this.page.getByLabel(ariaLabel, { exact: true }).first()).toBeEnabled();
            console.log(`Field ${ariaLabel} is enabled`);
        } catch (e) {
            console.log(`Error in validating field : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyTheLabelValue(text: string): Promise<void> {
        const locator = `label[aria-label="${text}"]`;
        try {
            await expect(this.page.locator(locator)).toBeVisible();

        } catch (e) {
            console.log(`Error in verify the label text : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyTheGridLabelValue(text: string): Promise<void> {
        try {

            await expect(this.page.getByRole('row', { name: 'Press SPACE to select this' }).getByLabel(text)).toBeVisible();

        } catch (e) {
            console.log(`Error in verify the label text : ${(e as Error).message}`);
            throw e;
        }
    }
    async clickOnMenuBtn(buttonName: string): Promise<void> {
        try {

            await this.page.getByRole('menuitem', { name: "New Configuration Answer. Add New Configuration Answer", exact: true }).first().focus(); // will update later
            await this.page.keyboard.press("ArrowDown");
            await this.page.keyboard.press('PageDown');
            await this.page.getByRole('menuitem', { name: buttonName, exact: true }).first().focus();
            await this.page.getByRole('menuitem', { name: buttonName, exact: true }).first().click();


        } catch (e) {
            console.log(`Error in clicking on Menu button : ${(e as Error).message}`);
            throw e;
        }
    }

    async fetchRecordGuid(url: string): Promise<string> {
        let guid;
        try {
            console.log(`URL : ${url}`);
            const parsedUrl = new URL(url);
            guid = parsedUrl.searchParams.get('id') ?? '';
            console.log(`GUID : ${guid}`);
        } catch (e) {
            console.log(`Error in fetching GUID : ${(e as Error).message}`);
            throw e;
        }
        return guid;
    }

    async closeAIAlerts(): Promise<void> {
        const hideformFillAssist = Common.Text.HideformFillAssist;
        try {
            if (await this.page.getByRole('button', { name: hideformFillAssist }).isVisible()) {
                //await this.page.getByRole('button', { name: hideformFillAssist }).click({ force: true });
                console.log(`AI popup is closed`);
            }
        } catch (e) {
            console.log(`Error while closing AI alerts : ${(e as Error).message}`);
            throw e;
        }
    }

    async dismissAIAlerts(): Promise<void> {
        const closeAssist = Common.AriaLabel.Dismiss;
        try {
            if (await this.page.getByRole('button', { name: closeAssist }).isVisible()) {
                await this.page.getByRole('button', { name: closeAssist }).click();

                console.log(`AI popup is closed`);
            }
        } catch (e) {
            console.log(`Error while closing AI alerts : ${(e as Error).message}`);
            throw e;
        }
    }
    async closeAddSrcAIAlerts(): Promise<void> {
        const hideformFillAssist = Common.Text.HideformFillAssist;
        const addSourcesHere = Common.Text.AddSourcesHere;
        try {
            await expect(this.page.locator("//*[text()='" + hideformFillAssist + "']")).toBeVisible();
            await expect(this.page.locator("//*[text()='" + addSourcesHere + "']")).toBeVisible();
            await this.page.locator("//*[text()='" + addSourcesHere + "']").click();

        } catch (e) {
            console.log(`Error while closing AI alerts : ${(e as Error).message}`);
            throw e;
        }
    }
    async closeAIForm(): Promise<void> {
        const hideformFillAssist = Common.CSS.AIForm;
        try {
            if (await this.page.locator(hideformFillAssist).isVisible()) {
                //await this.page.locator(hideformFillAssist).click({ force: true });
            }
        } catch (e) {
            console.log(`Error while closing AI Form : ${(e as Error).message}`);
            throw e;
        }
    }

    async handleConfirmationPopup(): Promise<void> {
        const saveAndContinue = Common.Title.SaveAndContinue;
        try {
            await this.page.waitForTimeout(2000); // This is intermittent issue Giving some wait time for the popup to appear
            if (await this.page.getByTitle(saveAndContinue).isVisible()) {
                await this.page.getByTitle(saveAndContinue).click();

            }
        } catch (e) {
            console.log(`Error while closing confirmation popup : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyLinkText(linkText: string, isVisible: boolean = true): Promise<void> {
        try {
            if (isVisible) {
                await expect(this.page.getByRole('link', { name: linkText })).toBeVisible();
            } else {
                await expect(this.page.getByRole('link', { name: linkText })).not.toBeVisible();
            }

        } catch (e) {
            console.log(`Error while verifying the link text : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyButtonText(button: string, isVisible: boolean = true): Promise<void> {
        try {
            if (isVisible) {
                await expect(this.page.getByRole('button', { name: button })).toBeVisible();
            }
            else
                await expect(this.page.getByRole('button', { name: button })).toBeHidden();

        } catch (e) {
            console.log(`Error while verifying the button text : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifytheButton(button: string, isVisible: boolean = true): Promise<void> {

        try {
            if (isVisible) {
                await expect(this.page.getByTestId(button)).toBeVisible();
            }
            else
                await expect(this.page.getByTestId(button)).toBeHidden();

        } catch (e) {
            console.log(`Error while verifying the button text : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyGridCellText(text: string): Promise<void> {
        try {
            await expect(this.page.getByRole('gridcell', { name: text }).first()).toBeVisible();

        } catch (e) {
            console.log(`Error while verifying the text : ${(e as Error).message}`);
            throw e;
        }
    }
    async clickOnGridCellText(text: string): Promise<void> {
        try {
            await this.page.getByRole('gridcell', { name: text }).first().waitFor();
            await expect(this.page.getByRole('gridcell', { name: text }).first()).toBeVisible();
            await this.page.getByRole('gridcell', { name: text }).first().dblclick();

        } catch (e) {
            console.log(`Error while Click on the Grid text : ${(e as Error).message}`);
            throw e;
        }
    }

    async selectGridCellText(text: string): Promise<void> {
        try {
            await this.page.getByRole('gridcell', { name: text }).first().waitFor();
            await expect(this.page.getByRole('gridcell', { name: text }).first()).toBeVisible();
            await this.page.getByRole('gridcell', { name: text }).first().click();

        } catch (e) {
            console.log(`Error while Click on the Grid text : ${(e as Error).message}`);
            throw e;
        }
    }

    async clickOnSwitchbutton(text: string): Promise<void> {
        try {
            await this.page.getByRole('switch', { name: text }).first().waitFor();
            await expect(this.page.getByRole('switch', { name: text }).first()).toBeVisible();
            await this.page.getByRole('switch', { name: text }).first().focus();
            await this.page.getByRole('switch', { name: text }).first().click();

        } catch (e) {
            console.log(`Error while Click the switch button : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifySwitchbutton(text: string): Promise<void> {
        try {
            await this.page.getByRole('switch', { name: text }).first().waitFor();
            await expect(this.page.getByRole('switch', { name: text }).first()).toBeVisible();
            await this.page.getByRole('switch', { name: text }).first().focus();

        } catch (e) {
            console.log(`Error while verifying the switch button : ${(e as Error).message}`);
            throw e;
        }
    }
    async clickonLinkText(linkText: string): Promise<void> {
        try {
            await expect(this.page.getByRole('link', { name: linkText })).toBeVisible();
            await this.page.getByRole('link', { name: linkText }).first().click();

        } catch (e) {
            console.log(`Error while Click on the link text : ${(e as Error).message}`);
            throw e;
        }
    }

    async clickonStartWithText(linkText: string): Promise<void> {
        try {
            const regex = new RegExp(`^${linkText}`);
            await expect(this.page.getByText(regex).first()).toBeVisible();
            await this.page.getByText(regex).first().click();

        } catch (e) {
            console.log(`Error while Click on the link text : ${(e as Error).message}`);
            throw e;
        }
    }
    async getTheCountofRecords(linkText: string): Promise<number> {
        try {
            const regex = new RegExp(`^${linkText}`);
            const count = await this.page.getByRole("link", { name: regex }).count();

            return count;
        } catch (e) {
            console.log(`Error while getting the count of link text : ${(e as Error).message}`);
            throw e;
        }

    }
    async ClickonButtonbyTitle(btnname: string): Promise<void> {
        try {
            await this.page.getByTitle(btnname, { exact: true }).first().waitFor();
            await this.page.getByTitle(btnname, { exact: true }).first().focus();
            await this.page.getByTitle(btnname, { exact: true }).first().click();
        } catch (e) {
            console.log(`Error while clicking on button : ${(e as Error).message}`);
            throw e;
        }
    }
    async enterTextinSearchbox(textbox: string, text: string): Promise<void> {
        try {
            await this.page.getByRole("searchbox", { name: textbox, exact: true }).first().waitFor({ state: "visible" });
            await this.page.getByRole("searchbox", { name: textbox, exact: true }).first().focus();
            await expect(this.page.getByRole("searchbox", { name: textbox, exact: true }).first()).toBeEditable();
            await this.page.getByRole("searchbox", { name: textbox, exact: true }).first().click();

            await this.page.waitForTimeout(2000);
            await this.page.getByRole("searchbox", { name: textbox, exact: true }).first().fill(text);


        } catch (e) {
            console.log(`Error while entering text in Searchbox : ${(e as Error).message}`);
            throw e;
        }
    }
    async clickOnByRoleTextbox(locator: string): Promise<void> {
        try {
            await this.page.getByRole('textbox', { name: locator, exact: true }).waitFor({ state: "visible" });
            await expect(this.page.getByRole('textbox', { name: locator, exact: true })).toBeVisible();
            await this.page.getByRole('textbox', { name: locator, exact: true }).click();


        } catch (e) {
            console.log(`Error in entering text: ${(e as Error).message}`);
            throw e;
        }
    }
    async clickOnMenuButton(buttonName: string, position: number): Promise<void> {
        try {
            await this.page.getByRole('menuitem', { name: buttonName, exact: true }).nth(position).waitFor({ state: "visible" });
            await expect(this.page.getByRole('menuitem', { name: buttonName, exact: true }).nth(position)).toBeVisible();
            await this.page.getByRole('menuitem', { name: buttonName, exact: true }).nth(position).focus();
            await this.page.getByRole('menuitem', { name: buttonName, exact: true }).nth(position).click();


        } catch (e) {
            console.log(`Error in clicking commandbar menu button : ${(e as Error).message}`);
            throw e;
        }
    }
    async clickOnAddicon(): Promise<void> {
        try {
            await this.page.waitForTimeout(5000);
            await this.page.locator('.___1960zyq_0000000 > button:nth-child(2)').click();
            await this.page.waitForTimeout(5000);

        } catch (e) {
            console.log(`Error in clicking Add icon in Questionnire : ${(e as Error).message}`);
            throw e;
        }
    }
    async validateAddicon(isVisible: boolean = true): Promise<void> {
        try {
            if (isVisible) {
                await expect(this.page.getByRole('button').filter({ hasText: /^$/ }).nth(4).first()).toBeVisible();
            }
            else
                await expect(this.page.getByRole('button').filter({ hasText: /^$/ }).nth(4).first()).toBeHidden();


        } catch (e) {
            console.log(`Error in verifying the Add icon in Questionnire : ${(e as Error).message}`);
            throw e;
        }
    }
    async getAutomationKeyGridCellValue(key: string): Promise<string[]> {

        try {
            await this.page.locator(key).first().waitFor();
            const code = await this.page.locator(key).allTextContents();
            return code;
        } catch (e) {
            console.log(`Error in Getting the value : ${(e as Error).message}`);
            throw e;
        }

    }
    async executeDragAndDrop(srcbtn: string, destbtn: string): Promise<void> {
        try {
            await this.page.getByRole("button", { name: srcbtn, exact: true }).first().waitFor();
            await this.page.getByRole("button", { name: srcbtn, exact: true }).first().focus();
            const src = await this.page.getByRole("button", { name: srcbtn, exact: true }).first();
            const dest = await this.page.getByRole("button", { name: destbtn, exact: true }).first();

            await src.dragTo(dest); // Dragging  & dropping 

            await this.page.waitForTimeout(2000);

        } catch (e) {
            console.log(`Error while dragging and dropping ${(e as Error).message}`);
            throw e;
        }
    }

    async getAllAnswerTexts(locator: string): Promise<string[]> {
        var answerTexts: string[];
        try {
            await this.page.locator(locator).first().waitFor();
            answerTexts = await this.page.locator(locator).allInnerTexts();
            return answerTexts

        } catch (e) {
            console.log(`Error while getting the All Answer Texts ${(e as Error).message}`);
            throw e;
        }
    }
    async VerifyInputFieldIsEditable(ariaLabel: string, isEditable: boolean = false): Promise<void> {
        try {
            await expect(this.page.getByLabel(ariaLabel).first()).toBeVisible();
            const hasAttr = await this.page.getByRole("textbox", { name: ariaLabel }).evaluate(el => el.hasAttribute('readonly'));
            if (isEditable) {
                await expect(hasAttr).toBeFalsy();
            }
            else
                await expect(hasAttr).toBeTruthy();
        } catch (e) {
            console.log(`Error while  validating the Readonly fields ${(e as Error).message}`);
            throw e;
        }
    }

    async deleteAllSubgridRecords(action: string): Promise<void> {
        const allRowCheckbox = Common.CSS.Rows;
        try {
            await expect(this.page.locator(allRowCheckbox).first()).toBeVisible();
            await this.page.locator(allRowCheckbox).first().focus();
            await this.page.locator(allRowCheckbox).first().click();
            await expect(this.page.getByRole('menuitem', { name: action, exact: true }).nth(0)).toBeVisible();
            await this.page.getByRole('menuitem', { name: action, exact: true }).nth(0).click();

        } catch (e) {
            console.log(`Error in deleting records : ${(e as Error).message}`);
            throw e;
        }
    }
    async searchRecord(filterbykeyword: string, record: string): Promise<void> {
        try {
            await this.page.getByPlaceholder(filterbykeyword, { exact: true }).first().waitFor({ state: "visible" });
            await this.page.getByPlaceholder(filterbykeyword, { exact: true }).first().click();
            await this.page.getByPlaceholder(filterbykeyword, { exact: true }).first().clear();
            await this.page.getByRole('searchbox', { name: filterbykeyword }).fill(record);
            await this.page.getByRole('searchbox', { name: filterbykeyword }).press('Enter');
        } catch (e) {
            console.log(`Error while searching record : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyNewButton(): Promise<void> {
        const newBtn = Common.Text.New;
        const newMenuItem = this.page.getByRole('menuitem', { name: newBtn }).first();
        try {
            await newMenuItem.waitFor({ state: 'visible' });
            await expect(newMenuItem).toBeEnabled();
            await expect(newMenuItem).toBeVisible();
            await expect(newMenuItem).toBeAttached();

        } catch (e) {
            console.log(`Error in verifying the New button : ${(e as Error).message}`);
            throw e;
        }
    }

    async verifyAcceptSuggestion(): Promise<void> {
        const acceptSuggestionBtn = Common.CSS.AcceptSuggestion;
        try {
            await this.page.locator(acceptSuggestionBtn).waitFor({ state: "visible", timeout: 10000 });
            await expect(this.page.locator(acceptSuggestionBtn)).toBeEnabled();
        } catch (e) {
            console.log(`Error while verifying the Accept Suggestion button : ${(e as Error).message}`);
            throw e;
        }
    }
    async clickOnPopupButton(btnName: string): Promise<void> {
        try {
            await this.page.waitForTimeout(3000); //getting intermittent error without wait
            if (await this.page.getByRole('button', { name: btnName }).first().isVisible()) {
                await expect(this.page.getByRole('button', { name: btnName }).first()).toBeVisible();
                await this.page.getByRole('button', { name: btnName }).first().waitFor();
                await this.page.getByRole('button', { name: btnName }).first().click();
            }

        } catch (e) {
            console.log(`Error while confirming popup window : ${(e as Error).message}`);
            throw e;
        }
    }
    async CheckThecheckbox(element: string): Promise<void> {
        try {
            const locator = await this.page.locator(element).first();
            await locator.waitFor({ state: "visible" });
            await locator.focus();
            await expect(locator).toBeVisible();
            if (! await locator.isChecked())
                await locator.click({ force: true });

        } catch (e) {
            console.log(`Error while clicking checkbox: ${(e as Error).message}`);
            throw e;
        }
    }
    async isElementTextIsVisible(text: string): Promise<boolean> {
        try {
            return await this.page.getByText(text).isVisible();
        } catch (e) {
            console.log(`Error while verifying the text : ${(e as Error).message}`);
            throw e;
        }
    }
    async clickOnNewConfigurationAnswer(buttonName: string): Promise<void> {
        try {
            await this.page.getByRole('textbox', { name: buttonName, exact: true }).first().focus();
            await this.page.getByRole('textbox', { name: buttonName, exact: true }).first().click();
            await this.page.keyboard.press('Tab');
            await this.page.keyboard.press('Enter');
            await this.verifySaveButton();

        } catch (e) {
            console.log(`Error while clicking on button : ${(e as Error).message}`);
            throw e;
        }
    }

    async mouseHoverTheIcons(questionName: string, position: number): Promise<void> {
        try {
            await this.page.getByRole('button', { name: `${questionName}` }).getByRole('button').nth(position).waitFor();
            await this.page.getByRole('button', { name: `${questionName}` }).getByRole('button').nth(position).hover();

        } catch (e) {
            console.log(`Error while hover the icon for the Question added in questionnaire  : ${(e as Error).message}`);
            throw e;
        }
    }
    async getToolTipText(questionName: string, position: number): Promise<string> {
        try {
            await this.page.getByRole('button', { name: `${questionName}` }).getByRole('button').nth(position).waitFor();
            const locator = await this.page.getByRole('button', { name: `${questionName}` }).getByRole('button').nth(position) ?? "";

            const role = await this.page.getAttribute("//div[@class='fui-AccordionItem']//button[@aria-labelledby]", "aria-labelledby") ?? "";
            const text = await this.page.locator(`div#${role}`).textContent() ?? "";
            return text;

        } catch (e) {
            console.log(`Error while getting tool tip text : ${(e as Error).message}`);
            throw e;
        }
    }

    async getHeaderTitle(): Promise<string> {
        const title = Common.CSS.HeaderTitle;
        try {
            await this.page.locator(title).waitFor();
            const header = await this.page.locator(title).textContent();
            return header ?? "";

        } catch (e) {
            console.log(`Error while getting the Header Title  : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyCommandBarBtn(buttonName: string): Promise<void> {
        try {
            await this.page.getByRole('menuitem', { name: buttonName, exact: true }).first().waitFor({ state: "visible" });
            await this.page.getByRole('menuitem', { name: buttonName, exact: true }).first().waitFor({ state: "attached" });
            await expect(this.page.getByRole('menuitem', { name: buttonName, exact: true }).first()).toBeVisible();
            await expect(this.page.getByRole('menuitem', { name: buttonName, exact: true }).first()).toBeEnabled();

        } catch (e) {
            console.log(`Error in verifying the commandbar button : ${(e as Error).message}`);
            throw e;
        }
    }
    async getLocatorText(locator: string): Promise<string> {
        try {
            await this.page.locator(locator).waitFor();
            const text = await this.page.locator(locator).textContent();
            return text ?? "";

        } catch (e) {
            console.log(`Error while getting the text : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyLocatorvalue(locator: string): Promise<void> {
        try {
            await this.page.locator(locator).waitFor();
        } catch (e) {
            console.log(`Error while validating the locator value : ${(e as Error).message}`);
            throw e;
        }
    }
    async selectcheckboxByLocator(label: string): Promise<void> {
        const locator = `//span[text()='${label}']//..//input`;
        try {
            await this.page.waitForTimeout(1000);
            await this.page.locator(locator).waitFor();
            await this.page.locator(locator).click();

        } catch (e) {
            console.log(`Error while clicking checkbox for ${label} : ${(e as Error).message}`);
            throw e;
        }
    }
    async verifyFieldEnabled(locator: string): Promise<void> {
        try {
            await this.page.locator(locator).waitFor({ state: "visible" });
            await expect(this.page.locator(locator).isDisabled()).toBeFalsy();
        } catch (e) {
            console.log(`Error while  validating the Enabled field ${(e as Error).message}`);
            throw e;
        }
    }
}
