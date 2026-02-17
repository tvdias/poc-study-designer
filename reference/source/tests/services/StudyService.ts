import { Page, expect } from '@playwright/test';
import { Project } from '../selectors/ProjectSelectors.json';
import { WebHelper } from '../utils/WebHelper';
import { waitUntilAppIdle } from '../utils/Login';
import { StudySelectors } from '../selectors/StudySelectors.json'
import { DropDownList } from '../constants/DropDownList.json'
import path from 'path';
import { Common } from '../selectors/CommonSelector.json'
import { time } from 'console';
export class StudyService {
    protected page: Page;
    private webHelper: WebHelper;

    constructor(page: Page) {
        this.page = page;
        this.webHelper = new WebHelper(this.page);
    }

    /**
     * Navigate to Studies tab and open the new study form
     * Used for performance testing to separate navigation from form fill
     */
    async navigateToStudiesTabAndOpenNewForm(): Promise<void> {
        const studies = Project.Tabs.Studies;
        const addNewStudy = StudySelectors.ByRole.AddNewStudy;
        try {
            await this.webHelper.clickOnTab(studies);
            await this.webHelper.clickOnCommandBarBtn(addNewStudy);
            await this.ValidateStudyStatusReason(DropDownList.Status.Draft);
            await this.webHelper.verifyAcceptSuggestion();
            await this.webHelper.clickOnHideFormButton();
        } catch (e) {
            console.log(`Error navigating to studies tab: ${(e as Error).message}`);
            throw e;
        }
    }

    /**
     * Fill study form without saving - used for performance testing
     * to measure save operation time separately
     */
    async fillStudyForm(studyName: string, category: string, fieldWorkmarket: string, 
        maconomyJobNumber: string, projectOperationsURL: string, scripterNotes: string): Promise<void> {
        const categoryElement = StudySelectors.ByRole.Category;
        try {
            await this.webHelper.enterTextByRoleTextbox(StudySelectors.ByRole.Name, studyName);
            await this.webHelper.enterTextByRoleTextbox(categoryElement, category);
            await this.webHelper.selectLookupByAriaLabel(StudySelectors.AriaLabel.FieldWorkMarket, fieldWorkmarket);
            await this.webHelper.enterTextByRoleTextbox(StudySelectors.AriaLabel.ProjectOperationsURL, projectOperationsURL);
            await this.webHelper.enterTextByRoleTextbox(StudySelectors.ByRole.ScripterNotes, scripterNotes);
            await this.webHelper.enterTextByRoleTextbox(StudySelectors.AriaLabel.MaconomyJobNumber, maconomyJobNumber);
        } catch (e) {
            console.log(`Error filling study form: ${(e as Error).message}`);
            throw e;
        }
    }

    async CreateNewStudy(studyName: string, category: string, fieldWorkmarket: string, maconomyJobNumber: string,
        projectOperationsURL: string, scripterNotes: string): Promise<void> {
        const studies = Project.Tabs.Studies;
        const addNewStudy = StudySelectors.ByRole.AddNewStudy;
        const categoryElement = StudySelectors.ByRole.Category;
        try {

            await this.webHelper.clickOnTab(studies);
            await this.webHelper.clickOnCommandBarBtn(addNewStudy);
            await this.ValidateStudyStatusReason(DropDownList.Status.Draft);
            await this.webHelper.verifyAcceptSuggestion();
            await this.webHelper.clickOnHideFormButton();
            await this.webHelper.enterTextByRoleTextbox(StudySelectors.ByRole.Name, studyName);

            await this.webHelper.enterTextByRoleTextbox(categoryElement, category);
            await this.webHelper.selectLookupByAriaLabel(StudySelectors.AriaLabel.FieldWorkMarket, fieldWorkmarket);
            await this.webHelper.enterTextByRoleTextbox(StudySelectors.AriaLabel.ProjectOperationsURL, projectOperationsURL);
            await this.webHelper.enterTextByRoleTextbox(StudySelectors.ByRole.ScripterNotes, scripterNotes);
            await this.webHelper.enterTextByRoleTextbox(StudySelectors.AriaLabel.MaconomyJobNumber, maconomyJobNumber);

            await this.webHelper.saveRecord();
            await this.webHelper.closeAIAlerts();
            await this.webHelper.closeAIForm();
            console.log(`New study is created: ${studyName}`);

        } catch (e) {
            console.log(`Error while creating a new study record : ${(e as Error).message}`);
            throw e;
        }
    }

    async UpdateFieldWorkMarket(fieldWorkmarket: string, existingFieldworkMarket: string) {
        try {
            await this.page.waitForTimeout(2000);
            await this.webHelper.clickOnButton("Delete " + existingFieldworkMarket);
            await this.webHelper.selectLookupByAriaLabel(StudySelectors.AriaLabel.FieldWorkMarket, fieldWorkmarket);
            await this.webHelper.saveRecord();

        } catch (e) {
            console.log(`Error while updating the FieldWork Market ${(e as Error).message}`);
            throw e;
        }
    }

