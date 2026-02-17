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
import { TestData } from '../../Test Data/ProjectData.json';
import { EntityLogicalNames } from '../../constants/CommonLogicalNames.json';
import { Common } from '../../selectors/CommonSelector.json';
import { Questionnaireservice } from '../../services/QuestionnaireService';
import { TestData as QuestTestData } from '../../Test Data/QuestionnnaireData.json';

// Using a LoginFixture and CreateProjectWithProduct to handle setup and teardown for all test cases:
// 1. Login the application with CS user
// 2. Create the project and add a product to the project before each test execution.
// 3. Delete the project after the each test case execution


// Merges the results of the login test and the add product test into a single combined test execution.
const test = mergeTests(logintest, addproduct);
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

test("[2385415,2385416,2377758] Verify Study Change Logs - Create new study version, deactivate questionnaire line, status reason, version number, questionnaire lines & buttons", { tag: ['@Regression', '@Study'] }, async ({ page, browser, loginPage, guid, projectName }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2385415' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2385416' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2377758' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);
    const newStudyVersionName = studyName + '_New Version2';


    await test.step('Validate button visibility when study is in Ready for Scripting state', async () => {
        const visibleButtonsOne: string[] = StudySelectors.ExpectedResults.DraftStudyButtons;
        await studyService.ValidateStudyButtons(visibleButtonsOne, 'Visible');
        const visibleButtonsTwo: string[] = StudySelectors.ExpectedResults.DraftStudyHiddenButtons;
        await studyService.ValidateStudyButtons(visibleButtonsTwo, 'Visible');
    });

    await studyService.createNewStudyVersion(newStudyVersionName);
    await webHelper.handleConfirmationPopup();
    await studyService.goToProject(projectName);
    await projectService.expandFirstQuestionFromQuestionnaire();
    await webHelper.clickonStartWithText("Edit");


    const deactivatedQuestion = await projectService.clickonButton(Common.Text.Deactivate);
    await webHelper.verifySaveButton();
    await page.goBack();
    await webHelper.verifySaveButton();
    await studyService.openStudy(newStudyVersionName);

    await test.step('Update study from Draft to Ready for Scritping', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
        await webHelper.saveRecord();
        await webHelper.clickGoBackArrow();

        await studyService.openStudy(newStudyVersionName);
    });

    const deactivateQuestion = deactivatedQuestion + ',' + 'Question removed';
    const changeLogs: string[] = [deactivateQuestion]
    await studyService.validateStudyChangeLogs(changeLogs);

    await test.step('Validate export to word document functionality', async () => {
        await webHelper.verifySaveButton();
        await studyService.ValidateCreateDocumentFunctionality();
    });

    await test.step('Validate export to XML functionality', async () => {
        await studyService.ValidateXMLExportFunctionality();
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
    await test.step('validate the Change log for the Question Add', async () => {
        await scripterStudyService.validateStudyChangeLogsForAddingQuestions(changeLogs, false);
    });
});


test("[2473640] Validate Study Change Logs - Re-Ordering the Questionnaire", { tag: ['@Regression', '@Study'] }, async ({ page, loginPage, guid, projectName }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2473640' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);

    const newStudyVersionName = studyName + '_New Version2';


    await test.step('create new study version', async () => {
        await studyService.createNewStudyVersion(newStudyVersionName);
        await webHelper.handleConfirmationPopup();
    });

    await test.step('change the order of Questionnaire', async () => {
        await studyService.goToProject(projectName);
    });
    const draganddropedQuestion = await projectService.dragAndDropTheMasterQuestionnaireLines();

    await studyService.openStudy(newStudyVersionName);

    await test.step('Update study from Draft to Ready for Scritping and validate the change logs', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
        await webHelper.saveRecord();
        await webHelper.clickGoBackArrow();
        await studyService.openStudy(newStudyVersionName);
        await page.waitForTimeout(20000);
    });

    const firstrowQuestion = draganddropedQuestion + ',' + 'Question Reordered'
    const changeLogs: string[] = [firstrowQuestion]
    await studyService.validateStudyChangeLogs(changeLogs);

});

