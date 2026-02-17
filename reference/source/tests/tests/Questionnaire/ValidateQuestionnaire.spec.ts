import { expect, test } from '@playwright/test';
import { LoginToMDAWithTestUser, waitUntilAppIdle } from '../../utils/Login';
import { TestUser, AppName, AppId } from '../../constants/TestUsers.json';
import { Utils } from "../../utils/utils";
import { DropDownList } from '../../constants/DropDownList.json';
import { WebHelper } from '../../utils/WebHelper';
import { CommonTestData } from '../../Test Data/CommonTestData.json';
import { Common } from '../../selectors/CommonSelector.json';
import { Questionnaire } from '../../selectors/QuestionnaireSelector.json';
import { QuestionBank } from '../../selectors/QuestionBankSelectors.json';
import { QuestionBankservice } from '../../services/QuestionBankService';
import { Questionnaireservice } from '../../services/QuestionnaireService';
import { TestData } from '../../Test Data/QuestionnnaireData.json';
import { TestData as ProjectTestData } from '../../Test Data/ProjectData.json';
import { TestData as QBTestData } from '../../Test Data/QuestionBankData.json';
import { deleteRecord } from '../../utils/APIHelper';
import { EntityLogicalNames } from '../../constants/CommonLogicalNames.json';
import { ProjectService } from '../../services/ProjectService';
import { StudySelectors } from '../../selectors/StudySelectors.json';
import { ProductsService } from '../../services/ProductsService';
import { StudyService } from '../../services/StudyService';
import { StudyTestData } from '../../Test Data/StudyData.json';
import { Project } from '../../selectors/ProjectSelectors.json';


test("[2324792,2513427] Validate The Questions and Associated Questions in Collapsed View after Adding Question and Module", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {

    test.info().annotations.push({ type: 'TestCaseId', description: '2324792' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2513427' });


    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);
    const questionnaireService = new Questionnaireservice(page);
    const projectName = "AUTO_TestPro" + Utils.generateGUID();

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.CSUser);

    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
    });
    await test.step('Add a Question to the Project', async () => {
        await questionnaireService.addQuestion(TestData.StandardQuestion, Questionnaire.Text.CategoryIntroduction);
    });
    await test.step('Verify the Added Question collapsed view', async () => {
        await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuest_VariableName1, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuestionText2);
        await questionnaireService.validateIconsForQuestionsAddedInQuestionnaire(TestData.StandardQuest_VariableName1);
    });
    await test.step('Add a Module to the Project', async () => {
        await questionnaireService.addModule(TestData.ModuleName);
    });
    await test.step('Verify the Added Module collapsed view', async () => {
        await questionnaireService.validateModulesAddedInQuestionnaire(TestData.ModuleName, TestData.M_QuestionName, TestData.M_QuestionType, TestData.M_QuestionText);
        await questionnaireService.validateIconsForQuestionsAddedInQuestionnaire(TestData.M_QuestionName);

    });

    await test.step('Delete the Created Project ', async () => {
        var guid = await webHelper.fetchRecordGuid(page.url());
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});


test("[2515377,2515410,2515414,2515417] Verify the Question details in the Questionnaire expanded view", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2515377' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2515410' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2515414' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2515417' });

    const questionbankService = new QuestionBankservice(page);

    const webHelper = new WebHelper(page);
    const questionName = CommonTestData.QuestionVariableName + Utils.generateText();
    const questionTitle = CommonTestData.QuestionTitle;
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();
    var id = "";

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.LibrarianUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.Librarian);
    });

    await test.step('Create a Question Bank', async () => {
        await questionbankService.fillMandatoryfieldsInQuestionBank(questionName, DropDownList.QuestionBank.MultiChoice, questionTitle, TestData.QuestionText);
        await questionbankService.fillOptionalfieldsInQuestionBank(TestData.SortOrder, TestData.SortOrder, TestData.MinLength, TestData.MaxLength, TestData.QuestionFormatDetails, TestData.QuestionRationale, TestData.ScripterNotes, TestData.Methodology, TestData.CustomNotes, TestData.SingleorMulticode);
        await questionbankService.clickonSaveRecord();
        await questionbankService.searchDraftQuestionBank(questionName);
    });

    await test.step('Change the Status Reason Draft to Active', async () => {
        await questionbankService.changeStatusReason();
        await questionbankService.selectQuestionBank(questionName);
        id = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Navigate and Click on New Question Answer Lists button', async () => {
        await questionbankService.clickNewQuestionAnswer();
    });

    await test.step('Fill and Create Answers List', async () => {
        await questionbankService.FillandCreateQuestionAnswsersList(TestData.AnswerText, TestData.AnswerCode, TestData.Location, CommonTestData.Answer, TestData.Property, TestData.Version);
        await page.close();
    });

    const context = await browser.newContext();
    const csUserPage = await context.newPage();
    const webHelperSecond = new WebHelper(csUserPage);
    const newProjectService = new ProjectService(csUserPage);
    const newQuestionnaireService = new Questionnaireservice(csUserPage);

    await test.step('Login as a CS user', async () => {
        await LoginToMDAWithTestUser(csUserPage, TestUser.CSUser, AppId.UC1, AppName.UC1, true);
    });
    await test.step('Go to project > Create a Project', async () => {
        await newProjectService.CreateProject(projectName);
    });
    await test.step('Add a Question to the Project', async () => {
        await newQuestionnaireService.addQuestion(questionName, questionTitle);
    });
    await test.step('Verify the Added Question details under Questionnaire', async () => {
        const answerType = "FIXED, EXCLUSIVE, OPEN";
        await newQuestionnaireService.validateQuestionsAddedInQuestionnaire(questionName, DropDownList.QuestionBank.MultiChoice, TestData.QuestionText);
        await newQuestionnaireService.expandQuestionnaire(DropDownList.QuestionBank.MultiChoice);
        await newQuestionnaireService.validateTheDataAddedInQuestionnaire(questionName, DropDownList.QuestionBank.MultiChoice, questionTitle, TestData.QuestionText, TestData.AnswerCode, TestData.AnswerText, TestData.ScripterNotes, TestData.QuestionFormatDetails, TestData.SortOrder, TestData.MinLength, TestData.MaxLength, answerType)
    });

    await test.step('Delete the Created Project ', async () => {
        var guid = await webHelper.fetchRecordGuid(csUserPage.url());
        await deleteRecord(EntityLogicalNames.Projects, guid);
        await deleteRecord(EntityLogicalNames.QuestionBanks, id);

    });
});


test(" [2515488]Add and Validate Question, Module and Custom Questions", { tag: ['@Smoke', '@Questionnaire'] }, async ({ page, browser }) => {

    test.info().annotations.push({ type: 'TestCaseId', description: '2515488' });

    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);
    const questionnaireService = new Questionnaireservice(page);
    const projectName = "AUTO_TestPro" + Utils.generateGUID();

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.CSUser);
    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
    });
    await test.step('Add a Standard Question to the Project', async () => {
        await questionnaireService.addQuestion(TestData.StandardQuestion, Questionnaire.Text.CategoryIntroduction);

    });
    await test.step('Verify the Added Standard Question collapsed view', async () => {
        await questionnaireService.validateQuestion(TestData.StandardQuest_VariableName1);
    });
    await test.step('Add a Module to the Project', async () => {
        await questionnaireService.addModule(TestData.ModuleName);
    });
    await test.step('Verify the Added Module collapsed view', async () => {
        await questionnaireService.validateModulesAddedInQuestionnaire(TestData.ModuleName, TestData.M_QuestionName, TestData.M_QuestionType, TestData.M_QuestionText);
    });
    await test.step('Add a Custom Question to the Project', async () => {
        await questionnaireService.addCustomQuestion(TestData.Custom);
    });
    await test.step('Verify the Added Custom Question', async () => {
        await questionnaireService.validateQuestion(TestData.Custom);
    });

    await test.step('Delete the Created Project ', async () => {
        var guid = await webHelper.fetchRecordGuid(page.url());
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});


test("[2530066,2516854,2529844] Verify that through Edit must be able to remove existing answer options", { tag: ['@Regression', '@Questionnaire'] }, async ({ page }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2530066' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2516854' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2529844' });

    const questionnaireService = new Questionnaireservice(page);
    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();
    const answerType = "EXCLUSIVE";


    await test.step('Login as a CS user', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);

    });
    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
    });
    await test.step('Add a Question to the Project', async () => {
        await questionnaireService.addQuestion(CommonTestData.QuestionBankName, CommonTestData.QuestionVariableText);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(CommonTestData.QuestionBankText, DropDownList.QuestionBank.NumericInput, CommonTestData.QuestionBankName);
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.NumericInput);
        await questionnaireService.validateTheAnswersInQuestionnaire(CommonTestData.AnswerCode, CommonTestData.AnswerText, answerType);
    });

    await test.step('Deactivate the existing answer', async () => {
        await webHelper.clickonStartWithText("Edit");
        await projectService.navigateToAnswersTabFromMasterQuestionnaireLines();
        await projectService.openAnswersListInGrid();
        await projectService.deactiavteTheAnswer(StudySelectors.Button.Deactivate);
        await page.goBack();
    });

    await test.step('Verify that the changes are visible in Questionnaire screen', async () => {
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.NumericInput);
        await questionnaireService.validateTheAnswersInQuestionnaire(CommonTestData.AnswerCode, CommonTestData.AnswerText, answerType, false);
    });

    await test.step('Delete the Created Project ', async () => {
        var guid = await webHelper.fetchRecordGuid(page.url());
        await deleteRecord(EntityLogicalNames.Projects, guid);

    });
});


