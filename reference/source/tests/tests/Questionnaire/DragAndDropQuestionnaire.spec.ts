import { expect, test } from '@playwright/test';
import { LoginToMDAWithTestUser, waitUntilAppIdle } from '../../utils/Login';
import { TestUser, AppName, AppId } from '../../constants/TestUsers.json';
import { Utils } from "../../utils/utils";
import { DropDownList } from '../../constants/DropDownList.json';
import { WebHelper } from '../../utils/WebHelper';
import { Common } from '../../selectors/CommonSelector.json';
import { Questionnaire } from '../../selectors/QuestionnaireSelector.json';
import { Questionnaireservice } from '../../services/QuestionnaireService';
import { TestData } from '../../Test Data/QuestionnnaireData.json';
import { deleteRecord } from '../../utils/APIHelper';
import { EntityLogicalNames } from '../../constants/CommonLogicalNames.json';
import { ProjectService } from '../../services/ProjectService';
import { Project } from '../../selectors/ProjectSelectors.json';
import { StudyService } from '../../services/StudyService';
import { StudyTestData } from '../../Test Data/StudyData.json';
import { QuestionBank } from '../../selectors/QuestionBankSelectors.json';

const userroles = [
    { user: TestUser.CSUser, TestcaseId: '2528482' },
    { user: TestUser.LibrarianUser, TestcaseId: '2621308' },
]
// Executes the same test case sequentially in the defined order using both CS user and Librarian user contexts.
test.describe.serial('', () => {
    for (const role of userroles) {

        test(`${role.TestcaseId} Verify that ${role.user} for All Question view, drag and drop is not allowed`, { tag: ['@Regression', '@Study'] }, async ({ page }) => {
            test.info().annotations.push({ type: 'TestCaseId', description: role.TestcaseId });

            const questionnaireService = new Questionnaireservice(page);
            const projectService = new ProjectService(page);

            const questionName = TestData.StandardQuestion;
            const questionTitle = Questionnaire.Text.CategoryIntroduction;
            const webHelper = new WebHelper(page);
            const projectName = "AUTO_TestProject_" + Utils.generateGUID();
            var guid: string = "";

            await test.step('Login into MDA application', async () => {
                await LoginToMDAWithTestUser(page, role.user, AppId.UC1, AppName.UC1);
                await webHelper.changeArea(Common.Text.CSUser);
            });

            await test.step('Go to project > Create a Project', async () => {
                await projectService.CreateProject(projectName);
                guid = await webHelper.fetchRecordGuid(page.url());
            });
            await test.step('Add a Question to the Project', async () => {
                await questionnaireService.addQuestion(questionName, questionTitle);
                await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuestionText2, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuest_VariableName1);
            });
            await test.step('Add a Module to the Project', async () => {
                await questionnaireService.addModule(TestData.ModuleName);
                await projectService.verifydragAndDropTheMasterQuestionnaireLines();
            });

            await test.step('Change to AllActive Questions', async () => {
                await webHelper.selectByOption(Common.CSS.ActiveQuestions, TestData.AllQuestions);
                await projectService.verifydragAndDropTheMasterQuestionnaireLines(false);
            });
            await test.step('Clean up created project record', async () => {
                await deleteRecord(EntityLogicalNames.Projects, guid);
            });
        });

    }
});