    async ValidateReadOnlyFieldsInStudy(): Promise<void> {
        try {
            await this.webHelper.verifyFieldReadonly(StudySelectors.AriaLabel.MaconomyJobNumber);
            await this.webHelper.verifyFieldReadonly(StudySelectors.AriaLabel.ProjectOperationsURL);
            await this.webHelper.verifyFieldDisabled(StudySelectors.CSS.FieldWorkMarket);
            await this.webHelper.validateCommandBarBtnNotVisible(StudySelectors.AriaLabel.NewFieldworkLangauges, StudySelectors.AriaLabel.MoreCommandsForFieldworkLanguages);
            await this.page.reload();
            await this.webHelper.verifySaveButton();
        } catch (e) {
            console.log(`Error while validating the Readonly fields in Study ${(e as Error).message}`);
            throw e;
        }
    }
    async ValidateEditableFieldsInStudy(): Promise<void> {
        try {
            await this.webHelper.VerifyInputFieldIsEditable(StudySelectors.AriaLabel.Name, true);
            await this.webHelper.VerifyInputFieldIsEditable(StudySelectors.AriaLabel.Category, true);
            await this.webHelper.VerifyInputFieldIsEditable(StudySelectors.AriaLabel.MaconomyJobNumber, true);
            await this.webHelper.VerifyInputFieldIsEditable(StudySelectors.AriaLabel.ProjectOperationsURL, true);
            await this.webHelper.verifyButtonText(StudySelectors.AriaLabel.SearchRecordsForFieldworkMarket);
            await this.webHelper.clickOnCommandBarBtn(StudySelectors.AriaLabel.MoreCommandsForFieldworkLanguages);
            await this.webHelper.handleConfirmationPopup();
            await this.webHelper.verifyCommandBarBtn(StudySelectors.AriaLabel.NewFieldworkLangauges);
        } catch (e) {
            console.log(`Error while validating the Editable fields in Study ${(e as Error).message}`);
            throw e;
        }
    }

    async ValidateInitialDraftStateStudy() {
        try {
            await this.webHelper.handleConfirmationPopup();
            await expect(this.page.getByRole('treegrid', { name: StudySelectors.ByRole.FieldworkLanguagesGrid })).toBeVisible();
            await this.ValidateStudyVersionNumber(StudySelectors.VersionNumber.One);
            await this.ValidateStudyStatusReason(DropDownList.Status.Draft);
        } catch (e) {
            console.log(`Error while validating a Draft study : ${(e as Error).message}`);
            throw e;
        }
    }

    async ValidateStudyVersionNumber(studyVersionNo: string) {
        const versionNumber = StudySelectors.CSS.VersionNumber;
        const versionNumbertext = StudySelectors.Text.VersionNumber;
        try {
            await expect(this.page.locator(versionNumber).nth(1)).toContainText(studyVersionNo);
            await expect(this.page.locator(StudySelectors.CSS.VersionNumber).nth(1)).toContainText(versionNumbertext);
        } catch (e) {
            console.log(`Error while validating study version number : ${(e as Error).message}`);
            throw e;
        }
    }

    async ValidateStudyStatusReason(studyStatusReason: string) {
        const statuReason = StudySelectors.CSS.StatuReason;
        const statusReasonText = StudySelectors.Text.StatusReason;
        try {

            await expect(this.page.locator(statuReason).nth(2)).toContainText(studyStatusReason);
            await expect(this.page.locator(statuReason).nth(2)).toContainText(statusReasonText);
        } catch (e) {
            console.log(`Error while validating study status reason : ${(e as Error).message}`);
            throw e;
        }
    }

    async compareMasterQuestionnairesWithStudyQuestionnairesLines(masterQuestionsCount: number, masterVariableNames: string[]) {
        try {
            const studyQuestionsCount = await this.getStudyQuestionnaireLinesCount();
            const studyVariableNames: string[] = await this.getStudyQuestionnaireLinesVaribaleName();

            console.log(`master questions count:${masterQuestionsCount} & Study questions count : ${studyQuestionsCount}`);
            console.log(`master variable names:${masterVariableNames}`);
            console.log(`Study variable names:${studyVariableNames}`);

            expect(masterQuestionsCount).toEqual(studyQuestionsCount);
            expect(masterVariableNames).toEqual(studyVariableNames);
        } catch (e) {
            console.log(`Error while comparing study & master questionnaire lines : ${(e as Error).message}`);
            throw e;
        }
    }