test("[2530097,2515415,2516857,2361640] Verify that through Edit must specifying answer properties (open, exclusive, fixed) - Answers screen", { tag: ['@Regression', '@Questionnaire'] }, async ({ browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2530097' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2515415' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2516857' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2361640' });

    const context = await browser.newContext();
    const page = await context.newPage();
    const questionbankService = new QuestionBankservice(page);

    const webHelper = new WebHelper(page);
    const questionName = CommonTestData.QuestionVariableName + Utils.generateText();
    const questionTitle = CommonTestData.QuestionTitle;
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();
    var id = "";

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.LibrarianUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.Librarian);
    });

    await test.step('Create a Question Bank', async () => {
        await questionbankService.fillMandatoryfieldsInQuestionBank(questionName, DropDownList.QuestionBank.MultiChoice, questionTitle, TestData.QuestionText);
        await questionbankService.fillOptionalfieldsInQuestionBank(TestData.SortOrder, TestData.SortOrder, TestData.MinLength, TestData.MaxLength, TestData.QuestionFormatDetails, TestData.QuestionRationale, TestData.ScripterNotes, TestData.Methodology, TestData.CustomNotes, TestData.SingleorMulticode);
        await questionbankService.clickonSaveRecord();
        await questionbankService.searchDraftQuestionBank(questionName);

    });

    await test.step('Change the Status Reason Draft to Active', async () => {
        await questionbankService.changeStatusReason();
        await questionbankService.selectQuestionBank(questionName);
        id = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Navigate and Click on New Question Answer Lists button', async () => {
        await questionbankService.clickNewQuestionAnswer();
    });

    await test.step('Fill and Create Answers List', async () => {
        await questionbankService.FillandCreateQuestionAnswsersList(TestData.AnswerText, TestData.AnswerCode, TestData.Location, CommonTestData.Answer, TestData.Property, TestData.Version);
        await page.close();
    });

    const newContext = await browser.newContext();
    const csUserPage = await newContext.newPage();
    const webHelperSecond = new WebHelper(csUserPage);
    const newProjectService = new ProjectService(csUserPage);
    const newQuestionnaireService = new Questionnaireservice(csUserPage);

    await test.step('Login as a CS user', async () => {
        await LoginToMDAWithTestUser(csUserPage, TestUser.CSUser, AppId.UC1, AppName.UC1);

    });
    await test.step('Go to project > Create a Project', async () => {
        await newProjectService.CreateProject(projectName);
    });
    await test.step('Add a Question to the Project', async () => {
        await newQuestionnaireService.addQuestion(questionName, questionTitle);
    });
    await test.step('Verify the Added Question details under Questionnaire', async () => {
        const answerType = "FIXED, EXCLUSIVE, OPEN";
        await newQuestionnaireService.validateQuestionsAddedInQuestionnaire(questionName, DropDownList.QuestionBank.MultiChoice, TestData.QuestionText);
        await newQuestionnaireService.expandQuestionnaire(DropDownList.QuestionBank.MultiChoice);
        await newQuestionnaireService.validateTheDataAddedInQuestionnaire(questionName, DropDownList.QuestionBank.MultiChoice, questionTitle, TestData.QuestionText, TestData.AnswerCode, TestData.AnswerText, TestData.ScripterNotes, TestData.QuestionFormatDetails, TestData.SortOrder, TestData.MinLength, TestData.MaxLength, answerType)
    });

    await webHelperSecond.clickonStartWithText("Edit");


    const questionVariableName = await newProjectService.navigateToAnswersTabFromMasterQuestionnaireLines();

    await test.step('Change the answer properties (open, exclusive, fixed)', async () => {
        await newProjectService.openAnswersAndChangeProperties(TestData.AnswerText);
        await webHelperSecond.saveAndCloseRecord();
    });

    await test.step('Verify that the changes are visible in Questionnaire screen', async () => {
        const answerType = "FIXED, EXCLUSIVE, OPEN";
        await newQuestionnaireService.expandQuestionnaire(DropDownList.QuestionBank.MultiChoice);
        await newQuestionnaireService.validateTheAnswersPropertiesInQuestionnaire(TestData.AnswerText, TestData.AnswerCode, answerType, false);
    });

    await test.step('Delete the Created Project ', async () => {
        var guid = await webHelper.fetchRecordGuid(csUserPage.url());
        await deleteRecord(EntityLogicalNames.Projects, guid);
        await deleteRecord(EntityLogicalNames.QuestionBanks, id);

    });
});


test("[2530063] Verify Edit Question and move to answers tab and add new answers", { tag: ['@Regression', '@Questionnaire'] }, async ({ page }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2530063' });

    const questionnaireService = new Questionnaireservice(page);
    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();

    await test.step('Login as a CS user', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
    });
    await test.step('Add a Question to the Project', async () => {
        await questionnaireService.addQuestion(TestData.StandardQuestion, TestData.StandardQuestionTitle);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuestionText2, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuest_VariableName1);
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.SmallTextInput);
    });

    await test.step('Add the Answer to the list', async () => {
        await webHelper.clickonStartWithText("Edit");
        await projectService.navigateToAnswersTabFromMasterQuestionnaireLines();
        await projectService.AddAnswerTextToTheList(TestData.AnswerText1);
        await webHelper.saveRecord();
        await webHelper.saveAndCloseRecord();
    });



    await test.step('Verify that the changes are visible in Questionnaire screen', async () => {
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.SmallTextInput);
        await webHelper.verifyTheCellText(TestData.AnswerText1);
    });

    await test.step('Delete the Created Project ', async () => {
        var guid = await webHelper.fetchRecordGuid(page.url());
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});


test("[2530062] Edit the Question details and  verify the changes in Questionnaire ", { tag: ['@Regression', '@Questionnaire'] }, async ({ page }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2530062' });

    const questionnaireService = new Questionnaireservice(page);
    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();

    await test.step('Login as a CS user', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);

    });
    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
    });
    await test.step('Add a Question to the Project', async () => {
        await questionnaireService.addQuestion(TestData.StandardQuestion, TestData.StandardQuestionTitle);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuestionText2, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuest_VariableName1);
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.SmallTextInput);
    });

    await test.step('Modify the Question details', async () => {
        await webHelper.clickonStartWithText("Edit");
        await questionnaireService.ModifyQuestionDetails(TestData.StandardQuestionText1, TestData.Custom);
        await webHelper.saveRecord();
        await webHelper.saveAndCloseRecord();
    });

    await test.step('Verify that the changes are visible in Questionnaire screen', async () => {
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.SmallTextInput);
        await questionnaireService.verifyQuestionDataInQuestionnaire(TestData.StandardQuestionText1, TestData.Custom);
    });

    await test.step('Delete the Created Project ', async () => {
        var guid = await webHelper.fetchRecordGuid(page.url());
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});


test("[2532189,2532191,2532194,2532197,2515598] Check the removing of modules from the Questionnaire after clicking on Remove button ", { tag: ['@Regression', '@Questionnaire'] }, async ({ page }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2532189' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2532191' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2532194' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2532197' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2515598' });

    const questionnaireService = new Questionnaireservice(page);
    const projectService = new ProjectService(page);


    const webHelper = new WebHelper(page);
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.CSUser);
    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
    });
    await test.step('Add a Module to the Project', async () => {
        await questionnaireService.addModule(TestData.ModuleName1);
        await questionnaireService.addModule(TestData.ModuleName2);
        await questionnaireService.addModule(TestData.ModuleName3);
        await questionnaireService.addModule(TestData.ModuleName4);
        await questionnaireService.addModule(TestData.ModuleName5);

    });
    await test.step('Verify the Count of Modules', async () => {
        let modulecount = await projectService.getMasterQuestionnaireLinesCount();
        await expect(modulecount).toBe(6);
    });


    await test.step('Remove and verify the existing/added Modules ', async () => {
        await questionnaireService.removeModule(TestData.ModuleName1, TestData.ModuleName2);
        let modulecount = await projectService.getMasterQuestionnaireLinesCount();
        await expect(modulecount).toBe(3);
    });

    await test.step('Delete the Created Project ', async () => {
        var guid = await webHelper.fetchRecordGuid(page.url());
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});

test("[2549722] Add new answers from custom question and check answer codes created", { tag: ['@Revisit', '@Questionnaire'] }, async ({ page }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2549722' });

    const questionnaireService = new Questionnaireservice(page);
    const projectService = new ProjectService(page);

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
    await test.step('Add a Custom Question to the Project', async () => {
        await questionnaireService.addCustomQuestion(TestData.Custom);
    });
    await test.step('Validate the Answer text and Answer Code for the custom Question', async () => {
        await questionnaireService.validateQuestion(TestData.Custom);
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.SingleChoice);
        await webHelper.verifyTheCellText(TestData.AnswerText2);
        await webHelper.verifyTheCellText(TestData.AnswerCode2);
        await webHelper.verifyTheCellText(TestData.AnswerText3);
        await webHelper.verifyTheCellText(TestData.AnswerCode3);
    });
    await webHelper.clickonStartWithText("Edit");

    await projectService.navigateToAnswersTabFromMasterQuestionnaireLines();

    await test.step('Add the Answer to the list', async () => {
        await projectService.AddAnswerTextToTheList(TestData.AnswerText4);
        await webHelper.saveAndCloseRecord();
    });

    await test.step('Verify the Answer Text and Answer Code in Questionnaire screen', async () => {
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.SingleChoice);
        await webHelper.verifyTheCellText(TestData.AnswerText4);
    });

    await test.step('Delete the Created Project ', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});

test("[2503589,2503591,2515422] Deactivate a question and check question is strike out and appearing in de-active question list", { tag: ['@Regression', '@Questionnaire'] }, async ({ browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2503589' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2503591' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2515422' });

    const context = await browser.newContext();
    const page = await context.newPage();
    const questionnaireService = new Questionnaireservice(page);
    const projectService = new ProjectService(page);

    const questionName = TestData.StandardQuestion;
    const questionTitle = Questionnaire.Text.CategoryIntroduction;
    const webHelper = new WebHelper(page);
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.CSUser);
    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
    });
    await test.step('Add a Question to the Project', async () => {
        await questionnaireService.addQuestion(questionName, questionTitle);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuestionText2, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuest_VariableName1);

    });
    await test.step('Deactivate the Question from Questionnaire', async () => {
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.SmallTextInput);
        await webHelper.clickonStartWithText("Edit");
        await projectService.clickonButton(Common.Text.Deactivate);
        await webHelper.verifyCommandBarBtn(Common.Text.Activate);
        await page.goBack();
        await webHelper.verifySaveButton();
        await webHelper.clickOnTab(Questionnaire.Tabs.Questionnaire);
        await webHelper.saveRecord();
        await webHelper.saveRecord();

    });
    await test.step('verify the Deactived Question In Active Questions', async () => {
        await webHelper.verifyTheSpantext(TestData.StandardQuestionText2, false);
        await webHelper.verifyTheLabeltext(DropDownList.QuestionBank.SmallTextInput, false);
        await webHelper.verifyTheSpantext(TestData.StandardQuest_VariableName1, false);
    });
    await test.step('Change to InActive Questions', async () => {
        await webHelper.selectByOption(Common.CSS.ActiveQuestions, TestData.InactiveQuestions);
    });

    await test.step('Verify the Question Details in Inactive Questions and check question is strike out', async () => {
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.SmallTextInput);
        await webHelper.verifyTheSpantext(TestData.StandardQuestionText2);
        await webHelper.verifyTheLabeltext(DropDownList.QuestionBank.SmallTextInput);
        await webHelper.verifyTheSpantext(TestData.StandardQuest_VariableName1);
        await questionnaireService.verifyQuestionsStrikeout(TestData.StandardQuestionText2);
    });

    await test.step('Delete the Created Project ', async () => {
        var guid = await webHelper.fetchRecordGuid(page.url());
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});