const roles = [
    { user: TestUser.CSUser, TestcaseId: '2528483' },
    { user: TestUser.LibrarianUser, TestcaseId: '2621315' },
]
// Executes the same test case sequentially in the defined order using both CS user and Librarian user contexts.
test.describe.serial('', () => {
    for (const role of roles) {

        test(`${role.TestcaseId} Verify that ${role.user} the order of the question when the question is drag and dropped.`, { tag: ['@Regression', '@Questionnaire'] }, async ({ page }) => {
            test.info().annotations.push({ type: 'TestCaseId', description: role.TestcaseId });
            const questionnaireService = new Questionnaireservice(page);
            const projectService = new ProjectService(page);

            const questionName = TestData.StandardQuestion;
            const questionTitle = Questionnaire.Text.CategoryIntroduction;
            const webHelper = new WebHelper(page);
            const projectName = "AUTO_TestProject_" + Utils.generateGUID();
            var guid: string = "";
            let masterQuestionnaireLinesVariableNames: string[] = [];
            let QuestionnaireLinesVariableNames: string[] = [];


            await test.step('Login into MDA application', async () => {
                await LoginToMDAWithTestUser(page, role.user, AppId.UC1, AppName.UC1);
                await webHelper.changeArea(Common.Text.CSUser);
            });

            await test.step('Go to project > Create a Project', async () => {
                await projectService.CreateProject(projectName);
                guid = await webHelper.fetchRecordGuid(page.url());
            });

            await test.step('Add a Question to the Project', async () => {
                await questionnaireService.addQuestion(questionName, questionTitle);
                await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuestionText2, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuest_VariableName1);
            });
            await test.step('Add a Module to the Project', async () => {
                await questionnaireService.addModule(TestData.ModuleName);
                await projectService.verifydragAndDropTheMasterQuestionnaireLines();
                masterQuestionnaireLinesVariableNames = await projectService.getQuestionnaireLinesVariableName();
            });

            await test.step('Drag and drop the questions', async () => {
                await projectService.dragAndDropTheMasterQuestionnaireLines();
                QuestionnaireLinesVariableNames = await projectService.getQuestionnaireLinesVariableName();
            });
            await test.step('Verify the order of the question when the question is drag and dropped.', async () => {
                await projectService.validateQuestionnairesAfterReOrder(masterQuestionnaireLinesVariableNames, QuestionnaireLinesVariableNames);
            });

            await test.step('Clean up created project record', async () => {
                await deleteRecord(EntityLogicalNames.Projects, guid);
            });
        });

    }
});

test("[2528489] Verify that the drag & drop functionality is not allowed for Scripter user", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2528489' });
    const questionnaireService = new Questionnaireservice(page);
    const projectService = new ProjectService(page);

    const questionName = TestData.StandardQuestion;
    const questionTitle = Questionnaire.Text.CategoryIntroduction;
    const webHelper = new WebHelper(page);
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();
    var guid = "";

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.CSUser);
    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
        guid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Add a Question to the Project', async () => {
        await questionnaireService.addQuestion(questionName, questionTitle);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuestionText2, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuest_VariableName1);

    });
    await test.step('Add a Module to the Project', async () => {
        await questionnaireService.addModule(TestData.ModuleName);
        await projectService.verifydragAndDropTheMasterQuestionnaireLines();
    });


    await test.step('Now share a project to scripter user from User management', async () => {
        await projectService.addScripterUserToProject();
        await webHelper.saveRecord();
        await page.close();
    });

    const context = await browser.newContext();
    const scripterUserPage = await context.newPage();
    const webHelperSecond = new WebHelper(scripterUserPage);
    const newProjectService = new ProjectService(scripterUserPage);
    const scripterUserQuestionnaireService = new Questionnaireservice(scripterUserPage);
    await test.step('Login as a scripter user', async () => {
        await LoginToMDAWithTestUser(scripterUserPage, TestUser.ScripterUser, AppId.UC1, AppName.UC1, true);
    });
    await test.step('Go to project > search for the shared project', async () => {
        await newProjectService.searchForProject(projectName);
    });

    await test.step('Navigate to Questionnaire tab>Active view', async () => {
        await scripterUserQuestionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuestionText2, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuest_VariableName1);

    });

    await test.step('Verify that the drag & drop functionality is not allowed for Scripter user ', async () => {
        await newProjectService.verifydragAndDropTheMasterQuestionnaireLines(false);
    });

    await test.step('Delete the Created Project ', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});