test("[2377721] Verify Study Change Logs - Add Question", { tag: ['@Regression', '@Study'] }, async ({ page, browser, loginPage, guid, projectName }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2377721' });

    const studyService = new StudyService(page);
    const questionnaireService = new Questionnaireservice(page);

    const webHelper = new WebHelper(page);
    const newStudyVersionName = studyName + '_New Version2';

    let changeLogs: string[];

    await test.step('Update the Study Version', async () => {
        await studyService.createNewStudyVersion(newStudyVersionName);
        await webHelper.handleConfirmationPopup();
    });
    await test.step('Add the Question to the Questionnaire', async () => {
        await studyService.goToProject(projectName);
        await questionnaireService.addQuestion(DropDownList.Questions.FAM_TRIED, QuestTestData.FAM_QuestionTitle);
    });

    await test.step('Update study from Draft to Ready for Scritping', async () => {
        await studyService.openStudy(newStudyVersionName);
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
        await webHelper.saveRecord();
        await webHelper.clickGoBackArrow();
        await studyService.openStudy(newStudyVersionName);
        await webHelper.saveRecord();
        await page.reload();
        await webHelper.saveRecord();
    });

    await test.step('validate the Change log for the Question Add', async () => {
        const addedQuestion = DropDownList.Questions.FAM_TRIED + ',' + 'Question added'
        changeLogs = [addedQuestion]
        await studyService.validateStudyChangeLogsForAddingQuestions(changeLogs);

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
    await test.step('validate the Change log for the Question Add', async () => {
        await scripterStudyService.validateStudyChangeLogsForAddingQuestions(changeLogs);
    });
});

test("[2379481,2377781] Verify Study Change Logs - Add Answer Change log", { tag: ['@Regression', '@Study'] }, async ({ page, browser, loginPage, guid, projectName }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2379481' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2377781' });


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
    await webHelper.clickonStartWithText("Edit");


    const questionVariableName = await projectService.navigateToAnswersTabFromMasterQuestionnaireLines();

    await test.step('Add the Answer to the list', async () => {
        await projectService.AddAnswerToTheList(TestData.AnswerText, TestData.AnswerCode);
        await webHelper.saveAndCloseRecord();
        await studyService.openStudy(newStudyVersionName);
    });
    await test.step('Update study from Draft to Ready for Scritping', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
        await webHelper.saveRecord();
        await webHelper.clickGoBackArrow();
        await studyService.openStudy(newStudyVersionName);
        await webHelper.verifySaveButton();
    });

    const addedAnswer = questionVariableName + ',' + 'Answer added';
    const changeLogs: string[] = [addedAnswer];
    await test.step('validate the Change log for the Question Add', async () => {
        await studyService.validateStudyChangeLogs(changeLogs);

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
    await test.step('validate the Change log for the Question Add', async () => {
        await scripterStudyService.validateStudyChangeLogsForAddingQuestions(changeLogs, false);
    });

});

test("[2379483,2377780] Verify Study Change Logs -Remove Answer change log", { tag: ['@Regression', '@Study'] }, async ({ page, browser, loginPage, guid, projectName }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2379483' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2377780' });


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

    const questionVariableName = await projectService.navigateToAnswersTabFromMasterQuestionnaireLines();

    await test.step('Deactivate the existing answer', async () => {
        await projectService.openAnswersListInGrid();
        await projectService.deactiavteTheAnswer(StudySelectors.Button.Deactivate);
        await webHelper.saveRecord();
        await webHelper.saveAndCloseRecord();
        await studyService.openStudy(newStudyVersionName);

    });
    await test.step('Update study from Draft to Ready for Scritping', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
    });
    const removedAnswer = questionVariableName + ',' + 'Answer removed';
    const changeLogs: string[] = [removedAnswer]
    await test.step('validate the Change log for the Question Add', async () => {
        await studyService.validateStudyChangeLogs(changeLogs);

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
    await test.step('validate the Change log for the Question Add', async () => {
        await scripterStudyService.validateStudyChangeLogsForAddingQuestions(changeLogs, false);
    });

});

test("[2379482] Verify updated answers are correctly updated in Change Log tab for a new version of study(Ready for Scripting->New Version)", { tag: ['@Regression', '@Study'] }, async ({ page, loginPage, guid, projectName }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2379482' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);
    const questionnaireService = new Questionnaireservice(page);

    const newStudyVersionName = studyName + '_New Version2';
    var questionVariableName = "";


    await test.step('Update any of the answers', async () => {
        await studyService.goToProject(projectName);
        await projectService.expandFirstQuestionFromQuestionnaire();
        await questionnaireService.clickonEditbutton();

        questionVariableName = await projectService.navigateToAnswersTabFromMasterQuestionnaireLines();
        await questionnaireService.clickOnAnswerText();
        await questionnaireService.updateAnswerText(TestData.AnswerText);
        await webHelper.saveAndCloseRecord();
    });

    await test.step('Update the Study Version', async () => {
        await studyService.openStudy(studyName);
        await studyService.createNewStudyVersion(newStudyVersionName);
        await webHelper.saveRecord();
    });
    await test.step('Update study from Draft to Ready for Scritping', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
    });

    await test.step('validate the Change log for the updating Answer Field', async () => {
        const removedAnswer = questionVariableName + ',' + 'Field Change (Answer)';
        const changeLogs: string[] = [removedAnswer]
        await studyService.validateStudyChangeLogs(changeLogs);
    });

    await test.step('Clean up created project record', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});

