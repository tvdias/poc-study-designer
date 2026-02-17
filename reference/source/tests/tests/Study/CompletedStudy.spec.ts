import { expect, mergeTests } from '@playwright/test';
import { logintest } from '../Fixture/LoginFixture';
import { addproduct } from '../Fixture/CreateProjectWithProduct';
import { ProjectService } from '../../services/ProjectService';
import { LoginToMDAWithTestUser } from '../../utils/Login';
import { Utils } from "../../utils/utils";
import { TestUser, AppName, AppId } from '../../constants/TestUsers.json';
import { StudyTestData } from '../../Test Data/StudyData.json'
import { DropDownList } from '../../constants/DropDownList.json';
import { StudyService } from '../../services/StudyService';
import { WebHelper } from '../../utils/WebHelper';
import { deleteRecord } from '../../utils/APIHelper';
import { StudySelectors } from '../../selectors/StudySelectors.json';
import { Questionnaireservice } from '../../services/QuestionnaireService';

// Using a LoginFixture and CreateProjectWithProduct to handle setup and teardown for all test cases:
// 1. Login the application with CS user
// 2. Create the project and add a product to the project before each test execution.
// 3. Delete the project after the each test case execution

// Merges the results of the login test and the add product test into a single combined test execution.
const test = mergeTests(logintest, addproduct);

test("[2359678,2359680,2373591] Verify Completed  study state - Check State, Version, Buttons, Create Document, Export XML", { tag: ['@Regression', '@Study'] }, async ({ page,loginPage, guid, projectName }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2359678' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2359680' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2373591' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);
    var guid: string;

    const studyName = StudyTestData.StudyName + Utils.generateGUID();

    await test.step('Create a study & Validate study state', async () => {
        await projectService.addScripterUserToProject();
        await studyService.CreateNewStudy(studyName, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await webHelper.handleConfirmationPopup();
        await studyService.ValidateInitialDraftStateStudy();
        await webHelper.saveRecord();
    });

    await test.step('Change the Study state to Ready for Scripting and save the study', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
    });
    await test.step('Change the study state to Approved for Launch and save the study', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ApprovedforLaunch);
    });
    await test.step('Click on the Complete Study button', async () => {
        await studyService.ValidateAndClickOnCompleteStudybutton();
    });

    await test.step('Validate study version & state of the study is Completed', async () => {
        await studyService.ValidateStudyVersionNumber(StudySelectors.VersionNumber.One);
        await studyService.ValidateStudyStatusReason(DropDownList.Status.Completed);
    });
    await test.step('Check study record is in inactive status & ready only', async () => {
        await studyService.ValidateReadOnlyFieldsInStudy();
    });
    await test.step('Validate button visibility when study is in Completed state', async () => {
        const visibleButtonsOne: string[] = StudySelectors.ExpectedResults.CompletedStudyButtons;
        await studyService.ValidateStudyButtons(visibleButtonsOne, 'Visible');
    });

    await test.step('Validate export to XML functionality', async () => {
        await studyService.ValidateXMLExportFunctionality(false);
    });
});

test("[2445498] Verify user is not allowed to edit study form Maconomy Job Number, Project Operations URL fields & Fieldwork Language grid records when study is in Ready for Scritping or Approved for Launch or Completed or Abandoned", { tag: ['@Regression', '@Study'] }, async ({ page,loginPage, guid, projectName }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2445498' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const questionnaireService = new Questionnaireservice(page);
    const webHelper = new WebHelper(page);
    var guid: string;

    const studyName = StudyTestData.StudyName + Utils.generateGUID();


    await test.step('Create a study & Validate study state', async () => {
        await projectService.addScripterUserToProject();
        await studyService.CreateNewStudy(studyName, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await webHelper.handleConfirmationPopup();
        await studyService.ValidateInitialDraftStateStudy();
        await webHelper.saveRecord();
    });

    await test.step('Add the Fieldwork Languages', async () => {
        await studyService.AddFieldWorkMarketLanguage(StudyTestData.Langauge1);
    });

    await test.step('Update study from Draft to Ready for Scritping', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
    });

    await test.step('Validate the Readonly field in the Ready for Scripting study', async () => {
        await studyService.ValidateReadOnlyFieldsInStudy();
    });

    await test.step('Click on the Rework button', async () => {
        await studyService.ValidateAndClickOnReworkbutton();
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();

    });

    await test.step('Validate the Readonly field in the Rework study', async () => {
        await studyService.ValidateReadOnlyFieldsInStudy();
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();
    });

    await test.step('Create a new study', async () => {
        await studyService.CreateNewStudy(studyName, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await webHelper.handleConfirmationPopup();
        await studyService.ValidateInitialDraftStateStudy();
    });

    await test.step('Add the Fieldwork Languages', async () => {
        await studyService.AddFieldWorkMarketLanguage(StudyTestData.Langauge1);
    });

    await test.step('Update new study from Draft to Ready for Scripting', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
    });

    await test.step('Change the study state to Approved for Launch and save the study', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ApprovedforLaunch);
    });

    await test.step('Validate the Readonly field in the Approved for Launch study', async () => {
        await studyService.ValidateReadOnlyFieldsInStudy();
    });

    await test.step('Click on the Complete Study button', async () => {
        await studyService.ValidateAndClickOnCompleteStudybutton();
    });

    await test.step('Validate the Readonly field in the Completed study', async () => {
        await studyService.ValidateReadOnlyFieldsInStudy();
        await page.goBack();
    });

    await test.step('Create another new study', async () => {
        await studyService.CreateNewStudy(studyName, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await webHelper.handleConfirmationPopup();
        await studyService.ValidateInitialDraftStateStudy();
    });

    await test.step('Click on the Abandon button and click on Yes on the confirmation popup', async () => {
        await studyService.ValidateAndClickOnAbandonButton();
    });

    await test.step('Validate the Readonly field in the Abandon study', async () => {
        await studyService.ValidateReadOnlyFieldsInStudy();
        await webHelper.saveAndCloseRecord();
    });

});