    async compareMasterQuestionnairesWithRFSStudyQuestionnairesLines(masterQuestionsCount: number, masterVariableNames: string[]) {
        try {
            const studyQuestionsCount = await this.getStudyQuestionnaireLinesCount();
            const studyVariableNames: string[] = await this.getStudyQuestionnaireLinesVaribaleName();

            console.log(`master questions count:${masterQuestionsCount} & Study questions count : ${studyQuestionsCount}`);
            console.log(`master variable names:${masterVariableNames}`);
            console.log(`Study variable names:${studyVariableNames}`);

            expect(masterQuestionsCount).not.toEqual(studyQuestionsCount);
            expect(masterVariableNames).not.toEqual(studyVariableNames);
        } catch (e) {
            console.log(`Error while comparing RFS study & master questionnaire lines : ${(e as Error).message}`);
            throw e;
        }
    }


    async getStudyQuestionnaireLinesCount(): Promise<number> {
        const questionnaireVariable = StudySelectors.CSS.QuestionnaireVariable;
        try {
            await expect(this.page.locator(questionnaireVariable).nth(0)).toBeVisible();
            const QuestionnaireLines = this.page.locator(questionnaireVariable);
            return await QuestionnaireLines.count();
        } catch (e) {
            console.log(`Error while fetching master questionnaire lines count : ${(e as Error).message}`);
            throw e;
        }
    }

    async deactivateTheQuestionFromStudy(text: string): Promise<void> {
        try {
            await this.webHelper.clickOnTab(Common.Tabs.Related);
            await this.webHelper.clickOnCommandBarBtn(StudySelectors.AriaLabel.StudyQuestionnaireLines);
            await this.webHelper.selectTheRow();
            await this.webHelper.clickOnCommandBarBtn(Common.Text.Deactivate);
            await this.webHelper.ClickonButtonbyTitle(Common.Text.Deactivate);
            await this.webHelper.verifyTheLabeltext(text, false);
        } catch (e) {
            console.log(`Error while deactivating the question from Study ${(e as Error).message}`);
            throw e;
        }
    }

    async activateTheQuestionFromStudy(text: string): Promise<void> {
        try {
            await this.webHelper.clickOnTab(Common.Tabs.Related);
            await this.webHelper.clickOnCommandBarBtn(StudySelectors.AriaLabel.StudyQuestionnaireLines);
            await this.webHelper.selectTheRow();
            await this.webHelper.clickOnCommandBarBtn(Common.Text.Activate);
            await this.webHelper.ClickonButtonbyTitle(Common.Text.Activate);
            await this.webHelper.verifyTheLabeltext(text, false);
        } catch (e) {
            console.log(`Error while reactivating the question from Study ${(e as Error).message}`);
            throw e;
        }
    }

    async verifytheQuestionInSelectedView(text: string, view: string): Promise<void> {
        try {
            await this.webHelper.clickOnButton(StudySelectors.AriaLabel.StudyQuestionnaireLineAssociatedView);
            await this.webHelper.clickonStartWithText(view);
            await this.webHelper.verifyTheGridLabelValue(text);
        } catch (e) {
            console.log(`Error while verifying the Deactivation question ${(e as Error).message}`);
            throw e;
        }
    }
    async getStudyQuestionnaireLinesVaribaleName(): Promise<string[]> {
        const questionnaireVariable = StudySelectors.CSS.QuestionnaireVariable;
        try {
            var studyVariableNames: string[] = [];
            await this.page.locator(questionnaireVariable + ' label').first().waitFor({ state: "visible" });
            await this.page.locator(questionnaireVariable + ' label').first().focus();
            const QuestionnaireLines = this.page.locator(questionnaireVariable + ' label');
            const count = await QuestionnaireLines.count();
            for (let index = 0; index < count; index++) {
                const text = await QuestionnaireLines.nth(index).innerText();
                if (text) {
                    studyVariableNames.push(text.trim());
                }
            }
            return studyVariableNames;
        } catch (e) {
            console.log(`Error while fetching master questionnaire lines variables names : ${(e as Error).message}`);
            throw e;
        }
    }

    async getStudySnapshotsVaribaleNames(): Promise<string[]> {
        const snapshotsVariableNames = StudySelectors.CSS.SnapshotsVariableNames;
        try {
            var studyVariableNames: string[] = [];
            await this.page.locator(snapshotsVariableNames).first().waitFor({ state: "visible" });
            const QuestionnaireLines = this.page.locator(snapshotsVariableNames);
            const count = await QuestionnaireLines.count();
            for (let index = 0; index < count; index++) {
                const text = await QuestionnaireLines.nth(index).innerText();
                if (text) {
                    studyVariableNames.push(text.trim());
                }
            }
            return studyVariableNames;
        } catch (e) {
            console.log(`Error while fetching study snapshots variable names : ${(e as Error).message}`);
            throw e;
        }
    }