test("[2528485] Verify that when a question is drag and dropped in the UMQ as a CS user, it gets updated for Scripter and Librarian user as well", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2528485' });
    const questionnaireService = new Questionnaireservice(page);
    const projectService = new ProjectService(page);

    const questionName = TestData.StandardQuestion;
    const questionTitle = Questionnaire.Text.CategoryIntroduction;
    const webHelper = new WebHelper(page);
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();
    var guid = "";
    let masterQuestionnaireLinesVariableNames: string[] = [];
    let QuestionnaireLinesVariableNames: string[] = [];

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.CSUser);
    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
        guid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Add a Question to the Project', async () => {
        await questionnaireService.addQuestion(questionName, questionTitle);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuestionText2, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuest_VariableName1);

    });

    await test.step('Add a Module to the Project', async () => {
        await questionnaireService.addModule(TestData.ModuleName);
        await projectService.verifydragAndDropTheMasterQuestionnaireLines();
        masterQuestionnaireLinesVariableNames = await projectService.getQuestionnaireLinesVariableName();
    });

    await test.step('Drag and drop the questions', async () => {
        await projectService.dragAndDropTheMasterQuestionnaireLines();
        QuestionnaireLinesVariableNames = await projectService.getQuestionnaireLinesVariableName();
    });
    await test.step('Verify the order of the question when the question is drag and dropped.', async () => {
        await projectService.validateQuestionnairesAfterReOrder(masterQuestionnaireLinesVariableNames, QuestionnaireLinesVariableNames);
    });

    await test.step('Now share a project to  Librarian user and scripter user from User management', async () => {
        await projectService.addScripterUserToProject(Project.ByRole.LibrarianUserName);
        await webHelper.saveRecord();
        await projectService.addScripterUserToProject();
        await webHelper.saveRecord();
        await page.close();
    });

    const context = await browser.newContext();
    const librarianUserPage = await context.newPage();
    const webHelperLibrarian = new WebHelper(librarianUserPage);
    const newProjectService = new ProjectService(librarianUserPage);
    const librarianQuestionnaireService = new Questionnaireservice(librarianUserPage);

    await test.step('Login as a scripter user', async () => {
        await LoginToMDAWithTestUser(librarianUserPage, TestUser.LibrarianUser, AppId.UC1, AppName.UC1, true);
    });
    await test.step('Go to project > search for the shared project', async () => {
        await webHelperLibrarian.verifyNewButton();
        await webHelperLibrarian.searchAndOpenRecord(projectName);
    });

    await test.step('Navigate to Questionnaire tab>Active view', async () => {
        await librarianQuestionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuestionText2, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuest_VariableName1);
    });
    await test.step('Verify that the question order as a Librarian is similar to that of a CS user', async () => {
        const librarianQuestionnaire = await newProjectService.getQuestionnaireLinesVariableName();
        await newProjectService.validateQuestionnairesAfterReOrder(masterQuestionnaireLinesVariableNames, librarianQuestionnaire);
    });

    const scriptercontext = await browser.newContext();
    const scripterUserPage = await scriptercontext.newPage();
    const scripterProjectService = new ProjectService(scripterUserPage);
    const scripterUserQuestionnaireService = new Questionnaireservice(scripterUserPage);

    await test.step('Login as a scripter user', async () => {
        await LoginToMDAWithTestUser(scripterUserPage, TestUser.ScripterUser, AppId.UC1, AppName.UC1, true);
    });
    await test.step('Go to project > search for the shared project', async () => {
        await scripterProjectService.searchForProject(projectName);
    });

    await test.step('Navigate to Questionnaire tab>Active view', async () => {
        await scripterUserQuestionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuestionText2, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuest_VariableName1);

    });

    await test.step('Verify that the question order as a Scripter is similar to that of a CS user', async () => {
        const scripterQuestionnaire = await scripterProjectService.getQuestionnaireLinesVariableName();
        await scripterProjectService.validateQuestionnairesAfterReOrder(masterQuestionnaireLinesVariableNames, scripterQuestionnaire);
    });
    await test.step('Delete the Created Project ', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});