const roles = [
    { user: TestUser.CSUser, TestcaseId: '2559310' },
    { user: TestUser.LibrarianUser, TestcaseId: '2621317' },
];

// Executes the same test case sequentially in the defined order using both CS user and Librarian user contexts.
test.describe.serial('', () => {
    for (const role of roles) {

        test(`${role.TestcaseId} Verify that when ${role.user} clicks on the Edit button on the UMQ expand view, the resulting page opens in the same page, not in a new tab.`, { tag: ['@Regression', '@Questionnaire'] }, async ({ page }) => {

            test.info().annotations.push({ type: 'TestCaseId', description: role.TestcaseId });

            const questionnaireService = new Questionnaireservice(page);
            const projectService = new ProjectService(page);

            const questionName = TestData.StandardQuestion;
            const questionTitle = Questionnaire.Text.CategoryIntroduction;
            const webHelper = new WebHelper(page);
            const projectName = "AUTO_TestProject_" + Utils.generateGUID();
            var guid = "";

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
            await test.step('Click on the Edit button, the resulting page opens in the same page', async () => {
                await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.SmallTextInput);
                await webHelper.clickonStartWithText("Edit");
                await webHelper.verifyTheTab(Questionnaire.Tabs.General);
            });

            await test.step('Delete the Created Project ', async () => {
                await deleteRecord(EntityLogicalNames.Projects, guid);
            });
        });
    }
});


test("[2570720] Verify that when a question has been made inactive, it must not be possible to add again", { tag: ['@Regression', '@Questionnaire'] }, async ({ page }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2570720' });

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
    await test.step('Deactivate the Question from Questionnaire', async () => {
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.SmallTextInput);
        await webHelper.clickonStartWithText("Edit");
        await projectService.clickonButton(Common.Text.Deactivate);
        await webHelper.verifyCommandBarBtn(Common.Text.Activate);
        await page.goBack();
        await webHelper.verifySaveButton();
        await webHelper.clickOnTab(Questionnaire.Tabs.Questionnaire);

    });
    await test.step('verify the Deactived Question In Active Questions', async () => {
        await webHelper.verifyTheSpantext(TestData.StandardQuestionText2, false);
        await webHelper.verifyTheLabeltext(DropDownList.QuestionBank.SmallTextInput, false);
        await webHelper.verifyTheSpantext(TestData.StandardQuest_VariableName1, false);
    });
    await test.step('Verify that the deactivated question cannot be added through add button in the header', async () => {
        await questionnaireService.verifyQuestionInImportFromLibrary(questionName, questionTitle, false);
    });

    await test.step('Delete the Created Project ', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});
test("[2570735] Verify that when a module has been made inactive, it must not be possible to add again", { tag: ['@Regression', '@Questionnaire'] }, async ({ page }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2570735' });

    const questionnaireService = new Questionnaireservice(page);
    const projectService = new ProjectService(page);

    const webHelper = new WebHelper(page);
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.CSUser);
    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
    });
    await test.step('Add a Module to the Project', async () => {
        await questionnaireService.addModule(TestData.ModuleName);
    });
    await test.step('Verify the Added Module collapsed view', async () => {
        await questionnaireService.validateModulesAddedInQuestionnaire(TestData.ModuleName, TestData.M_QuestionName, TestData.M_QuestionType, TestData.M_QuestionText, 0);
        await questionnaireService.validateIconsForQuestionsAddedInQuestionnaire(TestData.M_QuestionName);

    });

    await test.step('Remove and verify the existing/added Modules ', async () => {
        await questionnaireService.removeModuleFromQuestionnaire(TestData.ModuleName);
    });

    await test.step('Verify that the removed module cannot be added through add button in the header', async () => {
        await questionnaireService.verifyModuleInImportFromLibrary(TestData.ModuleName, false);
    });

    await test.step('Delete the Created Project ', async () => {
        var guid = await webHelper.fetchRecordGuid(page.url());
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});

test("[2546489,2546442] (Scripter) Verify that inactive records are Strikethrough", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2546489' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2546442' });

    const questionnaireService = new Questionnaireservice(page);
    const projectService = new ProjectService(page);

    const questionName = TestData.StandardQuestion;
    const questionTitle = Questionnaire.Text.CategoryIntroduction;
    const webHelper = new WebHelper(page);
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();
    var guid = "";

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
        guid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Add a Question to the Project', async () => {
        await questionnaireService.addQuestion(questionName, questionTitle);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuestionText2, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuest_VariableName1);

    });
    await test.step('Deactivate the Question from Questionnaire', async () => {
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.SmallTextInput);
        await webHelper.clickonStartWithText("Edit");
        await projectService.clickonButton(Common.Text.Deactivate);
        await webHelper.verifyCommandBarBtn(Common.Text.Activate);
        await page.goBack();
        await webHelper.verifySaveButton();
        await webHelper.clickOnTab(Questionnaire.Tabs.Questionnaire);
        await webHelper.saveRecord();

    });
    await test.step('verify the Deactived Question In Active Questions', async () => {
        await webHelper.verifyTheSpantext(TestData.StandardQuestionText2, false);
        await webHelper.verifyTheLabeltext(DropDownList.QuestionBank.SmallTextInput, false);
        await webHelper.verifyTheSpantext(TestData.StandardQuest_VariableName1, false);
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

    await test.step('Change to InActive Questions', async () => {
        await webHelperSecond.selectByOption(Common.CSS.ActiveQuestions, TestData.InactiveQuestions);
    });

    await test.step('Verify the Question Details in Inactive Questions and check question is strike out', async () => {
        await scripterUserQuestionnaireService.verifyQuestionsStrikeout(TestData.StandardQuestionText2);
    });

    await test.step('Delete the Created Project ', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});

test("[2444247] Verify if dummy questions can be added to a product template and successfully  applied to project", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2444247' });

    const questionnaireService = new Questionnaireservice(page);
    const projectService = new ProjectService(page);

    const productsService = new ProductsService(page);
    const productName = CommonTestData.ProductName + Utils.generateGUID();
    const productTemplateName = CommonTestData.ProductTemplateName + Utils.generateGUID();
    const productTemplateVersion = CommonTestData.ProductTemplateVersion;
    const webHelper = new WebHelper(page);
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();
    var productguid = "";
    var projectguid = "";



    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.LibrarianUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.Librarian)
    });

    await test.step('Create a product', async () => {
        await productsService.createProduct(productName);
        productguid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Add configurations questions', async () => {
        await productsService.addConfigQuestions(CommonTestData.ConfigQuestion);
    });

    await test.step('Create a product template', async () => {
        await productsService.createProductTemplate(productTemplateName, productTemplateVersion);
    });

    await test.step('Add a product template lines with a dummy question which is standard ', async () => {
        await productsService.addProductTemplateLines(productTemplateName, DropDownList.ConfigQuestions.Question, DropDownList.ConfigQuestions.True, CommonTestData.StandardDummyQuestion);
    });
    await test.step('Add a product template lines with a dummy question which is custom ', async () => {
        await productsService.addProductTemplateLines(productTemplateName, DropDownList.ConfigQuestions.Question, DropDownList.ConfigQuestions.True, CommonTestData.CustomDummyQuestion1);
    });

    await test.step('Add a product template lines with module where dummy question is available', async () => {
        await productsService.addProductTemplateLines(productTemplateName, DropDownList.ConfigQuestions.Module, DropDownList.ConfigQuestions.True, CommonTestData.ModuleWithDummyQuestion);
    });

    await test.step('Go to project > Create a Project', async () => {
        await webHelper.changeArea(Common.Text.CSUser);
        await projectService.CreateProject(projectName);
        projectguid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Add the product template to the Project', async () => {
        await projectService.addProductTemplate(productName);
        await projectService.applyProductTemplate();
        await webHelper.saveRecord();
    });

    await test.step('Apply template to the project such that dummy questions are added which are Active and included by default as True ', async () => {
        await questionnaireService.validateQuestionsAddedInQuestionnaire(CommonTestData.StandardDummyQuestion, DropDownList.QuestionBank.MultiChoice, CommonTestData.StandardDummyQuestionText);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(CommonTestData.CustomDummyQuestion, DropDownList.QuestionBank.Logic, CommonTestData.CustomDummyQuestionText);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(CommonTestData.StandardDummyQuestion1, DropDownList.QuestionBank.SmallTextInput, CommonTestData.StandardDummyQuestionText);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(CommonTestData.CustomDummyQuestion1, DropDownList.QuestionBank.NumericInput, CommonTestData.CustomDummyQuestionText1);

    });
    await test.step('Delete the Created Project ', async () => {
        await deleteRecord(EntityLogicalNames.Products, productguid);
        await deleteRecord(EntityLogicalNames.Projects, projectguid);

    });
});