    async ValidateCreateDocumentFunctionality(isVisible: boolean = true) {
        const clickHereToDownloadStudy = StudySelectors.Text.ClickHereToDownloadStudy;
        const clickFinishToProceedWith = StudySelectors.Text.ClickFinishToProceedWith;
        const finish = StudySelectors.ByRole.Finish;
        try {
            if (isVisible)
                await this.webHelper.verifySaveButton();
            await this.page.waitForTimeout(10000); //waiting for Loading the button
            if (await this.page.getByRole('menuitem', { name: StudySelectors.ByRole.CreateDocument, exact: true }).first().isHidden()) {
                await this.webHelper.clickOnCommandBarBtn(StudySelectors.AriaLabel.MoreCommands);
            }

            await this.page.getByTitle(StudySelectors.ByRole.CreateDocument).waitFor();
            await expect(this.page.getByTitle(StudySelectors.ByRole.CreateDocument)).toBeEnabled();
            await this.webHelper.clickOnCommandBarBtn(StudySelectors.ByRole.CreateDocument)
            await this.page.getByText(clickFinishToProceedWith).waitFor({ timeout: 60000 });

            await expect(this.page.getByRole('dialog', { name: StudySelectors.ByRole.DocumentsCorePackDialog })).toBeVisible();
            await expect(this.page.getByText(clickFinishToProceedWith)).toBeVisible();

            await expect(this.page.getByRole('button', { name: finish })).toBeVisible();
            await this.page.getByRole('button', { name: finish }).click();
        } catch (e) {
            console.log(`Error while create document functionality : ${(e as Error).message}`);
            throw e;
        }
    }
    async ValidateAndClickOnAbandonButton() {
        try {
            await this.webHelper.clickOnCommandBarBtn(StudySelectors.ByRole.Abandon);
            await this.webHelper.clickOnConfirmationPopup(Common.Text.Yes);
            await this.page.waitForTimeout(2000);
            await this.webHelper.clickOnConfirmationPopup(Common.Text.OK);

        } catch (e) {
            console.log(`Error while Click on Abandon button: ${(e as Error).message}`);
            throw e;
        }
    }
    async ValidateAndClickOnReworkbutton() {
        try {

            await this.webHelper.clickOnCommandBarBtn(StudySelectors.ByRole.Rework);
            await this.webHelper.clickOnConfirmationPopup(Common.Text.Yes);
            await this.webHelper.saveRecord();

        } catch (e) {
            console.log(`Error while Click on Rework button: ${(e as Error).message}`);
            throw e;
        }
    }
    async ValidateAndClickOnCompleteStudybutton() {
        try {
            await this.webHelper.clickOnCommandBarBtn(StudySelectors.ByRole.CompleteStudy);
            await this.webHelper.clickOnConfirmationPopup(Common.Text.OK);

        } catch (e) {
            console.log(`Error while Click on Complete Study button: ${(e as Error).message}`);
            throw e;
        }
    }

    async AddFieldWorkMarketLanguage(langauge: string) {
        try {
            await this.webHelper.clickOnCommandBarBtn(StudySelectors.AriaLabel.MoreCommandsForFieldworkLanguages);
            await this.webHelper.handleConfirmationPopup();
            await this.webHelper.clickOnCommandBarBtn(StudySelectors.AriaLabel.NewFieldworkLangauges);
            await this.webHelper.selectLookupByAriaLabel(StudySelectors.AriaLabel.Langauges, langauge);
            await this.webHelper.saveAndCloseQuickCreateRecord();


        } catch (e) {
            console.log(`Error while adding the FieldWork Market Language ${(e as Error).message}`);
            throw e;
        }
    }

    async ValidateXMLExportFunctionality(isVisible: boolean = true) {
        const clickFinishToProceedWith = StudySelectors.Text.ClickFinishToProceedWith;
        const finish = StudySelectors.ByRole.Finish;
        try {
            if (isVisible)
                await this.webHelper.verifySaveButton();

            await this.page.waitForTimeout(10000);//wait for Complete loading the button before clicking on Export to XML button 
            await this.page.getByTitle(StudySelectors.ByRole.ExportStudyAsXML).waitFor();
            await this.webHelper.clickOnCommandBarBtn(StudySelectors.ByRole.ExportStudyAsXML)
            await this.page.getByText(clickFinishToProceedWith).waitFor({ timeout: 60000 });

            await expect(this.page.getByRole('dialog', { name: StudySelectors.ByRole.DocumentsCorePackDialog })).toBeVisible();
            await expect(this.page.getByText(clickFinishToProceedWith)).toBeVisible();

            await expect(this.page.getByText(clickFinishToProceedWith)).toBeVisible();
            await expect(this.page.getByRole('button', { name: finish })).toBeVisible();
            await this.page.getByRole('button', { name: finish }).click();

        } catch (e) {
            console.log(`Error while exporting to xml file : ${(e as Error).message}`);
            throw e;
        }
    }

