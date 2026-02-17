import { expect, mergeTests } from '@playwright/test';
import { logintest } from '../Fixture/LoginFixture';
import { addproduct } from '../Fixture/CreateProjectWithProduct';
import { ProjectService } from '../../services/ProjectService';
import { Utils } from "../../utils/utils";
import { TestUser, AppName, AppId } from '../../constants/TestUsers.json';
import { StudyTestData } from '../../Test Data/StudyData.json'
import { DropDownList } from '../../constants/DropDownList.json';
import { StudyService } from '../../services/StudyService';
import { WebHelper } from '../../utils/WebHelper';
import { deleteRecord } from '../../utils/APIHelper';
import { StudySelectors } from '../../selectors/StudySelectors.json'

// Using a LoginFixture and CreateProjectWithProduct to handle setup and teardown for all test cases:
// 1. Login the application with CS user
// 2. Create the project and add a product to the project before each test execution.
// 3. Delete the project after the each test case execution

// Merges the results of the login test and the add product test into a single combined test execution.
const test = mergeTests(logintest, addproduct);

test("[2459171,2504994,2359679] Verify Abandon study state - Add scripter user to the project, Check State, Version, Buttons, Create Document, Export XML", { tag: ['@Regression', '@Study'] }, async ({ page,loginPage, guid, projectName}) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2459171' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2504994' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2359679' });

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

    });
    await test.step('Verify that the Abandon button is displayed for Draft study state', async () => {
        await studyService.ValidateInitialDraftStateStudy();
        const visibleButtonsOne: string[] = StudySelectors.ExpectedResults.DraftStudyButtons;
        await studyService.ValidateStudyButtons(visibleButtonsOne, 'Visible');
    });
    await test.step('Update study from Draft to Ready for Scritping', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
    });
    await test.step('Click on the Abandon button and click on Yes on the confirmation popup', async () => {
        await studyService.ValidateAndClickOnAbandonButton();
    });

    await test.step('Validate study version & status reason as Abandon', async () => {
        await studyService.ValidateStudyVersionNumber(StudySelectors.VersionNumber.One);
        await studyService.ValidateStudyStatusReason(DropDownList.Status.Abandon);
    });
    await test.step('Check study record is in inactive status & ready only', async () => {
        await studyService.ValidateReadOnlyFieldsInStudy();
    });

    await test.step('Validate button visibility when study is in Abandon state', async () => {
        const visibleButtonsOne: string[] = StudySelectors.ExpectedResults.AbandonStudyButtons;
        await studyService.ValidateStudyButtons(visibleButtonsOne, 'Visible');
    });

    await test.step('Validate export to XML functionality', async () => {
        await studyService.ValidateXMLExportFunctionality();
    });
    await test.step('Validate export to word document functionality', async () => {
        await studyService.ValidateCreateDocumentFunctionality();
    });
});

test("[2504992] Verify that study can be abandoned successfully when study is in Ready for Scripting & Draft state", { tag: ['@Regression', '@Study'] }, async ({ page,loginPage, guid, projectName }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2504992' });

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

    });

    await test.step('Click on the Abandon button and click on Yes on the confirmation popup', async () => {
        await studyService.ValidateAndClickOnAbandonButton();
    });

    await test.step('Validate study version & status reason as Abandon', async () => {
        await studyService.ValidateStudyVersionNumber(StudySelectors.VersionNumber.One);
        await studyService.ValidateStudyStatusReason(DropDownList.Status.Abandon);
    });
});