test("[2505144] Verify that Librarian user is able to view the master questionnaire for a project in a single-page interface 1", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2505144' });

    const questionnaireService = new Questionnaireservice(page);
    const projectService = new ProjectService(page);

    const webHelper = new WebHelper(page);
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();
    var projectguid = "";


    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.LibrarianUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.CSUser)
    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
        projectguid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Verify that Librarian user is able to view the master questionnaire tab for existing project in single-page interface', async () => {
        await questionnaireService.addQuestion(TestData.StandardQuestion, TestData.StandardQuestionTitle);
        await questionnaireService.validateAllTabinSingleView();
    });

    await test.step('Delete the Created Project ', async () => {
        await deleteRecord(EntityLogicalNames.Projects, projectguid);

    });
});
test("[2505143]  Verify that Scripter user is able to view (read-only) the master questionnaire for a project in a single-page interface", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2505143' });

    const projectService = new ProjectService(page);

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

    await test.step('Now share a project to scripter user from User management', async () => {
        await projectService.addScripterUserToProject();
        await webHelper.saveRecord();
        await page.close();
    });

    const context = await browser.newContext();
    const scripterUserPage = await context.newPage();
    const newProjectService = new ProjectService(scripterUserPage);
    const scripterUserQuestionnaireService = new Questionnaireservice(scripterUserPage);

    await test.step('Login as a scripter user', async () => {
        await LoginToMDAWithTestUser(scripterUserPage, TestUser.ScripterUser, AppId.UC1, AppName.UC1, true);
    });
    await test.step('Go to project > search for the shared project', async () => {
        await newProjectService.searchForProject(projectName);
    });

    await test.step('Verify that Scripter user is able to RO view the master questionnaire tab for new created project in single-page interface', async () => {
        await scripterUserQuestionnaireService.validateAllTabinSingleView(false);
    });

    await test.step('Delete the Created Project ', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});

const userroles = [
    { user: TestUser.CSUser, TestcaseId: '2513464' },
    { user: TestUser.LibrarianUser, TestcaseId: '2621517' },
]
// Executes the same test case sequentially in the defined order using both CS user and Librarian user contexts.
test.describe.serial('', () => {
    for (const role of userroles) {

        test(`${role.TestcaseId} Verify that the ${role.user} is able to filter the questions based on Question Title and Question Variable Name in the Search bar.`, { tag: ['@Regression', '@Questionnaire'] }, async ({ page }) => {
            test.info().annotations.push({ type: 'TestCaseId', description: role.TestcaseId });

            const questionnaireService = new Questionnaireservice(page);
            const projectService = new ProjectService(page);
            const webHelper = new WebHelper(page);
            const projectName = "AUTO_TestProject_" + Utils.generateGUID();
            var guid = "";

            await test.step('Login into MDA application', async () => {
                await LoginToMDAWithTestUser(page, role.user, AppId.UC1, AppName.UC1);
                await webHelper.changeArea(Common.Text.CSUser);
            });

            await test.step('Go to project > Create a Project', async () => {
                await projectService.CreateProject(projectName);
                guid = await webHelper.fetchRecordGuid(page.url());
            });
            await test.step('Add  a Module to the Project', async () => {
                await questionnaireService.addModule(CommonTestData.ModuleWithDummyQuestion);
                await questionnaireService.validateQuestionsAddedInQuestionnaire(CommonTestData.CustomDummyQuestion, DropDownList.QuestionBank.Logic, CommonTestData.CustomDummyQuestionText);
                await questionnaireService.validateQuestionsAddedInQuestionnaire(CommonTestData.StandardDummyQuestion1, DropDownList.QuestionBank.SmallTextInput, CommonTestData.StandardDummyQuestionText1);
            });
            await test.step('Verify that user is able to filter the questions based on the Question Text and Question Variable Name', async () => {
                await questionnaireService.filterQuestionsInQuestionnaire(CommonTestData.CustomDummyQuestionText);
                await questionnaireService.validateQuestionsAddedInQuestionnaire(CommonTestData.CustomDummyQuestion, DropDownList.QuestionBank.Logic, CommonTestData.CustomDummyQuestionText);
                await questionnaireService.filterQuestionsInQuestionnaire(CommonTestData.CustomDummyQuestion);
                await questionnaireService.validateQuestionsAddedInQuestionnaire(CommonTestData.CustomDummyQuestion, DropDownList.QuestionBank.Logic, CommonTestData.CustomDummyQuestionText);
                await questionnaireService.filterQuestionsInQuestionnaire(CommonTestData.StandardDummyQuestion1);
                await questionnaireService.validateQuestionsAddedInQuestionnaire(CommonTestData.StandardDummyQuestion1, DropDownList.QuestionBank.SmallTextInput, CommonTestData.StandardDummyQuestionText1);
                await questionnaireService.filterQuestionsInQuestionnaire(CommonTestData.StandardDummyQuestionText1);
                await questionnaireService.validateQuestionsAddedInQuestionnaire(CommonTestData.StandardDummyQuestion1, DropDownList.QuestionBank.SmallTextInput, CommonTestData.StandardDummyQuestionText1);

            });

            await test.step('Delete the Created Project ', async () => {
                await deleteRecord(EntityLogicalNames.Projects, guid);
            });
        });
    }
});

test("[2515419] Validate that any updates made to questions in the Master Questionnaire are accurately reflected in the Questionnaire tab → Questions for CS user", { tag: ['@Regression', '@Questionnaire'] }, async ({ page }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2515419' });

    const projectService = new ProjectService(page);
    const questionnaireService = new Questionnaireservice(page);

    const webHelper = new WebHelper(page);
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();


    await test.step('Login as a CS user', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);

    });
    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
    });
    await test.step('Add a Question to the Project', async () => {
        await questionnaireService.addQuestion(TestData.StandardQuestion, TestData.StandardQuestionTitle);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuestion, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuestionText2);
    });

    await test.step('Click on Edit button', async () => {
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.SmallTextInput);
        await webHelper.clickonStartWithText("Edit");
    });

    await test.step('Modify the Question details and Add the Answer to the list', async () => {
        await questionnaireService.ModifyQuestionDetails(TestData.StandardQuestionText1, TestData.Custom);
        await projectService.navigateToAnswersTabFromMasterQuestionnaireLines();
        await projectService.AddAnswerTextToTheList(TestData.AnswerText1);
        await webHelper.saveAndCloseRecord();

    });

    await test.step('Verify that the changes are visible in Questionnaire screen', async () => {
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.SmallTextInput);
        await questionnaireService.verifyQuestionDataInQuestionnaire(TestData.StandardQuestionText1, TestData.Custom);
        await questionnaireService.validateTheAnswerText(TestData.AnswerText1);
    });

    await test.step('Delete the Created Project ', async () => {
        var guid = await webHelper.fetchRecordGuid(page.url());
        await deleteRecord(EntityLogicalNames.Projects, guid);

    });
});
test("[2557097] Check the Product and product template questions are added when they are existing questions/modules in the Questionnaire", { tag: ['@Regression', '@Questionnaire'] }, async ({ page }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2557097' });

    const projectService = new ProjectService(page);
    const questionnaireService = new Questionnaireservice(page);

    const webHelper = new WebHelper(page);
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();


    await test.step('Login as a CS user', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);

    });
    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
    });
    await test.step('Add a Question to the Project', async () => {
        await questionnaireService.addQuestion(TestData.StandardQuestion, TestData.StandardQuestionTitle);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuestion, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuest_VariableName1);
    });
    await test.step('Add a Module to the Project', async () => {
        await questionnaireService.addModule(TestData.ModuleName1);
    });

    await test.step('Add the product template to the Project', async () => {
        await projectService.addProductTemplate(ProjectTestData.Product1);
        await projectService.applyProductTemplate();
        await webHelper.saveRecord();
    });
    await test.step('Verify all added Questions/Module are also added in Questionnaire', async () => {
        const questionnaireNames: string[] = await projectService.getQuestionnaireLinesVariableName();
        await expect(questionnaireNames.length).toBe(3);
        await expect(questionnaireNames).not.toContain(TestData.StandardQuestion);
        await expect(questionnaireNames).not.toContain(TestData.ModuleName1);
        await expect(questionnaireNames).not.toContain(TestData.Question2);

    });

    await test.step('Delete the Created Project ', async () => {
        var guid = await webHelper.fetchRecordGuid(page.url());
        await deleteRecord(EntityLogicalNames.Projects, guid);

    });

});

test("[2546518] Verify that the dummy questions added by the scripter is displayed in all Study states", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2546518' });

    const projectService = new ProjectService(page);

    const webHelper = new WebHelper(page);
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();
    var guid = "";

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
        guid = await webHelper.fetchRecordGuid(page.url());
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
        await webHelperSecond.verifyTheSpantext(Project.Text.Questions);
    });

    await test.step('Add dummy questions in the Questionnaire', async () => {
        await scripterUserQuestionnaireService.addQuestion(CommonTestData.StandardDummyQuestion, CommonTestData.QuestionTitle, false);
        await scripterUserQuestionnaireService.validateQuestionsAddedInQuestionnaire(CommonTestData.StandardDummyQuestion, DropDownList.QuestionBank.MultiChoice, CommonTestData.StandardDummyQuestionText);
    });


    const csContext = await browser.newContext();
    const csUserPage = await csContext.newPage();
    const webHelperCS = new WebHelper(csUserPage);
    const csuserProjectService = new ProjectService(csUserPage);
    const csUserQuestionnaireService = new Questionnaireservice(csUserPage);
    const studyService = new StudyService(csUserPage);
    const studyName = StudyTestData.StudyName + Utils.generateGUID();

    await test.step('Login as a CS user', async () => {
        await LoginToMDAWithTestUser(csUserPage, TestUser.CSUser, AppId.UC1, AppName.UC1, true);
    });
    await test.step('Go to project > search for the shared project', async () => {
        await webHelperCS.searchAndOpenRecord(projectName);
        await csUserQuestionnaireService.validateQuestionsAddedInQuestionnaire(CommonTestData.StandardDummyQuestion, DropDownList.QuestionBank.MultiChoice, CommonTestData.StandardDummyQuestionText);
    });

    await test.step('Create a study & Validate study state', async () => {

        await studyService.CreateNewStudy(studyName, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await webHelperCS.handleConfirmationPopup();
        await studyService.ValidateInitialDraftStateStudy();
        await webHelperCS.saveRecord();
        const dummyQuestion: string[] = await studyService.getStudyQuestionnaireLinesVaribaleName();
        await expect(dummyQuestion[0]).toContain(CommonTestData.StandardDummyQuestion);
    });

    await test.step('Change the Study state to Ready for Scripting and save the study', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
        const dummyQuestion: string[] = await studyService.getStudyQuestionnaireLinesVaribaleName();
        await expect(dummyQuestion[0]).toContain(CommonTestData.StandardDummyQuestion);
    });
    await test.step('Change the study state to Approved for Launch and save the study', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ApprovedforLaunch);
        const dummyQuestion: string[] = await studyService.getStudyQuestionnaireLinesVaribaleName();
        await expect(dummyQuestion[0]).toContain(CommonTestData.StandardDummyQuestion);
    });
    await test.step('Click on the Complete Study button', async () => {
        await studyService.ValidateAndClickOnCompleteStudybutton();
        const dummyQuestion: string[] = await studyService.getStudyQuestionnaireLinesVaribaleName();
        await expect(dummyQuestion[0]).toContain(CommonTestData.StandardDummyQuestion);
    });

    await test.step('Delete the Created Project ', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});