    async ValidateStudyButtons(buttons: string[], visibleOrHidden: string) {
        await this.page.waitForTimeout(2000);
        try {
            if (visibleOrHidden == 'Visible') {
                for (const button of buttons) {
                    await this.page.getByRole('menuitem', { name: button, exact: true }).first().waitFor();
                }
            } else {
                if (visibleOrHidden == 'Hidden') {
                    await this.page.getByRole("menuitem", { name: StudySelectors.AriaLabel.MoreCommands, exact: true }).click();

                    for (const button of buttons) {
                        await expect(this.page.getByRole('menuitem', { name: button, exact: true })).toBeVisible();
                    }
                }
                else {
                    for (const button of buttons) {
                        await expect(this.page.getByRole('menuitem', { name: button })).not.toBeVisible();
                    }
                }
            }
        } catch (e) {
            console.log(`Error while validating study buttons: ${(e as Error).message}`);
            throw e;
        }
    }

    async ValidateFieldworkMarketLanguages(text: string, visibleOrHidden: string) {
        try {
            if (visibleOrHidden == 'visible') {
                await this.webHelper.verifyLinkText(text);
            }
            else {
                await this.webHelper.verifyLinkText(text, false);
            }
        } catch (e) {
            console.log(`Error while validating study Fieldwork Market Languages: ${(e as Error).message}`);
            throw e;
        }
    }

    async updateStudyStatus(state: string): Promise<void> {
        const moreHeaderEditableFields = Common.ByRole.MoreHeaderEditableFields;
        const statusReason = Common.ByRole.StatusReason;
        try {
            await this.webHelper.handleConfirmationPopup();
            await this.page.getByRole('button', { name: moreHeaderEditableFields }).waitFor({ state: "visible" });
            await expect(this.page.getByRole('button', { name: moreHeaderEditableFields })).toBeVisible();
            await this.page.getByRole('button', { name: moreHeaderEditableFields }).focus();
            await this.page.getByRole('button', { name: moreHeaderEditableFields }).click({ force: true });
            await this.webHelper.handleConfirmationPopup();
            await this.page.getByRole('combobox', { name: statusReason }).waitFor({ state: "visible" });
            await expect(this.page.getByRole('combobox', { name: statusReason })).toBeVisible();
            await this.page.getByRole('combobox', { name: statusReason }).click();
            await this.page.getByRole('option', { name: state }).waitFor();
            await this.page.getByRole('option', { name: state }).click();
            await this.webHelper.handleConfirmationPopup();
            if (state == 'Ready for Scripting') {
                await this.webHelper.clickOnConfirmationPopup(Common.Text.OK);
            }
            await this.webHelper.saveRecord();
            await this.webHelper.handleConfirmationPopup();

        } catch (e) {
            console.log(`Error while updating study state : ${(e as Error).message}`);
            throw e;
        }
    }

    async ValidateStudySnapshotQuestions(): Promise<void> {
        try {
            const studyVariableNames: string[] = await this.getStudyQuestionnaireLinesVaribaleName();
            await this.page.reload();
            await this.webHelper.verifySaveButton();
            await this.page.waitForTimeout(4000);
            await this.page.reload();
            await this.webHelper.verifySaveButton();
            await this.webHelper.saveRecord();

            await this.webHelper.clickOnTab(StudySelectors.Tabs.Snapshots);
            const snapshotsVariableNames: string[] = await this.getStudySnapshotsVaribaleNames();

            console.log(`Study variable names :${studyVariableNames}`);
            console.log(`Study snapshots variable names :${snapshotsVariableNames}`);
            expect(snapshotsVariableNames).toEqual(studyVariableNames);
        } catch (e) {
            console.log(`Error while validating study snapshots questions : ${(e as Error).message}`);
            throw e;
        }
    }
    async ValidateStudySnapshotQuestionsIsDummy(): Promise<void> {
        try {

            for (let index = 0; index <= 2; index++) {
                await this.page.reload();

                await this.page.waitForTimeout(4000);
            }
            await this.webHelper.clickOnTab(StudySelectors.Tabs.Snapshots);
            const snapshotsVariableNames: string[] = await this.getStudySnapshotsVaribaleNames();
            console.log(`Study snapshots variable names :${snapshotsVariableNames}`);
            const IsDummy = StudySelectors.CSS.SnapshotsVariableNameDummyStatus;
            await this.page.locator(IsDummy).first().waitFor({ state: "visible" });
            const status = this.page.locator(IsDummy).innerText();
            expect(status).toBeTruthy();
        } catch (e) {
            console.log(`Error while validating snapshot study questions Dummy Status : ${(e as Error).message}`);
            throw e;
        }
    }


