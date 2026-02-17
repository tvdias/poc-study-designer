import { test } from '@playwright/test';
import { ProjectService } from '../../services/ProjectService';
import { LoginToMDAWithTestUser } from '../../utils/Login';
import { Utils } from "../../utils/utils";
import { WebHelper } from '../../utils/WebHelper';
import { deleteRecord } from '../../utils/APIHelper';
import { ProductsService } from '../../services/ProductsService';
import { ConfigQuestionsService } from '../../services/ConfigQuestionsService';
import { TestUser, AppName, AppId } from '../../constants/TestUsers.json';
import { TestData } from '../../Test Data/ProjectData.json';
import { EntityLogicalNames } from '../../constants/CommonLogicalNames.json';
import { DropDownList } from '../../constants/DropDownList.json';
import { Common } from '../../selectors/CommonSelector.json';
import { CommonTestData } from '../../Test Data/CommonTestData.json';
import { ConfigQuestions } from '../../selectors/ConfigQuestionsSelector.json';
import { Questionnaireservice } from '../../services/QuestionnaireService';
import { TestData as UMQTestData } from '../../Test Data/QuestionnnaireData.json';



test("[2271907] Create project as a CS user", { tag: ['@Smoke', '@CreateProject'] }, async ({ page }) => {

    test.info().annotations.push({ type: 'TestCaseId', description: '2271907' });

    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);

    await test.step('Navigating to URL', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.CSUser)
    });

    await test.step('Create a project', async () => {
        const projectName = TestData.ProjectName + Utils.generateGUID();
        await projectService.CreateProject(projectName);
    });

    await test.step('Clean up created record', async () => {
        var guid = await webHelper.fetchRecordGuid(page.url());
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});



const roles = [
    { user: TestUser.CSUser, TestcaseId: '2526934' },
    { user: TestUser.LibrarianUser, TestcaseId: '2621292' },
];

// Executes the same test case sequentially in the defined order using both CS user and Librarian user contexts.
test.describe.serial('Login functionality for different roles', () => {
    for (const role of roles) {

        test(`${role.TestcaseId} Create project as a ${role.user} and Add Question, Add Module`, { tag: ['@Smoke', '@CreateProject'] }, async ({ page }) => {
            test.info().annotations.push({ type: 'TestCaseId', description: role.TestcaseId });


            const projectService = new ProjectService(page);
            const webHelper = new WebHelper(page);
            const questionnaireService = new Questionnaireservice(page);
            var guid: string;

            await test.step('Navigating to URL', async () => {
                await LoginToMDAWithTestUser(page, role.user, AppId.UC1, AppName.UC1);
            });

            await test.step('Create a project', async () => {
                const projectName = TestData.ProjectName + Utils.generateGUID();
                await projectService.CreateProject(projectName);
                guid = await webHelper.fetchRecordGuid(page.url());
            });

            await test.step('Add a Standard Question to the Project', async () => {
                await questionnaireService.addQuestion(UMQTestData.StandardQuestion, UMQTestData.StandardQuestionTitle);
            });
            await test.step('Verify the Added Standard Question collapsed view', async () => {
                await questionnaireService.validateQuestion(UMQTestData.StandardQuestion);
            });
            await test.step('Add a Module to the Project', async () => {
                await questionnaireService.addModule(TestData.ExistingModule);
            });
            await test.step('Verify the Added Module collapsed view', async () => {
                await questionnaireService.validateModulesAddedInQuestionnaire(TestData.ExistingModule, TestData.M_QuestionName, TestData.M_QuestionType, TestData.M_QuestionText);
            });

            await test.step('Clean up created record', async () => {
                await deleteRecord(EntityLogicalNames.Projects, guid);
            });
        });
    }
});

test("[2335587] Verify with Scripter User- the Project details are displaying as Read-only", { tag: ['@Smoke', '@CreateProject'] }, async ({ page, browser }) => {

    test.info().annotations.push({ type: 'TestCaseId', description: '2335587' });

    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);
    const projectName = TestData.ProjectName + Utils.generateGUID();

    await test.step('Navigating to URL', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
    });

    await test.step('Create a project', async () => {
        await projectService.CreateProject(projectName);
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
    await test.step('Login as a scripter user', async () => {
        await LoginToMDAWithTestUser(scripterUserPage, TestUser.ScripterUser, AppId.UC1, AppName.UC1, true);
    });
    await test.step('Go to project > search for the shared project', async () => {
        await newProjectService.searchForProject(projectName);
    });
    await test.step('Check and validate fields are Not editable', async () => {
        await newProjectService.verifyProjectFieldsAreNotEditable();
    });

    await test.step('Clean up created record', async () => {
        var guid = await webHelperSecond.fetchRecordGuid(scripterUserPage.url());
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });

});

test("[2363270,2557095] Should display the configuration questions in the order that is defined in the product template", { tag: ['@Regression', '@Projects'] }, async ({ page }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2363270' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2557095' });

    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);

    await test.step('Navigating to URL', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
    });

    await test.step('Create a project', async () => {
        const projectName = TestData.ProjectName + Utils.generateGUID();
        await projectService.CreateCustomProject(projectName, DropDownList.Project.CATI, TestData.Client, TestData.CommissioningMarket, projectName);
    });

    await test.step('Generate master questionnaire & validate no. of questions', async () => {
        await projectService.addProductTemplate(TestData.Product1);
        await projectService.applyProductTemplate();
        await webHelper.saveRecord();
        await projectService.validateNoOfMasterQuestionnaire(3);
    });

    await test.step('Clean up created record', async () => {
        var guid = await webHelper.fetchRecordGuid(page.url());
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});


test("[2557094] Add a Product and Product Template and check all the Config Questions are displayed", { tag: ['@Regression', '@Projects'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2557094' });

    const webHelper = new WebHelper(page);
    const projectService = new ProjectService(page);
    let answers: string[] = [CommonTestData.Answer, CommonTestData.No];
    let answerSelection: string[] = [CommonTestData.Answer];
    const configQuestion = TestData.Question + Utils.generateGUID();

    let configQuestionGuid: string, productGuid: string, projectGuid: string;

    await test.step('Login with CS user into application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
    });

    await test.step('Create a project', async () => {
        const projectName = TestData.ProjectName + Utils.generateGUID();
        await projectService.CreateCustomProject(projectName, DropDownList.Project.CATI, TestData.Client, TestData.CommissioningMarket, projectName);
        projectGuid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Add product template & answer config questions', async () => {
        await projectService.addProductTemplate(TestData.Product1);
        await projectService.waitUntilConfigQuestionsVisible();
        await projectService.selectConfigQuestionAnswers(answerSelection);
    });

    await test.step('Verify all the Config Questions are displayed in Canvas', async () => {
        await projectService.validateConfigQuestionsInCanvas(TestData.ConfigQuestion, 0);
        await projectService.validateConfigQuestionsInCanvas(TestData.ConfigQuestion2, 1);
    });

    await test.step('Clean up created record', async () => {
        await deleteRecord(EntityLogicalNames.Projects, projectGuid);
    });
});