const users = [
    { user: TestUser.CSUser, TestcaseId: '2563383' },
    { user: TestUser.LibrarianUser, TestcaseId: '2621521' },
]
// Executes the same test case sequentially in the defined order using both CS user and Librarian user contexts.
test.describe.serial('', () => {
    for (const role of users) {

        test(`${role.TestcaseId} Verify that ${role.user} the AI Answer code is generated for custom Answer on a standard question`, { tag: ['@Regression', '@Questionnaire'] }, async ({ page }) => {
            test.info().annotations.push({ type: 'TestCaseId', description: role.TestcaseId });

            const projectService = new ProjectService(page);
            const questionbankService = new Questionnaireservice(page);

            const webHelper = new WebHelper(page);

            var guid: string;
            const projectName = "AUTO_TestProject_" + Utils.generateGUID();

            await test.step('Navigating to URL', async () => {
                await LoginToMDAWithTestUser(page, role.user, AppId.UC1, AppName.UC1);
            });

            await test.step('Create a project', async () => {
                await projectService.CreateProject(projectName);
                guid = await webHelper.fetchRecordGuid(page.url());
            });

            await test.step('Add Question to the Project', async () => {
                await questionbankService.addQuestion(TestData.StandardQuestion, TestData.StandardQuestionTitle);
                await questionbankService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuestion, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuestionText2);
            });

            await test.step('In the Questionnaire tab, expand the question and click on edit', async () => {
                await projectService.expandFirstQuestionFromQuestionnaire();
                await webHelper.clickonStartWithText("Edit");
            });


            await test.step('Navigate to the Answer tab and add custom answers', async () => {
                await projectService.navigateToAnswersTabFromMasterQuestionnaireLines();
                await projectService.AddAnswerTextToTheList(TestData.AnswerText5);
                await webHelper.saveRecord();
            });

            await test.step('Verify that the AI Answer code is generated for custom Answer on a standard question', async () => {
                const aiAnswerCode = await questionbankService.getAICodeForAnswerText(Questionnaire.CSS.AnswerCode);
                await expect(aiAnswerCode[0].length).toBeGreaterThan(0);
                await expect(aiAnswerCode[0]).toContain("UK");
                await expect(aiAnswerCode[0]).toContain("BEAUTIFUL");
            });

            await test.step('Clean up created project record', async () => {
                await deleteRecord(EntityLogicalNames.Projects, guid);
            });
        });
    }
});

test("[2563384] Verify that the Answer code for standard answers should not be overwritten by AI Answer code", { tag: ['@Regression', '@Questionnaire'] }, async ({ browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2563384' });

    const context = await browser.newContext();
    const page = await context.newPage();
    const questionbankService = new QuestionBankservice(page);

    const webHelper = new WebHelper(page);
    const questionName = CommonTestData.QuestionVariableName + Utils.generateText();
    const questionTitle = CommonTestData.QuestionTitle;
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();
    var id = "";

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.LibrarianUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.Librarian);
    });

    await test.step('Create a Question Bank', async () => {
        await questionbankService.fillMandatoryfieldsInQuestionBank(questionName, DropDownList.QuestionBank.MultiChoice, questionTitle, TestData.QuestionText);
        await questionbankService.fillOptionalfieldsInQuestionBank(TestData.SortOrder, TestData.SortOrder, TestData.MinLength, TestData.MaxLength, TestData.QuestionFormatDetails, TestData.QuestionRationale, TestData.ScripterNotes, TestData.Methodology, TestData.CustomNotes, TestData.SingleorMulticode);
        await questionbankService.clickonSaveRecord();
        await questionbankService.searchDraftQuestionBank(questionName);
    });

    await test.step('Change the Status Reason Draft to Active', async () => {
        await questionbankService.changeStatusReason();
        await questionbankService.selectQuestionBank(questionName);
        id = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Navigate and Click on New Question Answer Lists button', async () => {
        await questionbankService.clickNewQuestionAnswer();
    });

    await test.step('Fill and Create Answers List', async () => {
        await questionbankService.FillandCreateQuestionAnswsersList(TestData.AnswerText, TestData.AnswerCode, TestData.Location, CommonTestData.Answer, TestData.Property, TestData.Version);
        await page.close();
    });

    const newContext = await browser.newContext();
    const csUserPage = await newContext.newPage();
    const webHelperSecond = new WebHelper(csUserPage);
    const newProjectService = new ProjectService(csUserPage);
    const newQuestionnaireService = new Questionnaireservice(csUserPage);

    await test.step('Login as a CS user', async () => {
        await LoginToMDAWithTestUser(csUserPage, TestUser.CSUser, AppId.UC1, AppName.UC1);
    });
    await test.step('Go to project > Create a Project', async () => {
        await newProjectService.CreateProject(projectName);
    });
    await test.step('Add a Question to the Project', async () => {
        await newQuestionnaireService.addQuestion(questionName, questionTitle);
    });
    await test.step('Verify that the Answer code for standard answers should not be overwritten by AI Answer code', async () => {
        const answerType = "FIXED, EXCLUSIVE, OPEN";
        await newQuestionnaireService.validateQuestionsAddedInQuestionnaire(questionName, DropDownList.QuestionBank.MultiChoice, TestData.QuestionText);
        await newQuestionnaireService.expandQuestionnaire(DropDownList.QuestionBank.MultiChoice);
        await newQuestionnaireService.validateTheDataAddedInQuestionnaire(questionName, DropDownList.QuestionBank.MultiChoice, questionTitle, TestData.QuestionText, TestData.AnswerCode, TestData.AnswerText, TestData.ScripterNotes, TestData.QuestionFormatDetails, TestData.SortOrder, TestData.MinLength, TestData.MaxLength, answerType)
    });


    await test.step('Delete the Created Project ', async () => {
        var guid = await webHelper.fetchRecordGuid(csUserPage.url());
        await deleteRecord(EntityLogicalNames.Projects, guid);
        await deleteRecord(EntityLogicalNames.QuestionBanks, id);

    });
});

test("[2546469] Verify that the user is able to add a standard dummy or custom question in the Questionnaire tab.", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2546469' });

    const questionnaireService = new Questionnaireservice(page);
    const projectService = new ProjectService(page);

    const webHelper = new WebHelper(page);
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();
    var projectguid = "";

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.CSUser)
    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
    });
    await test.step('Add a Standard Dummy Question to the Project', async () => {
        await questionnaireService.addQuestion(CommonTestData.StandardDummyQuestion1, CommonTestData.StandardDummyQuestionTitle1);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(CommonTestData.StandardDummyQuestion1, DropDownList.QuestionBank.SmallTextInput, CommonTestData.StandardDummyQuestionText1);

    });

    await test.step('Add a Custom Dummy Question to the Project', async () => {
        await questionnaireService.addCustomQuestionToQuestionnaire(CommonTestData.CustomDummyQuestion, CommonTestData.DummyQuestionCustomTitle);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(CommonTestData.CustomDummyQuestion, DropDownList.QuestionBank.Logic, CommonTestData.CustomDummyQuestionText);
    });

    await test.step('Delete the Created Project ', async () => {
        await deleteRecord(EntityLogicalNames.Projects, projectguid);

    });
});

test("[2546471,2530060] Verify that the user is able to edit standard dummy and custom dummy questions", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2546471' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2530060' });

    const projectService = new ProjectService(page);

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

    await test.step('Add dummy questions in the Questionnaire', async () => {
        await scripterUserQuestionnaireService.addQuestion(CommonTestData.StandardDummyQuestion, CommonTestData.QuestionTitle, false);
        await scripterUserQuestionnaireService.validateQuestionsAddedInQuestionnaire(CommonTestData.StandardDummyQuestion, DropDownList.QuestionBank.MultiChoice, CommonTestData.StandardDummyQuestionText);
    });

    await test.step('Modify the Question details', async () => {
        await scripterUserQuestionnaireService.expandQuestionnaire(DropDownList.QuestionBank.MultiChoice);
        await webHelperSecond.clickonStartWithText("Edit");
        await scripterUserQuestionnaireService.ModifyQuestionDetails(TestData.StandardQuestionText1, TestData.Custom);
        await webHelperSecond.saveRecord();
        await webHelperSecond.saveAndCloseRecord();
    });

    await test.step('Verify that the changes are visible in Questionnaire screen', async () => {
        await scripterUserQuestionnaireService.expandQuestionnaire(DropDownList.QuestionBank.MultiChoice);
        await scripterUserQuestionnaireService.verifyQuestionDataInQuestionnaire(TestData.StandardQuestionText1, TestData.Custom);
    });


    await test.step('Delete the Created Project ', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});

