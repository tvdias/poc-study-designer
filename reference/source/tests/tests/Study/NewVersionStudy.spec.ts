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
import { StudySelectors } from '../../selectors/StudySelectors.json';
import { EntityLogicalNames } from '../../constants/CommonLogicalNames.json';


// Using a LoginFixture and CreateProjectWithProduct to handle setup and teardown for all test cases:
// 1. Login the application with CS user
// 2. Create the project and add a product to the project before each test execution.
// 3. Delete the project after the each test case execution

// Merges the results of the login test and the add product test into a single combined test execution.
const test = mergeTests(logintest, addproduct);

test("[2375193,2360346] Create a new version of study from existing study", { tag: ['@Regression', '@Study'] }, async ({ page,loginPage, guid, projectName }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2360346' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2375193' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);

    const studyName = StudyTestData.StudyName + Utils.generateGUID();
    const newStudyVersionName = studyName + '_New Version2';

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
    });
    await test.step('Verify that there are already snapshot records created ', async () => {
        await studyService.ValidateStudySnapshotQuestions();
    });
    await test.step('Update the Study Version', async () => {
        await studyService.createNewStudyVersion(newStudyVersionName);
        await webHelper.saveRecord();
    });
    await test.step('Validate study version & status reason', async () => {
        await studyService.ValidateStudyVersionNumber(StudySelectors.VersionNumber.Two);
        await studyService.ValidateStudyStatusReason(DropDownList.Status.Draft);
    });
    await test.step('Validate export to word document functionality', async () => {
        await studyService.ValidateCreateDocumentFunctionality();
    });

    await test.step('Verify that the existing snapshot records are deleted ', async () => {
        await studyService.ValidateStudySnapshotQuestionsDeleted();
    });
    await test.step('Clean up created project record', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});