test("[2528492] Verify that when the sort order is changed, its updated in the existing draft studies>Questions", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2528492' });
    const questionnaireService = new Questionnaireservice(page);
    const projectService = new ProjectService(page);
    const studyService = new StudyService(page);

    const questionName = TestData.StandardQuestion;
    const questionTitle = Questionnaire.Text.CategoryIntroduction;
    const webHelper = new WebHelper(page);
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();
    var guid = "";
    const studyName = StudyTestData.StudyName + Utils.generateGUID();
    let masterQuestionnaireLinesVariableNames: string[] = [];
    let QuestionnaireLinesVariableNames: string[] = [];

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.LibrarianUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.CSUser);
    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
        guid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Add a Question to the Project', async () => {
        await questionnaireService.addQuestion(questionName, questionTitle);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuestionText2, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuest_VariableName1);

    });

    await test.step('Add a Module to the Project', async () => {
        await questionnaireService.addModule(TestData.ModuleName);
        await projectService.verifydragAndDropTheMasterQuestionnaireLines();
        masterQuestionnaireLinesVariableNames = await projectService.getQuestionnaireLinesVariableName();
    });

    await test.step('Create a study & Compare questionnaire lines with master questionnaire lines', async () => {
        await studyService.CreateNewStudy(studyName, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await webHelper.handleConfirmationPopup();
        await webHelper.saveRecord();
        await studyService.ValidateInitialDraftStateStudy();
        await webHelper.verifySaveButton();
        const studyQuestion: string[] = await studyService.getStudyQuestionnaireLinesVaribaleName();
        await expect(studyQuestion[0]).toContain(questionName);
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();
    });
    await test.step('Drag and drop the questions', async () => {
        await questionnaireService.navigateToTheTab(Questionnaire.Tabs.Questionnaire);
        await projectService.dragAndDropTheMasterQuestionnaireLines();
        QuestionnaireLinesVariableNames = await projectService.getQuestionnaireLinesVariableName();
    });
    await test.step('Verify the order of the question when the question is drag and dropped.', async () => {
        await projectService.validateQuestionnairesAfterReOrder(masterQuestionnaireLinesVariableNames, QuestionnaireLinesVariableNames);
    });

    await test.step('Navigate to the Studies tab and open the existing Draft studies ', async () => {
        await studyService.openStudy(studyName);
        await webHelper.verifySaveButton();
    });

    await test.step('Verify that when the sort order is changed, its updated in the existing draft studies>Questions', async () => {
        const studyQuestion: string[] = await studyService.getStudyQuestionnaireLinesVaribaleName();
        await expect(studyQuestion[1]).toContain(questionName);
        await expect(studyQuestion[0]).toContain(TestData.M_QuestionName);

    });

    await test.step('Delete the Created Project ', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});

test("[2530091] Verify that through Edit must be able to reorder answers using a drag-and-drop method", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2530091' });
    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);
    const questionnaireService = new Questionnaireservice(page);
    const projectName = "AUTO_TestPro" + Utils.generateGUID();
    var answerTextsinAnswerTab: string[];
    var answerTextsinQuestionnaireTab: string[];

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.CSUser);
    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
    });
    await test.step('Add a Custom Question to the Project', async () => {
        await questionnaireService.addCustomQuestion(TestData.Custom);
    });

    await test.step('Expand the Custom Question', async () => {
        await questionnaireService.expandQuestionnaire(TestData.Custom);
    });

    await test.step('Click on the Edit Button', async () => {
        await questionnaireService.clickonEditbutton();
    });

    await test.step('Navigate to the Answer Tab', async () => {
        await questionnaireService.navigateToTheTab(QuestionBank.Tabs.Answers);
    });

    await test.step('Drag and Drop the Answers', async () => {
        await webHelper.executeDragAndDrop(Questionnaire.Text.AnswerDest, Questionnaire.Text.AnswerSrc);
    });

    await test.step('Get all Answer Texts from Answer Tab', async () => {
        answerTextsinAnswerTab = await questionnaireService.getAllAnswerTextsfromAnswerTab();
        await webHelper.saveAndCloseRecord();
    });

    await test.step('Expand the Custom Question and get all Answer Texts from Questionnaire Tab', async () => {
        await questionnaireService.expandQuestionnaire(TestData.Custom);
        answerTextsinQuestionnaireTab = await questionnaireService.getAllAnswerTextsfromQuestionnaireTab();
    });

    await test.step('Validate that the answers order in the Answer tab and Questionnaire tab are same', async () => {
        await expect(answerTextsinAnswerTab[0]).toBe(answerTextsinQuestionnaireTab[0]);
        await expect(answerTextsinAnswerTab[1]).toBe(answerTextsinQuestionnaireTab[1]);
    });

});

