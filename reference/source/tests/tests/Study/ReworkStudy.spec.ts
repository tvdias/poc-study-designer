import { expect, mergeTests } from '@playwright/test';
import { logintest } from '../Fixture/LoginFixture';
import { addproduct } from '../Fixture/CreateProjectWithProduct';
import { ProjectService } from '../../services/ProjectService';
import { LoginToMDAWithTestUser, waitUntilAppIdle } from '../../utils/Login';
import { Utils } from "../../utils/utils";
import { TestUser, AppName, AppId } from '../../constants/TestUsers.json';
import { StudyTestData } from '../../Test Data/StudyData.json';
import { DropDownList } from '../../constants/DropDownList.json';
import { StudyService } from '../../services/StudyService';
import { WebHelper } from '../../utils/WebHelper';
import { deleteRecord } from '../../utils/APIHelper';
import { StudySelectors } from '../../selectors/StudySelectors.json';

// Using a LoginFixture and CreateProjectWithProduct to handle setup and teardown for all test cases:
// 1. Login the application with CS user
// 2. Create the project and add a product to the project before each test execution.
// 3. Delete the project after the each test case execution

// Merges the results of the login test and the add product test into a single combined test execution.
const test = mergeTests(logintest, addproduct);

test("[2520718] Verify Rework  study state - Check State, Version, Buttons, Create Document, Export XML", { tag: ['@Regression', '@Study'] }, async ({ page, loginPage, guid, projectName }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2520718' });

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
        await webHelper.saveRecord();

    });

    await test.step('Change the Study state to Ready for Scripting and save the study', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
    });

    await test.step('Verify that the Rework button is displayed for Ready for Scripting study', async () => {
        const visibleButtonsOne: string[] = StudySelectors.ExpectedResults.DraftStudyHiddenButtons;
        await studyService.ValidateStudyButtons(visibleButtonsOne, 'Visible');
        await webHelper.verifySaveButton();
    });
    await test.step('Click on the Rework button and verify that a new study is created with draft state  and Version number', async () => {
        await studyService.ValidateAndClickOnReworkbutton();
        await studyService.ValidateStudyStatusReason(DropDownList.Status.Draft);
        await studyService.ValidateStudyVersionNumber(StudySelectors.VersionNumber.Two);
        await webHelper.saveRecord();
        await page.goBack();
        await webHelper.verifySaveButton();

    });

    await test.step('Validate the old study state is set to Rework and  study version', async () => {
        await studyService.ValidateStudyVersionNumber(StudySelectors.VersionNumber.One);
        await studyService.ValidateStudyStatusReason(DropDownList.Status.Rework);
    });

    await test.step('Validate button visibility when study is in Rework state', async () => {
        const visibleButtonsOne: string[] = StudySelectors.ExpectedResults.ReworkStudyButtons;
        await studyService.ValidateStudyButtons(visibleButtonsOne, 'Visible');
    });

    await test.step('Validate export to word document functionality', async () => {
        await studyService.ValidateCreateDocumentFunctionality();
    });

    await test.step('Validate export to XML functionality', async () => {
        await studyService.ValidateXMLExportFunctionality();
    });
});