test("[2377768] Verify creation of the changelog record for field change - Question Text", { tag: ['@Regression', '@Study'] }, async ({ page, browser, loginPage, guid, projectName }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2377768' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const questionnaireService = new Questionnaireservice(page);

    const webHelper = new WebHelper(page);

    const newStudyVersionName = studyName + '_New Version2';


    await test.step('Update the Study Version', async () => {
        await studyService.createNewStudyVersion(newStudyVersionName);
        await webHelper.handleConfirmationPopup();
    });

    await studyService.goToProject(projectName);

    await projectService.expandFirstQuestionFromQuestionnaire();
    await webHelper.clickonStartWithText("Edit")

    const questionVariableName = await projectService.getQuestionVariableName();

    await test.step('Update the Question Text field for the question in the master questionnaire', async () => {
        await questionnaireService.ModifyQuestionDetails(QuestTestData.StandardQuestionText1, QuestTestData.Custom);
        await webHelper.saveAndCloseRecord();
        await studyService.openStudy(newStudyVersionName);

    });
    await test.step('Update study from Draft to Ready for Scritping', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
    });
    const removedAnswer = questionVariableName + ',' + 'Field Change (Question)';
    const changeLogs: string[] = [removedAnswer]
    await test.step('validate the Change log for the field change - Question Text', async () => {
        await studyService.validateStudyChangeLogs(changeLogs);

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
    await test.step('validate the Change log for the field change - Question Text', async () => {
        await scripterStudyService.validateStudyChangeLogsForAddingQuestions(changeLogs, false);
    });

});
test("[2377777] Verify creation of the changelog record for field change - Scripter Notes", { tag: ['@Regression', '@Study'] }, async ({ page, browser, loginPage, guid, projectName }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2377777' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const questionnaireService = new Questionnaireservice(page);

    const webHelper = new WebHelper(page);

    const newStudyVersionName = studyName + '_New Version2';


    await test.step('Update the Study Version', async () => {
        await studyService.createNewStudyVersion(newStudyVersionName);
        await webHelper.handleConfirmationPopup();
    });

    await studyService.goToProject(projectName);

    await projectService.expandFirstQuestionFromQuestionnaire();
    await webHelper.clickonStartWithText("Edit")

    const questionVariableName = await projectService.getQuestionVariableName();

    await test.step('Update the Scripter Notes for the question in the master questionnaire', async () => {
        await questionnaireService.ModifyQuestionScripterNotes(QuestTestData.ScripterNotes);
        await webHelper.saveAndCloseRecord();
        await studyService.openStudy(newStudyVersionName);

    });
    await test.step('Update study from Draft to Ready for Scritping', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
    });
    const removedAnswer = questionVariableName + ',' + 'Field Change (Question)';
    const changeLogs: string[] = [removedAnswer]
    await test.step('validate the Change log for field change - Scripter Notes', async () => {
        await studyService.validateStudyChangeLogs(changeLogs);

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
    await test.step('validate the Change log for the field change - Scripter Notes', async () => {
        await scripterStudyService.validateStudyChangeLogsForAddingQuestions(changeLogs, false);
    });

});
test("[2384674] Verify creation of the changelog record for adding module", { tag: ['@Regression', '@Study'] }, async ({ page, browser, loginPage, guid, projectName }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2384674' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const questionnaireService = new Questionnaireservice(page);

    const webHelper = new WebHelper(page);

    const newStudyVersionName = studyName + '_New Version2';


    await test.step('Update the Study Version', async () => {
        await studyService.createNewStudyVersion(newStudyVersionName);
        await webHelper.handleConfirmationPopup();
    });

    await studyService.goToProject(projectName);

    await test.step('Add a module in the master questionnaire', async () => {
        await questionnaireService.addModule(QuestTestData.ModuleName);
        await webHelper.saveRecord();
        await studyService.openStudy(newStudyVersionName);

    });
    await test.step('Update study from Draft to Ready for Scritping', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
    });
    const removedAnswer = QuestTestData.ModuleName + ',' + QuestTestData.M_QuestionName + ',' + 'Module added';
    const changeLogs: string[] = [removedAnswer]
    await test.step('validate the Change log for the Question Add', async () => {
        await studyService.validateStudyChangeLogsForModuleChange(changeLogs);

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
    await test.step('validate the Change log for the Module Add', async () => {
        await scripterStudyService.validateStudyChangeLogsForModuleChange(changeLogs, false);
    });

});

test("[2384676] Verify creation of the changelog record for removing module", { tag: ['@Regression', '@Study'] }, async ({ page, browser, loginPage, guid, projectName }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2384676' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const questionnaireService = new Questionnaireservice(page);

    const webHelper = new WebHelper(page);

    const newStudyVersionName = studyName + '_New Version2';

    await test.step('Update the Study Version', async () => {
        await studyService.createNewStudyVersion(newStudyVersionName);
        await webHelper.handleConfirmationPopup();
    });

    await studyService.goToProject(projectName);

    await test.step('Remove a module in the master questionnaire', async () => {
        await questionnaireService.removeModuleFromQuestionnaire(QuestTestData.ModuleName6);
        await webHelper.saveRecord();
        await studyService.openStudy(newStudyVersionName);

    });
    await test.step('Update study from Draft to Ready for Scritping', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
    });
    const removedAnswer = QuestTestData.ModuleName6 + ',' + QuestTestData.ModuleName6_Question1 + ',' + 'Module removed';
    const changeLogs: string[] = [removedAnswer]
    await test.step('validate the Change log for the Question Add', async () => {
        await studyService.validateStudyChangeLogsForModuleChange(changeLogs);

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
    await test.step('validate the Change log for the Module Add', async () => {
        await scripterStudyService.validateStudyChangeLogsForModuleChange(changeLogs, false);
    });

});



