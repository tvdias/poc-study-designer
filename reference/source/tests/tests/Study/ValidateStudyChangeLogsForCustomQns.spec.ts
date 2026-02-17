import { expect, mergeTests } from '@playwright/test';
import { logintest } from '../Fixture/LoginFixture';
import { createproject } from '../Fixture/CreateProjectWithCustomQue';
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
import { TestData } from '../../Test Data/ProjectData.json';
import { EntityLogicalNames } from '../../constants/CommonLogicalNames.json';
import { Common } from '../../selectors/CommonSelector.json';
import { Questionnaireservice } from '../../services/QuestionnaireService';
import { TestData as QuestTestData } from '../../Test Data/QuestionnnaireData.json';

// Using a LoginFixture and CreateProjectWithCustomQue to handle setup and teardown for all test cases:
// 1. Login the application with CS user
// 2. Create the project and add a custom Question to the project before each test execution.
// 3. Delete the project after the each test case execution


// Merges the results of the login test and the create a project and add custom Question into a single combined test execution.
const test = mergeTests(logintest, createproject);
let studyName = "";

//  Executes setup steps before each test case to prepare the required environment.
test.beforeEach(async ({ page, loginPage, guid, projectName }) => {

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);
    studyName = StudyTestData.StudyName + Utils.generateGUID();


    const masterQuestionnaireLinesCount = await projectService.getMasterQuestionnaireLinesCount();

    await test.step('Create a study & Compare questionnaire lines with master questionnaire lines', async () => {
        await projectService.addScripterUserToProject();

        await studyService.CreateNewStudy(studyName, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await webHelper.handleConfirmationPopup();
        await studyService.ValidateInitialDraftStateStudy();
    });

    await test.step('Update study from Draft to Ready for Scritping', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
    });

    const studyQuestionsCount = await studyService.getStudyQuestionnaireLinesCount();
    expect(masterQuestionnaireLinesCount).toBe(studyQuestionsCount);

    await test.step('Validate study version & status reason', async () => {
        await studyService.ValidateStudyVersionNumber(StudySelectors.VersionNumber.One);
        await studyService.ValidateStudyStatusReason(DropDownList.Status.ReadyForScripting);
    });

});

test("[2377775] Verify creation of the changelog record for Custom Question field change - Question Variable Name", { tag: ['@Regression', '@Study'] }, async ({ page, browser, loginPage, guid, projectName }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2377775' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);

    const newStudyVersionName = studyName + '_New Version2';


    await test.step('Update the Study Version', async () => {
        await studyService.createNewStudyVersion(newStudyVersionName);
        await webHelper.handleConfirmationPopup();
    });

    await studyService.goToProject(projectName);

    await projectService.expandFirstQuestionFromQuestionnaire();
    await webHelper.clickonStartWithText("Edit")

    await test.step('Update the Question Text field for the question in the master questionnaire', async () => {
        await projectService.updateQuestionVariableName(QuestTestData.Question);
        await webHelper.saveAndCloseRecord();
        await studyService.openStudy(newStudyVersionName);

    });
    await test.step('Update study from Draft to Ready for Scritping', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
    });
    const fieldChange = QuestTestData.Question.toUpperCase() + ',' + "GENDER" + ',' + 'Field Change (Question)' + ',' + 'Question - Question Variable Name';
    const changeLogs: string[] = [fieldChange]
    await test.step('validate the Change log for the Custom Question field change - Question Variable Name', async () => {
        await studyService.validateStudyChangeLogsForQuestionFieldChange(changeLogs);

    });
    const context = await browser.newContext();
    const scripterUserPage = await context.newPage();
    const newProjectService = new ProjectService(scripterUserPage);
    const scripterStudyService = new StudyService(scripterUserPage);

    await test.step('Login as a scripter user', async () => {
        await LoginToMDAWithTestUser(scripterUserPage, TestUser.ScripterUser, AppId.UC1, AppName.UC1, true);
    });
    await test.step('Go to project > search for the shared project', async () => {
        await newProjectService.searchForProject(projectName);
    });
    await test.step('Open the Study', async () => {
        await scripterStudyService.openStudy(newStudyVersionName);
    });
    await test.step('validate the Change log for the  Custom Question field change - Question Variable Name', async () => {
        await scripterStudyService.validateStudyChangeLogsForQuestionFieldChange(changeLogs, false);
    });

});
test("[2377769] Verify creation of the changelog record for Custom Question field change - Question Title", { tag: ['@Regression', '@Study'] }, async ({ page, browser, loginPage, guid, projectName }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2377769' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);
    const newStudyVersionName = studyName + '_New Version2';

    await test.step('Update the Study Version', async () => {
        await studyService.createNewStudyVersion(newStudyVersionName);
        await webHelper.handleConfirmationPopup();
    });

    await studyService.goToProject(projectName);

    await projectService.expandFirstQuestionFromQuestionnaire();
    await webHelper.clickonStartWithText("Edit")

    await test.step('Update the Question Text field for the question in the master questionnaire', async () => {
        await projectService.updateQuestionTitle(QuestTestData.Question);
        await webHelper.saveAndCloseRecord();
        await studyService.openStudy(newStudyVersionName);

    });
    await test.step('Update study from Draft to Ready for Scritping', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
    });
    const fieldChange = "GENDER" + ',' + "GENDER" + ',' + 'Field Change (Question)' + ',' + 'Question - Question Title';
    const changeLogs: string[] = [fieldChange]
    await test.step('validate the Change log for the Custom Question field change - Question Title', async () => {
        await studyService.validateStudyChangeLogsForQuestionFieldChange(changeLogs);

    });
    const context = await browser.newContext();
    const scripterUserPage = await context.newPage();
    const newProjectService = new ProjectService(scripterUserPage);
    const scripterStudyService = new StudyService(scripterUserPage);

    await test.step('Login as a scripter user', async () => {
        await LoginToMDAWithTestUser(scripterUserPage, TestUser.ScripterUser, AppId.UC1, AppName.UC1, true);
    });
    await test.step('Go to project > search for the shared project', async () => {
        await newProjectService.searchForProject(projectName);
    });
    await test.step('Open the Study', async () => {
        await scripterStudyService.openStudy(newStudyVersionName);
    });
    await test.step('validate the Change log for the  Custom Question field change - Question Title', async () => {
        await scripterStudyService.validateStudyChangeLogsForQuestionFieldChange(changeLogs, false);
    });


});




