import { expect, mergeTests } from '@playwright/test';
import { logintest } from '../Fixture/LoginFixture';
import { addproduct } from '../Fixture/CreateProjectWithProduct';
import { ProjectService } from '../../services/ProjectService';
import { Utils } from "../../utils/utils";
import { StudyTestData } from '../../Test Data/StudyData.json'
import { DropDownList } from '../../constants/DropDownList.json';
import { StudyService } from '../../services/StudyService';
import { WebHelper } from '../../utils/WebHelper';
import { deleteRecord } from '../../utils/APIHelper';
import { StudySelectors } from '../../selectors/StudySelectors.json'
import { TestData } from '../../Test Data/ProjectData.json';
import { EntityLogicalNames } from '../../constants/CommonLogicalNames.json';

// Using a LoginFixture and CreateProjectWithProduct to handle setup and teardown for all test cases:
// 1. Login the application with CS user
// 2. Create the project and add a product to the project before each test execution.
// 3. Delete the project after the each test case execution

// Merges the results of the login test and the add product test into a single combined test execution.
const test = mergeTests(logintest, addproduct);

test("[2359676] Verify Approved for launch study - Add scripter user to the project, status reason and buttons", { tag: ['@Regression', '@Study'] }, async ({ page,loginPage, guid, projectName }) => {

    test.info().annotations.push({ type: 'TestCaseId', description: '2359676' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);
    const studyName = StudyTestData.StudyName + Utils.generateGUID();



    await test.step('Create a study & Validate study state', async () => {
        await projectService.addScripterUserToProject();
        await studyService.CreateNewStudy(studyName, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await webHelper.handleConfirmationPopup();
        await studyService.ValidateInitialDraftStateStudy();
    });

    await test.step('Update study from Draft to Ready for Scritping', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
    });

    await test.step('Validate study version & status reason', async () => {
        await studyService.ValidateStudyVersionNumber(StudySelectors.VersionNumber.One);
        await studyService.ValidateStudyStatusReason(DropDownList.Status.ReadyForScripting);
        await webHelper.saveRecord();
        await webHelper.clickGoBackArrow();
    });

    await test.step('Update study from Ready for Scritping to Approve for launch', async () => {
        await studyService.openStudy(studyName);
        await studyService.updateStudyStatus(DropDownList.Status.ApprovedforLaunch);
    });

    await test.step('Validate study version & status reason', async () => {
        await studyService.ValidateStudyVersionNumber(StudySelectors.VersionNumber.One);
        await studyService.ValidateStudyStatusReason(DropDownList.Status.ApprovedforLaunch);
    });

    await test.step('Check study record becomes read only except Status Reason field', async () => {
        await studyService.ValidateReadOnlyFieldsInStudy();
    });

    await test.step('Validate button visibility when study is in Approved for Launch state', async () => {
        const visibleButtonsOne: string[] = StudySelectors.ExpectedResults.ApprovedforLaunch;
        await studyService.ValidateStudyButtons(visibleButtonsOne, 'Visible');
    });
    await test.step('Validate export to word document functionality', async () => {
        await studyService.ValidateCreateDocumentFunctionality();
    });

    await test.step('Validate export to XML functionality', async () => {
        await studyService.ValidateXMLExportFunctionality();
    });
});