test("[2444932] Verify if updating a master questionnaire line IsDummy property updates study snapshots / change log / questionnaire lines", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2444932' });

    const questionnaireService = new Questionnaireservice(page);
    const projectService = new ProjectService(page);
    const studyService = new StudyService(page);
    const studyName = StudyTestData.StudyName + Utils.generateGUID();
    const newStudyVersionName = studyName + "_V2";

    const webHelper = new WebHelper(page);
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();
    var projectguid = "";

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.CSUser)
    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
        projectguid = await webHelper.fetchRecordGuid(page.url());
    });
    await test.step('Add a Question to the Project', async () => {
        await questionnaireService.addQuestion(TestData.StandardQuestion, Questionnaire.Text.CategoryIntroduction);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuest_VariableName1, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuestionText2);
    });
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

    await test.step('Update the Study Version', async () => {
        await studyService.createNewStudyVersion(newStudyVersionName);
        await webHelper.clickOnPopupButton(Common.Text.OK);
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();

    });

    await test.step('Change the IsDummy Toggle', async () => {
        await studyService.goToProject(projectName);
        await projectService.expandFirstQuestionFromQuestionnaire();
        await webHelper.clickonStartWithText("Edit");
        await questionnaireService.toggleIsDummyQuestion();
    });

    await test.step('Update study from Draft to Ready for Scritping', async () => {
        await studyService.openStudy(newStudyVersionName);
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();
        await studyService.openStudy(newStudyVersionName);
        await webHelper.verifySaveButton();
        await webHelper.saveRecord();
    });

    await test.step('Check the snapshot"IsDummy " column in snapshot questionnaire line ', async () => {
        await studyService.ValidateStudySnapshotQuestionsIsDummy();
    });
    await test.step('Check the snapshot"IsDummy " column in Changelog snapshot questionnaire line ', async () => {
        const deactivateQuestion = TestData.StandardQuest_VariableName1 + ',' + 'Field Change (Question)';
        const changeLogs: string[] = [deactivateQuestion]
        await studyService.validateStudyChangeLogs(changeLogs);
    });


    await test.step('Delete the Created Project ', async () => {
        await deleteRecord(EntityLogicalNames.Projects, projectguid);

    });
});
test("[2515523,2515597]  To verify if a user can choose between Standard and Custom question types when inserting questions.", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2515523' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2515597' });

    const questionnaireService = new Questionnaireservice(page);
    const projectService = new ProjectService(page);

    const webHelper = new WebHelper(page);
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();
    var projectguid = "";

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.CSUser)
    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
        projectguid = await webHelper.fetchRecordGuid(page.url());
    });
    await test.step('Choose questions from side panel and search for a standard question', async () => {
        await questionnaireService.addQuestion(TestData.StandardQuestion, Questionnaire.Text.CategoryIntroduction);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuest_VariableName1, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuestionText2);
    });

    await test.step('Try to search for a Custom question : "CUSTOM_LARGE_TEXT" ', async () => {
        await questionnaireService.searchCustomQuestionFromStandardQuestionTab(TestData.Custom);
        await questionnaireService.verifyNoResultsMatchMessage();
    });
    await test.step('Try to search for a standard question  ', async () => {
        await questionnaireService.addQuestion(TestData.Question1, TestData.Question_Title1);
    });
    await test.step('Try to search for a standard question  from Custom as the search option from the side panel ', async () => {
        await questionnaireService.searchStandardQuestionFromCustomQuestionTab(CommonTestData.QuestionBankName);
        await questionnaireService.verifyNoResultsMatchMessage();
    });
    await test.step('Try to search for a custom question ', async () => {
        await questionnaireService.addCustomQuestionToQuestionnaire(CommonTestData.CustomDummyQuestion, CommonTestData.DummyQuestionCustomTitle);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(CommonTestData.CustomDummyQuestion, DropDownList.QuestionBank.Logic, CommonTestData.CustomDummyQuestionText);
    });

    await test.step('Delete the Created Project ', async () => {
        await deleteRecord(EntityLogicalNames.Projects, projectguid);

    });
});

test("[2515621] To verify if the module is added to the questionnaire line for whose clicked on + button", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2515621' });

    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);
    const questionnaireService = new Questionnaireservice(page);
    const projectName = "AUTO_TestPro" + Utils.generateGUID();

    let masterQuestionnaireLinesVariableNames: string[] = [];
    let QuestionnaireLinesVariableNames: string[] = [];

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.CSUser);

    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
    });

    await test.step('Add a Module to the Project', async () => {
        await questionnaireService.addModule(TestData.ModuleName6);
    });
    await test.step('Check if all individual questions are added with the modules', async () => {
        await questionnaireService.validateModulesAddedInQuestionnaire(TestData.ModuleName6, TestData.ModuleName6_Question1, DropDownList.QuestionBank.NumericMatrix, TestData.ModuleName6_Question_Text);
    });
    await test.step('Reorder a question which is associated with module ', async () => {
        masterQuestionnaireLinesVariableNames = await projectService.getQuestionnaireLinesVariableName();
        await projectService.dragAndDropTheMasterQuestionnaireLines();
        QuestionnaireLinesVariableNames = await projectService.getQuestionnaireLinesVariableName();
        await projectService.validateQuestionnairesAfterReOrder(masterQuestionnaireLinesVariableNames, QuestionnaireLinesVariableNames);
    });
    await test.step('Check if all module level details are available ', async () => {
        await questionnaireService.validateModulesAddedInQuestionnaire(TestData.ModuleName6, TestData.ModuleName6_Question2, DropDownList.QuestionBank.NumericInput, TestData.ModuleName6_Question_Text2);
    });
    await test.step('Check if all the questions related to a module is visible in the questionnaire line once a module is added  ', async () => {
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.NumericMatrix);
        await questionnaireService.validateTheDataAddedInQuestionnaire(TestData.ModuleName6_Question1, DropDownList.QuestionBank.NumericMatrix, TestData.ModuleName6_QuestionTitle, TestData.ModuleName6_Question_Text, TestData.ModuleName6_AnswerCode, TestData.ModuleName6_AnswerText, TestData.ModuleName6_ScripterNotes, TestData.ModuleName6_QuestionFormatDetails, TestData.SortOrder1, TestData.MinLength, TestData.MaxLength, "")
    });

    await test.step('Delete the Created Project ', async () => {
        var guid = await webHelper.fetchRecordGuid(page.url());
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});
test("[2516853] Veirfy that the Dummy question indicator is displayed as a badge next to Question Type field", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2516853' });

    const projectService = new ProjectService(page);
    const questionnaireService = new Questionnaireservice(page);

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

    await test.step('Add dummy questions in the Questionnaire', async () => {
        await questionnaireService.addQuestion(CommonTestData.StandardDummyQuestion, CommonTestData.QuestionTitle, false);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(CommonTestData.StandardDummyQuestion, DropDownList.QuestionBank.MultiChoice, CommonTestData.StandardDummyQuestionText);
    });

    await test.step('Verify that the dummy question is displayed as a badge next to Question Type field ', async () => {
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.MultiChoice);
        await questionnaireService.verifyDummyQuestionBadgeIsDisplayed(TestData.DummyQuestionBadgeText);
    });

    await test.step('Delete the Created Project ', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});

test("[2516862] Verify that each answer grid contains three columns  Answer Code, Answer Text and Answer Properties (Fixed, Exclusive, Open)", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2516862' });

    const questionbankService = new QuestionBankservice(page);

    const webHelper = new WebHelper(page);
    const questionName = CommonTestData.QuestionVariableName + Utils.generateText();
    const questionTitle = CommonTestData.QuestionTitle;
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();
    var id = "";

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.LibrarianUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.Librarian);
    });

    await test.step('Create a Question Bank', async () => {
        await questionbankService.fillMandatoryfieldsInQuestionBank(questionName, DropDownList.QuestionBank.MultiChoice, questionTitle, TestData.QuestionText);
        await questionbankService.fillOptionalfieldsInQuestionBank(TestData.SortOrder, TestData.SortOrder, TestData.MinLength, TestData.MaxLength, TestData.QuestionFormatDetails, TestData.QuestionRationale, TestData.ScripterNotes, TestData.Methodology, TestData.CustomNotes, TestData.SingleorMulticode);
        await questionbankService.clickonSaveRecord();
        await questionbankService.searchDraftQuestionBank(questionName);

    });

    await test.step('Change the Status Reason Draft to Active', async () => {
        await questionbankService.changeStatusReason();
        await questionbankService.selectQuestionBank(questionName);
        id = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Navigate and Click on New Question Answer Lists button', async () => {
        await questionbankService.clickNewQuestionAnswer();
    });

    await test.step('Fill and Create Answers List', async () => {
        await questionbankService.FillandCreateQuestionAnswsersList(TestData.AnswerText, TestData.AnswerCode, TestData.Location, CommonTestData.Answer, TestData.Property, TestData.Version);
        await page.close();
    });

    const context = await browser.newContext();
    const csUserPage = await context.newPage();
    const webHelperSecond = new WebHelper(csUserPage);
    const newProjectService = new ProjectService(csUserPage);
    const newQuestionnaireService = new Questionnaireservice(csUserPage);

    await test.step('Login as a CS user', async () => {
        await LoginToMDAWithTestUser(csUserPage, TestUser.CSUser, AppId.UC1, AppName.UC1, true);
    });
    await test.step('Go to project > Create a Project', async () => {
        await newProjectService.CreateProject(projectName);
    });
    await test.step('Add a Question to the Project', async () => {
        await newQuestionnaireService.addQuestion(questionName, questionTitle);
    });
    await test.step('Verify that each answer grid contains three columns  Answer Code, Answer Text and Answer Properties (Fixed, Exclusive, Open)', async () => {
        const answerType = "FIXED, EXCLUSIVE, OPEN";
        await newQuestionnaireService.validateQuestionsAddedInQuestionnaire(questionName, DropDownList.QuestionBank.MultiChoice, TestData.QuestionText);
        await newQuestionnaireService.expandQuestionnaire(DropDownList.QuestionBank.MultiChoice);
        const count = await newQuestionnaireService.getCountOfColumnsInAnswerGrid();
        expect(count).toBe(3);
        await newQuestionnaireService.validateTheDataAddedInQuestionnaire(questionName, DropDownList.QuestionBank.MultiChoice, questionTitle, TestData.QuestionText, TestData.AnswerCode, TestData.AnswerText, TestData.ScripterNotes, TestData.QuestionFormatDetails, TestData.SortOrder, TestData.MinLength, TestData.MaxLength, answerType)
    });

    await test.step('Delete the Created Project ', async () => {
        var guid = await webHelper.fetchRecordGuid(csUserPage.url());
        await deleteRecord(EntityLogicalNames.Projects, guid);
        await deleteRecord(EntityLogicalNames.QuestionBanks, id);
    });
});