    async validateStudyChangeLogs(changeDetails: string[]): Promise<void> {
        let formerQuestionName = "";
        try {
            let count: number = 0;
            await this.webHelper.clickOnTab(StudySelectors.Tabs.ChangeLog);

            for (let index = 0; index <= 3; index++) {
                await this.page.reload();
                await this.page.waitForTimeout(3000);
                await this.webHelper.saveRecord();
                await this.webHelper.verifyTheTab(StudySelectors.Tabs.ChangeLog);
            }

            await this.webHelper.clickOnTab(StudySelectors.Tabs.ChangeLog);


            for (const changeDetail of changeDetails) {
                console.log(`Change details : ${changeDetail}`)
                var change = changeDetail.split(',');

                await expect(this.page.locator(StudySelectors.CSS.ChangeLogVariableName).nth(0)).toBeVisible();

                const questionnaireName = await this.page.locator(StudySelectors.CSS.ChangeLogCurrentQuestionName).nth(count).innerText();
                let changeType = await this.page.locator(StudySelectors.CSS.ChangeLogChangeType).nth(count).innerText();

                if (questionnaireName == "") {
                    formerQuestionName = await this.page.locator(StudySelectors.CSS.FormerQuestion).nth(count).innerText();
                }
                else {
                    formerQuestionName = questionnaireName;
                }

                console.log(`Expected variable name : ${change[0]} & Actual varibale name: ${formerQuestionName}`);
                console.log(`Expected change type : ${change[1]} & Actual change type : ${changeType}`);

                expect(formerQuestionName.trim()).toBe(change[0].trim());
                expect(changeType.trim()).toBe(change[1].trim());
                count++;
            }
        } catch (e) {
            console.log(`Error while validating study change logs : ${(e as Error).message}`);
            throw e;
        }
    }

    async validateStudyChangeLogsForModuleChange(changeDetails: string[], isVisible: boolean = true): Promise<void> {

        try {
            let count: number = 0;
            await this.webHelper.clickOnTab(StudySelectors.Tabs.ChangeLog);

            if (isVisible) {
                for (let index = 0; index <= 3; index++) {
                    await this.page.reload();
                    await this.page.waitForTimeout(3000);
                    await this.webHelper.saveRecord();
                    await this.webHelper.verifyTheTab(StudySelectors.Tabs.ChangeLog);
                }
            }
            await this.webHelper.clickOnTab(StudySelectors.Tabs.ChangeLog);
            for (const changeDetail of changeDetails) {
                console.log(`Change details : ${changeDetail}`)
                var change = changeDetail.split(',');

                await expect(this.page.locator(StudySelectors.CSS.ChangeLogVariableName).nth(0)).toBeVisible();

                const relatedObject = await this.page.locator(StudySelectors.CSS.ChangeLogModuleChange).nth(count).innerText();
                const moduleName = await this.page.locator(StudySelectors.CSS.ChangeLogFormerQuestionName).nth(count).innerText();
                let questionnaireName = await this.page.locator(StudySelectors.CSS.ChangeLogCurrentQuestionName).nth(count).innerText();
                let changeType = await this.page.locator(StudySelectors.CSS.ChangeLogChangeType).nth(count).innerText();
                if (questionnaireName == "") {
                    questionnaireName = await this.page.locator(StudySelectors.CSS.ChangeLogModuleFormerName).nth(count).innerText();
                }

                console.log(`Expected variable name : ${change[0]} & Actual varibale name: ${questionnaireName}`);
                console.log(`Expected change type : ${change[1]} & Actual change type : ${changeType}`);

                expect(relatedObject.trim()).toBe("Module");
                expect(moduleName.trim()).toBe(change[0].trim());
                //expect(questionnaireName.trim()).toBe(change[1].trim());
                expect(changeType.trim()).toBe(change[2].trim());
                count++;
            }
        } catch (e) {
            console.log(`Error while validating study change logs : ${(e as Error).message}`);
            throw e;
        }
    }
    async validateStudyChangeLogsForQuestionFieldChange(changeDetails: string[], isVisible: boolean = true): Promise<void> {

        try {
            let count: number = 0;
            await this.webHelper.clickOnTab(StudySelectors.Tabs.ChangeLog);

            if (isVisible) {
                for (let index = 0; index <= 3; index++) {
                    await this.page.reload();
                    await this.page.waitForTimeout(3000);
                    await this.webHelper.saveRecord();
                    await this.webHelper.verifyTheTab(StudySelectors.Tabs.ChangeLog);
                }
            }
            await this.webHelper.clickOnTab(StudySelectors.Tabs.ChangeLog);
            for (const changeDetail of changeDetails) {
                console.log(`Change details : ${changeDetail}`)
                var change = changeDetail.split(',');

                await expect(this.page.locator(StudySelectors.CSS.ChangeLogVariableName).nth(0)).toBeVisible();

                const relatedObject = await this.page.locator(StudySelectors.CSS.ChangeLogModuleChange).nth(count).innerText();
                let questionnaireName = await this.page.locator(StudySelectors.CSS.ChangeLogCurrentQuestionName).nth(count).innerText();
                let formerQuestionName = await this.page.locator(StudySelectors.CSS.ChangeLogModuleFormerName).nth(count).innerText();

                let changeType = await this.page.locator(StudySelectors.CSS.ChangeLogChangeType).nth(count).innerText();
                let fieldChange = await this.page.locator(StudySelectors.CSS.ChangeLogFieldChange).nth(count).innerText();

                console.log(`Expected variable name : ${change[0]} & Actual varibale name: ${questionnaireName}`);
                console.log(`Expected change type : ${change[1]} & Actual change type : ${changeType}`);

                expect(relatedObject.trim()).toBe("Question");
                expect(questionnaireName.trim()).toBe(change[0].trim());
                expect(formerQuestionName.trim()).toBe(change[1].trim());
                expect(changeType.trim()).toBe(change[2].trim());
                expect(fieldChange.trim()).toBe(change[3].trim());

                count++;
            }
        } catch (e) {
            console.log(`Error while validating study change logs : ${(e as Error).message}`);
            throw e;
        }
    }
    async validateFieldChangeStudyChangeLogs(changeDetails: string): Promise<void> {
        try {

            for (let index = 0; index <= 3; index++) {
                await this.page.reload();
                await this.page.waitForTimeout(3000);
                await this.webHelper.saveRecord();
                await this.webHelper.verifyTheTab(StudySelectors.Tabs.ChangeLog);
            }

            await this.webHelper.clickOnTab(StudySelectors.Tabs.ChangeLog);
            await expect(this.page.locator(StudySelectors.CSS.ChangeLogVariableName).nth(0)).toBeVisible();
            let changeType: string[] = await this.page.locator(StudySelectors.CSS.ChangeLogFieldChange).allInnerTexts();
            console.log(`Expected change type : ${changeDetails} & Actual change type : ${changeType}`);
            expect(changeType).toContain(changeDetails.trim());

        } catch (e) {
            console.log(`Error while validating study change logs for Field Change : ${(e as Error).message}`);
            throw e;
        }
    }