test("[2524821] To check if studies with different status is affected by adding of modules / questions from QL > side panel", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2524821' });
    const questionnaireService = new Questionnaireservice(page);
    const projectService = new ProjectService(page);
    const studyService = new StudyService(page);

    const webHelper = new WebHelper(page);
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();
    var guid = "";
    const studyName1 = StudyTestData.StudyName + Utils.generateGUID();
    const studyName2 = StudyTestData.StudyName + Utils.generateGUID();

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.LibrarianUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.CSUser);
    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
        guid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Add Standard Question to the Project', async () => {
        await questionnaireService.addQuestion(TestData.StandardQuestion, TestData.StandardQuestion);
    });

    await test.step('Create a study', async () => {
        await projectService.addScripterUserToProject();
        await studyService.CreateNewStudy(studyName1, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await studyService.ValidateInitialDraftStateStudy();
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();
    });

    await test.step('Create another study', async () => {
        await studyService.CreateNewStudy(studyName2, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await studyService.ValidateInitialDraftStateStudy();
    });

    await test.step('Update study from Draft to Ready for Scripting', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();
    });

    await test.step('Compare questionnaire lines with master questionnaire lines after Standard Question is added', async () => {
        await questionnaireService.navigateToTheTab(Questionnaire.Tabs.Questionnaire);
        var masterQuestionnaireLinesCount = await projectService.getMasterQuestionnaireLinesCount();
        const masterQuestionnaireLinesVariableNames: string[] = await projectService.getQuestionnaireLinesVariableName();
        await studyService.openStudy(studyName1);
        await studyService.compareMasterQuestionnairesWithStudyQuestionnairesLines(masterQuestionnaireLinesCount, masterQuestionnaireLinesVariableNames);
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();
    });

    await test.step('Add a Custom Question to the Project', async () => {
        await questionnaireService.addCustomQuestion(TestData.Custom);
    });

    await test.step('Compare questionnaire lines with master questionnaire lines after Custom Question is added', async () => {
        var masterQuestionnaireLinesCount = await projectService.getMasterQuestionnaireLinesCount();
        const masterQuestionnaireLinesVariableNames: string[] = await projectService.getQuestionnaireLinesVariableName();
        await studyService.openStudy(studyName1);
        await studyService.compareMasterQuestionnairesWithStudyQuestionnairesLines(masterQuestionnaireLinesCount, masterQuestionnaireLinesVariableNames);
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();
    });

    await test.step('Add a Module to the Project', async () => {
        await questionnaireService.addModule(TestData.ModuleName);
    });

    await test.step('Compare questionnaire lines with master questionnaire lines after adding Module', async () => {
        var masterQuestionnaireLinesCount = await projectService.getMasterQuestionnaireLinesCount();
        const masterQuestionnaireLinesVariableNames: string[] = await projectService.getQuestionnaireLinesVariableName();
        await studyService.openStudy(studyName1);
        await studyService.compareMasterQuestionnairesWithStudyQuestionnairesLines(masterQuestionnaireLinesCount, masterQuestionnaireLinesVariableNames);
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();
    });

    await test.step('Compare questionnaire lines with master questionnaire lines with Ready for Scripting study', async () => {
        await questionnaireService.navigateToTheTab(Questionnaire.Tabs.Questionnaire);
        var masterQuestionnaireLinesCount = await projectService.getMasterQuestionnaireLinesCount();
        const masterQuestionnaireLinesVariableNames: string[] = await projectService.getQuestionnaireLinesVariableName();
        await studyService.openStudy(studyName2);
        await studyService.compareMasterQuestionnairesWithRFSStudyQuestionnairesLines(masterQuestionnaireLinesCount, masterQuestionnaireLinesVariableNames);
        await webHelper.saveAndCloseRecord();
    });

    await test.step('Delete the Created Project ', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});