test("[2520478] To check if Questionnaire line tab in a project  is RO for scripter", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2520478' });

    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);
    const questionnaireService = new Questionnaireservice(page);
    const projectName = "AUTO_TestPro" + Utils.generateGUID();

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.CSUser);

    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
    });
    await test.step('Add a Question to the Project', async () => {
        await questionnaireService.addQuestion(TestData.StandardQuestion, Questionnaire.Text.CategoryIntroduction);
    });
    await test.step('Verify the Added Question collapsed view', async () => {
        await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuest_VariableName1, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuestionText2);
        await questionnaireService.validateIconsForQuestionsAddedInQuestionnaire(TestData.StandardQuest_VariableName1);
    });
    await test.step('Add a Module to the Project', async () => {
        await questionnaireService.addModule(TestData.ModuleName);
    });
    await test.step('Verify the Added Module collapsed view', async () => {
        await questionnaireService.validateModulesAddedInQuestionnaire(TestData.ModuleName, TestData.M_QuestionName, TestData.M_QuestionType, TestData.M_QuestionText);
        await questionnaireService.validateIconsForQuestionsAddedInQuestionnaire(TestData.M_QuestionName);

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

    await test.step('Check if all the questions / modules are available from which CS user / librarian user added', async () => {
        await scripterUserQuestionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuest_VariableName1, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuestionText2);
        await scripterUserQuestionnaireService.validateModulesAddedInQuestionnaire(TestData.ModuleName, TestData.M_QuestionName, TestData.M_QuestionType, TestData.M_QuestionText);
    });

    await test.step('Check if any + button is visible ', async () => {
        await scripterUserQuestionnaireService.verifyAddIconinQuestionnaire();
        await scripterUserQuestionnaireService.clickonAddIconinQuestionnaire();
         await scripterUserQuestionnaireService.verifyTabsinImportFromLibrary(Questionnaire.CSS.QuestionButton);
        await scripterUserQuestionnaireService.verifyTabsinImportFromLibrary(Questionnaire.CSS.CustomButton);
    });

    await test.step('Delete the Created Project ', async () => {
        var guid = await webHelper.fetchRecordGuid(scripterUserPage.url());
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});

test("[2520479] To check if Questionnaire line tab in a project contains all access of a CS user to Librarian", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2520479' });
    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);
    const questionnaireService = new Questionnaireservice(page);
    const projectName = "AUTO_TestPro" + Utils.generateGUID();

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.CSUser);

    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
    });
    await test.step('Add a Question to the Project', async () => {
        await questionnaireService.addQuestion(TestData.StandardQuestion, Questionnaire.Text.CategoryIntroduction);
    });
    await test.step('Verify the Added Question collapsed view', async () => {
        await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuest_VariableName1, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuestionText2);
        await questionnaireService.validateIconsForQuestionsAddedInQuestionnaire(TestData.StandardQuest_VariableName1);
    });
    await test.step('Add a Module to the Project', async () => {
        await questionnaireService.addModule(TestData.ModuleName);
    });
    await test.step('Verify the Added Module collapsed view', async () => {
        await questionnaireService.validateModulesAddedInQuestionnaire(TestData.ModuleName, TestData.M_QuestionName, TestData.M_QuestionType, TestData.M_QuestionText);
        await questionnaireService.validateIconsForQuestionsAddedInQuestionnaire(TestData.M_QuestionName);
    });

    await test.step('Now share a project to Librarian user from User management', async () => {
        await projectService.addScripterUserToProject(Project.ByRole.LibrarianUserName);
        await webHelper.saveRecord();
        await page.close();
    });

    const context = await browser.newContext();
    const librarianUserPage = await context.newPage();
    const webHelperSecond = new WebHelper(librarianUserPage);
    const newProjectService = new ProjectService(librarianUserPage);
    const librarianUserQuestionnaireService = new Questionnaireservice(librarianUserPage);

    await test.step('Login as a scripter user', async () => {
        await LoginToMDAWithTestUser(librarianUserPage, TestUser.LibrarianUser, AppId.UC1, AppName.UC1, true);
    });
    await test.step('Go to project > search for the shared project', async () => {
        await webHelperSecond.searchAndOpenRecord(projectName);
    });

    await test.step('Check if all the questions / modules are available from which CS user / librarian user added', async () => {
        await librarianUserQuestionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuest_VariableName1, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuestionText2);
        await librarianUserQuestionnaireService.validateModulesAddedInQuestionnaire(TestData.ModuleName, TestData.M_QuestionName, TestData.M_QuestionType, TestData.M_QuestionText);
    });

    await test.step('Check if any + button is visible ', async () => {
        await librarianUserQuestionnaireService.verifyAddIconinQuestionnaire();
    });

    await test.step('Click on + button and try to add a question', async () => {
        await librarianUserQuestionnaireService.addQuestion(CommonTestData.StandardDummyQuestion, CommonTestData.QuestionTitle);
        await librarianUserQuestionnaireService.validateQuestionsAddedInQuestionnaire(CommonTestData.StandardDummyQuestion, DropDownList.QuestionBank.MultiChoice, CommonTestData.StandardDummyQuestionText);
    });

    await test.step('Click on + button and try to add a module', async () => {
        await librarianUserQuestionnaireService.addModule(TestData.ModuleName6);
        await librarianUserQuestionnaireService.validateModulesAddedInQuestionnaire(TestData.ModuleName6, TestData.ModuleName6_Question1, DropDownList.QuestionBank.NumericMatrix, TestData.ModuleName6_Question_Text);
    });
    await test.step('Delete the Created Project ', async () => {
        var guid = await webHelper.fetchRecordGuid(librarianUserPage.url());
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });

});

test("[2546465]  Verify that Remove Module button is removed for Scripter user and Questions, Custom buttons are displayed in the side panel", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2546465' });
    const projectService = new ProjectService(page);

    const webHelper = new WebHelper(page);
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();
    let guid = "";

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
        guid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Now share a project to scripter user from User management', async () => {
        await projectService.addScripterUserToProject();
        await webHelper.saveRecord();
        await page.close();
    });

    const context = await browser.newContext();
    const scripterUserPage = await context.newPage();
    const newProjectService = new ProjectService(scripterUserPage);
    const scripterUserQuestionnaireService = new Questionnaireservice(scripterUserPage);

    await test.step('Login as a scripter user', async () => {
        await LoginToMDAWithTestUser(scripterUserPage, TestUser.ScripterUser, AppId.UC1, AppName.UC1, true);
    });
    await test.step('Go to project > search for the shared project', async () => {
        await newProjectService.searchForProject(projectName);
    });

    await test.step('Verify that the Remove Modules button is not displayed for scripter user  i.e Scripter user will not be able to Remove Modules,', async () => {
        await scripterUserQuestionnaireService.verifyRemoveModule(false);
    });
    await test.step('Click on the + icon on the Questionnaire grid header ', async () => {
        await scripterUserQuestionnaireService.clickonAddIconinQuestionnaire();
    });
    await test.step('Verify that in the Add question side panel, Standard and Custom button is displayed ', async () => {
        await scripterUserQuestionnaireService.verifyTabsinImportFromLibrary(Questionnaire.CSS.QuestionButton);
        await scripterUserQuestionnaireService.verifyTabsinImportFromLibrary(Questionnaire.CSS.CustomButton);
    });

    await test.step('Delete the Created Project ', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});

test("[2546477]  Verify that the scripter user is not able to edit, remove and redo the non dummy questions", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2546477' });
    const projectService = new ProjectService(page);
    const questionnaireService = new Questionnaireservice(page);

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
        await questionnaireService.addQuestion(TestData.StandardQuestion, Questionnaire.Text.CategoryIntroduction);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuest_VariableName1, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuestionText2);
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

    await test.step('Verify that the scripter user is not able to remove and redo the non dummy questions. i.e.- The delete button is greyed out - The redo button is greyed out - The edit button is displayed', async () => {
        await scripterUserQuestionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuest_VariableName1, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuestionText2);
        await scripterUserQuestionnaireService.verifyQuestionnairebutton(Questionnaire.TestDataId.DeleteButton, false);
        await scripterUserQuestionnaireService.verifyQuestionnairebutton(Questionnaire.TestDataId.ReactivateButton, false);
        await scripterUserQuestionnaireService.expandQuestionnaire(DropDownList.QuestionBank.SmallTextInput);
        await scripterUserQuestionnaireService.verifyQuestionnairebutton(Questionnaire.TestDataId.EditButton);

    });
    await test.step('Delete the Created Project ', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});