    async getMasterQuestionnaireLinesCount(): Promise<number> {
        const questionnaireVariable = StudySelectors.CSS.QuestionnaireVariable
        try {
            await this.page.locator(questionnaireVariable).nth(0).focus();
            await expect(this.page.locator(questionnaireVariable).nth(0)).toBeVisible();
            const masterQuestionnaireLines = this.page.locator(questionnaireVariable);
            return await masterQuestionnaireLines.count();
        } catch (e) {
            console.log(`Error while fetching master questionnaire lines count : ${(e as Error).message}`);
            throw e;
        }
    }
    async validateStudyChangeLogsForRemovingQuestions(changeDetails: string[]): Promise<void> {
        try {
            let count: number = 0;
            await this.webHelper.clickOnTab(StudySelectors.Tabs.ChangeLog);

            for (let index = 0; index <= 2; index++) {
                await this.page.reload();

                await this.page.waitForTimeout(5000);
            }

            await this.webHelper.clickOnTab(StudySelectors.Tabs.ChangeLog);


            for (const changeDetail of changeDetails) {
                console.log(`Change details : ${changeDetail}`)
                var change = changeDetail.split(',');

                await expect(this.page.locator(StudySelectors.CSS.ChangeLogQuestionName).nth(0)).toBeVisible();

                const questionnaireName = await this.page.locator(StudySelectors.CSS.ChangeLogQuestionName).nth(count).innerText();
                let changeType = await this.page.locator(StudySelectors.CSS.ChangeLogChangeType).nth(count).innerText();

                console.log(`Expected variable name : ${change[0]} & Actual varibale name: ${questionnaireName}`);
                console.log(`Expected change type : ${change[1]} & Actual change type : ${changeType}`);

                expect(questionnaireName.trim()).toBe(change[0].trim());
                expect(changeType.trim()).toBe(change[1].trim());
                count++;
            }
        } catch (e) {
            console.log(`Error while validating study change logs when Removing the Question : ${(e as Error).message}`);
            throw e;
        }
    }