test("[2515416]  Verify that the Row Sort Order, Column Sort Order, Answer Min, and Answer Max are displayed when the Question Type is set as follows:", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2515416' });

    const projectService = new ProjectService(page);
    const questionnaireService = new Questionnaireservice(page);

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

    await test.step('Add a Multple Choice Question to the Project', async () => {
        await questionnaireService.addQuestion(QBTestData.MultipleChoiceQn, QBTestData.MultipleChoiceQnTitle);
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.MultiChoice);
    });
    await test.step('Verify that the Row Sort Order, Column Sort Order, Answer Min, and Answer Max are displayed when the Question Type is set as follows', async () => {
        await questionnaireService.validateTheDataAddedInQuestionnaire(QBTestData.MultipleChoiceQn, DropDownList.QuestionBank.MultiChoice, QBTestData.MultipleChoiceQnTitle, QBTestData.SingleChoiceMatrixQn_Text, TestData.ModuleName6_AnswerCode, QBTestData.AnswerText, TestData.ModuleName6_ScripterNotes, QBTestData.FormatDetails, TestData.SortOrder1, QBTestData.MinimumLength, QBTestData.MaximumLength, "");
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.MultiChoice);
    });
    await test.step('Add a Multple Choice Matrix Question to the Project', async () => {
        await questionnaireService.addQuestion(QBTestData.MultipleChoiceMatrixQn, QBTestData.MultipleChoiceMatrixQnTitle);
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.MultipleChoiceMatrix);
    });
    await test.step('Verify that the Row Sort Order, Column Sort Order, Answer Min, and Answer Max are displayed when the Question Type is set as follows', async () => {
        await questionnaireService.validateTheDataAddedInQuestionnaire(QBTestData.MultipleChoiceMatrixQn, DropDownList.QuestionBank.MultipleChoiceMatrix, QBTestData.MultipleChoiceMatrixQnTitle, QBTestData.Regression, TestData.ModuleName6_AnswerCode, QBTestData.AnswerText, TestData.ModuleName6_ScripterNotes, QBTestData.FormatDetails, TestData.SortOrder1, "", "", "");
        await webHelper.verifyText(QuestionBank.Title.ColumnSortOrder);
    });
    await test.step('Add a Numeric Input Question to the Project', async () => {
        await questionnaireService.addQuestion(QBTestData.NumericInputQn, QBTestData.NumericInputQn_Title);
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.NumericInput);
    });
    await test.step('Verify that the Answer Min, and Answer Max are displayed when the Question Type is set as follows', async () => {
        await questionnaireService.validateTheDataAddedInQuestionnaire(QBTestData.NumericInputQn, DropDownList.QuestionBank.NumericInput, QBTestData.NumericInputQn_Title, QBTestData.NumericInputQn_Text, TestData.ModuleName6_AnswerCode, QBTestData.AnswerText, TestData.ModuleName6_ScripterNotes, TestData.ModuleName6_QuestionFormatDetails, "", QBTestData.MinimumLength, QBTestData.MaximumLength2, "");
    });

    await test.step('Add a Numeric Matrix Question to the Project', async () => {
        await questionnaireService.addQuestion(QBTestData.NumericMatrixQn, QBTestData.NumericMatrixQn_Title);
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.NumericMatrix);
    });
    await test.step('Verify that the Row Sort Order, Answer Min, and Answer Max are displayed when the Question Type is set as follows', async () => {
        await questionnaireService.validateTheDataAddedInQuestionnaire(QBTestData.NumericMatrixQn, DropDownList.QuestionBank.NumericMatrix, QBTestData.NumericMatrixQn_Title, QBTestData.NumericMatrixQn_Text, TestData.ModuleName6_AnswerCode, QBTestData.AnswerText, TestData.ModuleName6_ScripterNotes, TestData.ModuleName6_QuestionFormatDetails, TestData.SortOrder1, TestData.MinLength, TestData.MaxLength, "");
        await webHelper.verifyText(QuestionBank.Title.RowSortOrder);
    });

    await test.step('Add a Single Choice Question to the Project', async () => {
        await questionnaireService.addQuestion(QBTestData.SingleChoiceQn, QBTestData.SingleChoiceQn_Title);
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.SingleChoice);
    });
    await test.step('Verify that the Row Sort Order, Column Sort Order, Answer Min, and Answer Max are displayed when the Question Type is set as follows', async () => {
        const answerType = "EXCLUSIVE, OPEN";
        await questionnaireService.validateTheDataAddedInQuestionnaire(QBTestData.SingleChoiceQn, DropDownList.QuestionBank.SingleChoice, QBTestData.SingleChoiceQn_Title, QBTestData.SingleChoiceQn_Text, QBTestData.SingleChoiceQn_AnswerCode, QBTestData.SingleChoiceQn_AnswerCode, TestData.ModuleName6_ScripterNotes, TestData.ModuleName6_QuestionFormatDetails, TestData.SortOrder1, "", "", answerType);
        await webHelper.verifyText(QuestionBank.Title.RowSortOrder);
    });
    await test.step('Add a Single Choice Matrix Question to the Project', async () => {
        await questionnaireService.addQuestion(QBTestData.SingleChoiceMatrixQn, QBTestData.SingleChoiceMatrixQn_Title);
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.SingleChoiceMatrix);
    });
    await test.step('Verify that the Row Sort Order, Column Sort Order, Answer Min, and Answer Max are displayed when the Question Type is set as follows', async () => {
        await questionnaireService.validateTheDataAddedInQuestionnaire(QBTestData.SingleChoiceMatrixQn, DropDownList.QuestionBank.SingleChoiceMatrix, QBTestData.SingleChoiceMatrixQn_Title, QBTestData.SingleChoiceMatrixQn_Text, QBTestData.SingleChoiceMatrixQn_AnswerCode, QBTestData.SingleChoiceMatrixQn_AnswerText, TestData.ModuleName6_ScripterNotes, QBTestData.TestForma, TestData.SortOrder1, "", "", "");
        await webHelper.verifyText(QuestionBank.Title.RowSortOrder);
        await webHelper.verifyText(QuestionBank.Title.ColumnSortOrder);

    });

    await test.step('Add a Text Input Matrix Question to the Project', async () => {
        await questionnaireService.addQuestion(QBTestData.TextInputMatrixQn, QBTestData.TextInputMatrixQn_Title);
        await questionnaireService.expandQuestionnaire(DropDownList.QuestionBank.TextInputMatrix);
    });
    await test.step('Verify that the Row Sort Order, Answer Min, and Answer Max are displayed when the Question Type is set as follows', async () => {
        await questionnaireService.validateTheDataAddedInQuestionnaire(QBTestData.MultipleChoiceMatrixQn, DropDownList.QuestionBank.TextInputMatrix, QBTestData.TextInputMatrixQn_Title, QBTestData.TextInputMatrixQn_Text, TestData.ModuleName6_AnswerCode, QBTestData.AnswerText, TestData.ModuleName6_ScripterNotes, TestData.ModuleName6_QuestionFormatDetails, TestData.SortOrder2, "", "", "");
        await webHelper.verifyText(QuestionBank.Title.RowSortOrder);
    });
    await test.step('Delete the Created Project ', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});
test("[2538876]  To check if Reactivate button is NOT available for active question", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2538876' });
    const projectService = new ProjectService(page);
    const questionnaireService = new Questionnaireservice(page);

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
        await questionnaireService.addQuestion(TestData.StandardQuestion, Questionnaire.Text.CategoryIntroduction);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.StandardQuest_VariableName1, DropDownList.QuestionBank.SmallTextInput, TestData.StandardQuestionText2);
    });

    await test.step('check if each row contains reactivate Test or trash icon for active QL', async () => {
        await questionnaireService.verifyQuestionnairebutton(Questionnaire.TestDataId.ReactivateButton, false);
        await questionnaireService.validateIconsForQuestionsAddedInQuestionnaire(TestData.StandardQuest_VariableName1);
        await questionnaireService.expandQuestionnaire(TestData.StandardQuest_VariableName1)
        await questionnaireService.verifyQuestionnairebutton(Questionnaire.TestDataId.ReactivateButton, false);

    });

    await test.step('Delete the Created Project ', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});

test("[2527546] To check if + button is visible in QL tab for Librarians and all functions of CS user applies for librarian", { tag: ['@Regression', '@Questionnaire'] }, async ({ page, browser }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2527546' });
    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);
    const questionnaireService = new Questionnaireservice(page);
    const projectName = "AUTO_TestPro" + Utils.generateGUID();

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.LibrarianUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.CSUser);
    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
    });
    await test.step('Add a Standard Question to the Project', async () => {
        await questionnaireService.addQuestion(TestData.StandardQuestion, Questionnaire.Text.CategoryIntroduction);
        await questionnaireService.validateQuestion(TestData.StandardQuest_VariableName1);
    });

    await test.step('Add a Module to the Project', async () => {
        await questionnaireService.addModule(TestData.ModuleName);
        await questionnaireService.validateModulesAddedInQuestionnaire(TestData.ModuleName, TestData.M_QuestionName, TestData.M_QuestionType, TestData.M_QuestionText);
    });

    await test.step('Add a Custom Question to the Project', async () => {
        await questionnaireService.addCustomQuestion(TestData.Custom);
        await questionnaireService.validateQuestion(TestData.Custom);
    });
    await test.step('Now add more than one module by selecting them from results > Click on save', async () => {
        await questionnaireService.addModule(TestData.ModuleName1);
        await questionnaireService.addModule(TestData.ModuleName2);
        await questionnaireService.addModule(TestData.ModuleName4);
    });
    await test.step('Now add more than one question by selecting them from results > Click on save', async () => {
        await questionnaireService.addQuestion(QBTestData.MultipleChoiceQn, QBTestData.MultipleChoiceQnTitle);
        await questionnaireService.addQuestion(QBTestData.MultipleChoiceMatrixQn, QBTestData.MultipleChoiceMatrixQnTitle);
    });

    await test.step('Delete the Created Project ', async () => {
        var guid = await webHelper.fetchRecordGuid(page.url());
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});
test("[2532193] Verify that next to each module name in the side panel, it displays how many questions the module is associated to ", { tag: ['@Regression', '@Questionnaire'] }, async ({ page }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2532193' });
    const questionnaireService = new Questionnaireservice(page);
    const projectService = new ProjectService(page);


    const webHelper = new WebHelper(page);
    const projectName = "AUTO_TestProject_" + Utils.generateGUID();
    let questionsCount = 0;
    var guid = "";

    await test.step('Login into MDA application', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
        await webHelper.changeArea(Common.Text.CSUser);
    });

    await test.step('Go to project > Create a Project', async () => {
        await projectService.CreateProject(projectName);
        guid = await webHelper.fetchRecordGuid(page.url());

    });
    await test.step('Add a Module to the Project', async () => {
        await questionnaireService.addModule(CommonTestData.ModuleWithDummyQuestion);
        questionsCount = await projectService.getMasterQuestionnaireLinesCount();

    });
    await test.step('Click on the "Remove Modules" button ', async () => {
        await questionnaireService.clickOnRemoveModule();

    });
    await test.step('Verify that next to each module name in the side panel, it displays how many questions the module is associated to', async () => {
        await questionnaireService.expandModuleInRemoveModuleSection(CommonTestData.ModuleWithDummyQuestion);
        const count = await questionnaireService.getCountofQuestionsInRemoveModule();
        expect(questionsCount).toBe(count);
    });

    await test.step('Delete the Created Project ', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});