    async validateStudyChangeLogsForAddingQuestions(changeDetails: string[], isVisible: boolean = true): Promise<void> {
        let formerQuestionName = "";
        try {
            let count: number = 0;
            await this.webHelper.clickOnTab(StudySelectors.Tabs.ChangeLog);
            if (isVisible) {
                for (let index = 0; index <= 5; index++) {
                    await this.page.reload();

                    await this.page.waitForTimeout(5000);
                }
            }
            await this.webHelper.clickOnTab(StudySelectors.Tabs.ChangeLog);

            for (const changeDetail of changeDetails) {
                console.log(`Change details : ${changeDetail}`)
                var change = changeDetail.split(',');

                await expect(this.page.locator(StudySelectors.CSS.ChangeLogCurrentQuestionName).nth(0)).toBeVisible();

                const questionnaireName = await this.page.locator(StudySelectors.CSS.ChangeLogCurrentQuestionName).nth(count).innerText();
                let changeType = await this.page.locator(StudySelectors.CSS.ChangeLogChangeType).nth(count).innerText();

                console.log(`Expected variable name : ${change[0]} & Actual varibale name: ${questionnaireName}`);
                console.log(`Expected change type : ${change[1]} & Actual change type : ${changeType}`);
                if (questionnaireName == "") {
                    formerQuestionName = await this.page.locator(StudySelectors.CSS.ChangeLogFormerQuestionName).nth(count).innerText();
                }
                else {
                    formerQuestionName = questionnaireName;
                }
                if (formerQuestionName == "") {
                    formerQuestionName = await this.page.locator(StudySelectors.CSS.ChangeLogModuleFormerName).nth(count).innerText();
                }
                expect(formerQuestionName.trim()).toBe(change[0].trim());
                expect(changeType.trim()).toBe(change[1].trim());
                count++;
            }
        } catch (e) {
            console.log(`Error while validating study change logs after Adding the Question: ${(e as Error).message}`);
            throw e;
        }
    }

    async createNewStudyVersion(newStudyVersionName: string): Promise<void> {
        try {
            await this.webHelper.verifySaveButton();
            await this.webHelper.clickOnCommandBarBtn(StudySelectors.ByRole.NewVersion);
            await this.webHelper.clickOnButton(Common.Text.Yes);
            await expect(this.page.getByText(StudySelectors.Text.NewStudyVersionSuccessMsg)).toBeVisible();
            await this.webHelper.clickOnButton(Common.Text.OK);
            await this.page.getByRole('textbox', { name: StudySelectors.ByRole.Name }).clear();
            await this.webHelper.enterTextByRoleTextbox(StudySelectors.ByRole.Name, newStudyVersionName);
            await this.webHelper.saveRecord();

            console.log(`New study version is created: ${newStudyVersionName}`);
        } catch (e) {
            console.log(`Error while creating new study version : ${(e as Error).message}`);
            throw e;
        }
    }

    async goToProject(project: string): Promise<void> {
        try {
            await this.webHelper.clickonLinkText(project);

        } catch (e) {
            console.log(`Error while navigating the project : ${(e as Error).message}`);
            throw e;
        }
    }

    async openStudy(studyName: string): Promise<void> {
        try {
            await this.webHelper.clickOnTab(Project.Tabs.Studies);
            await this.webHelper.clickonLinkText(studyName);
            await this.webHelper.verifyTheTab(Project.Tabs.ChangeLog)
        } catch (e) {
            console.log(`Error while opening existing study : ${(e as Error).message}`);
            throw e;
        }
    }
    async AddQuestionsToStudy(question: string) {
        try {
            await this.webHelper.clickOnCommandBarBtn(StudySelectors.AriaLabel.AddQuestions);
            await this.webHelper.selectLookupByAriaLabel(StudySelectors.AriaLabel.SelectRecord, question);
            await this.webHelper.ClickonButtonbyTitle(Common.Title.Add);
            await this.webHelper.clickOnConfirmationPopup(Common.Text.OK);


        } catch (e) {
            console.log(`Error while Adding the Question: ${(e as Error).message}`);
            throw e;
        }
    }

    async ValidateStudySnapshotQuestionsDeleted(): Promise<void> {
        const noDataText = StudySelectors.CSS.NoDataAvailableInGrid;
        try {
            await this.page.reload();
            await this.webHelper.verifySaveButton();
            await this.webHelper.clickOnTab(StudySelectors.Tabs.Snapshots);
            await this.page.locator(noDataText).last().waitFor({ state: "visible" });
        } catch (e) {
            console.log(`Error while validating study snapshots questions are Not Present : ${(e as Error).message}`);
            throw e;
        }
    }

    async AddNewFieldWorkMarketLanguage(langauge: string) {
        try {
            await this.webHelper.clickOnCommandBarBtn(StudySelectors.AriaLabel.NewFieldworkLangauges);
            await this.webHelper.selectLookupByAriaLabel(StudySelectors.AriaLabel.Langauges, langauge);
            await this.webHelper.saveAndCloseQuickCreateRecord();

        } catch (e) {
            console.log(`Error while adding the FieldWork Market Language ${(e as Error).message}`);
            throw e;
        }
    }
}

