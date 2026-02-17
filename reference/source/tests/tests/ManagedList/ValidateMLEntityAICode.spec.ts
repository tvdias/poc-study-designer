import { expect, test } from '../Fixture/ManagedListFixture.ts';
import { LoginToMDAWithTestUser } from '../../utils/Login';
import { TestUser, AppName, AppId } from '../../constants/TestUsers.json';
import { WebHelper } from '../../utils/WebHelper';
import { Questionnaireservice } from '../../services/QuestionnaireService';
import { TestData } from '../../Test Data/QuestionnnaireData.json';
import { ProjectService } from '../../services/ProjectService';
import { ManagedListService } from '../../services/ManagedListService';
import { TestData as MLTestData } from '../../Test Data/ManagedListData.json';
import { ManagedList } from '../../selectors/ManagedListSelectors.json';
import { Common } from '../../selectors/CommonSelector.json';
import { StudyTestData } from '../../Test Data/StudyData.json';
import { StudyService } from '../../services/StudyService';
import { Utils } from "../../utils/utils";
import { DropDownList } from '../../constants/DropDownList.json';
import { Questionnaire } from '../../selectors/QuestionnaireSelector.json';


// Using a ManagedListFixture to handle setup and teardown for all test cases:
// 1. Login the application with CS user
// 2. Create the project and add a question before each test execution.
// 3. Create a Manged List to the Project
// 4. Delete the project after the each test case execution

test("[2553042] CS user must not be allowed to enter the Answer Code initially while creating the Managed List entity record for the manually generated Managed List", { tag: ['@Regression', '@MLEntity'] }, async ({ loginPage, page, browser, projectName, managedListName, guid }) => {

    test.info().annotations.push({ type: 'TestCaseId', description: '2553042' });

    const webHelper = new WebHelper(page);
    const questionnaireService = new Questionnaireservice(page);
    const managedListService = new ManagedListService(page);

    await test.step('Go to Entities tab and Click on the New ML Entity button', async () => {
        await questionnaireService.navigateToTheTab(ManagedList.Tabs.Entities);
        await managedListService.clickOnNewManagedListEntityButton();
        await managedListService.verifyQuickCreateWindow();
    });
    await test.step('Enter Answer Text and check if Answer Code is AI generated', async () => {
        await managedListService.fillAnswerText(TestData.AnswerText);
        await webHelper.verifyFieldReadonly(ManagedList.AriaLabel.AnswerCode);
        await webHelper.saveAndCloseQuickCreateRecord();
        const aiAnswerCode = await managedListService.validateAICodeForAnswerText();
        await expect(aiAnswerCode.length).toBeGreaterThan(0);
        await expect(aiAnswerCode).toContain(TestData.AnswerText.toUpperCase());

    });


});

test("[2553044] Check whether the Answer Codes are allowed to edit only when the toggle button is ON", { tag: ['@Regression', '@MLEntity'] }, async ({ loginPage, page, browser, projectName, managedListName, guid }) => {

    test.info().annotations.push({ type: 'TestCaseId', description: '2553044' });

    const webHelper = new WebHelper(page);
    const questionnaireService = new Questionnaireservice(page);
    const managedListService = new ManagedListService(page);

    await test.step('Go to Entities tab and Click on the New ML Entity button', async () => {
        await questionnaireService.navigateToTheTab(ManagedList.Tabs.Entities);
        await managedListService.clickOnNewManagedListEntityButton();
        await managedListService.verifyQuickCreateWindow();
    });
    await test.step('Enter Answer Text and check if Answer Code is AI generated', async () => {
        await managedListService.fillAnswerText(TestData.AnswerText);
        await webHelper.verifyFieldReadonly(ManagedList.AriaLabel.AnswerCode);
        await webHelper.saveAndCloseQuickCreateRecord();
    });
    await test.step('Goto entities and open an entity to edit when toggle button is ON', async () => {
        await managedListService.toggleAnswerCodeEditable();
    });
    await test.step('Edit the Answer Code and check whether AI is generated the new Answer code', async () => {
        await questionnaireService.clickOnAnswerText();
        await questionnaireService.updateAnswerText(TestData.AnswerText5);

        const aiAnswerCode = await managedListService.validateAICodeForAnswerText();
        await expect(aiAnswerCode.length).toBeGreaterThan(0);
        await expect(aiAnswerCode).toContain("UK");
        await expect(aiAnswerCode).toContain("BEAUTIFUL");

    });


});
test("[2553052] Check Scripter cannot edit the Answer Code of a manually created managed list without needing the toggle - toggle OFF scenario", { tag: ['@Regression', '@MLEntity'] }, async ({ loginPage, page, browser, projectName, managedListName, guid }) => {

    test.info().annotations.push({ type: 'TestCaseId', description: '2553052' });

    const webHelper = new WebHelper(page);
    const questionnaireService = new Questionnaireservice(page);
    const managedListService = new ManagedListService(page);
    const projectService = new ProjectService(page);

    await test.step('Go to Entities tab and Click on the New ML Entity button', async () => {
        await questionnaireService.navigateToTheTab(ManagedList.Tabs.Entities);
        await managedListService.clickOnNewManagedListEntityButton();
        await managedListService.verifyQuickCreateWindow();
    });
    await test.step('Enter Answer Text and check if Answer Code is AI generated', async () => {
        await managedListService.fillAnswerText(TestData.AnswerText);
        await webHelper.verifyFieldReadonly(ManagedList.AriaLabel.AnswerCode);
        await webHelper.saveAndCloseQuickCreateRecord();
        await webHelper.verifySaveButton();
        await webHelper.saveAndCloseRecord();
        await webHelper.verifySaveButton();
        await webHelper.saveAndCloseRecord();
        await webHelper.verifySaveButton();
    });
    await test.step('Add Scripter user to the Project', async () => {
        await projectService.addScripterUserToProject();
    });
    const context = await browser.newContext();
    const scripterUserPage = await context.newPage();
    const webHelperSecond = new WebHelper(scripterUserPage);
    const newProjectService = new ProjectService(scripterUserPage);
    const scripterUserManagedListService = new ManagedListService(scripterUserPage);
    const scripterUserquestionnaireService = new Questionnaireservice(scripterUserPage);

    await test.step('Login into MDA application with Scripter User', async () => {
        await LoginToMDAWithTestUser(scripterUserPage, TestUser.ScripterUser, AppId.UC1, AppName.UC1);
    });
    await test.step('Open the project ', async () => {
        await newProjectService.searchForProject(projectName);
    });
    await test.step('Navigate to Projects -> Managed Lists tab', async () => {
        await scripterUserquestionnaireService.navigateToTheTab(ManagedList.Tabs.ManagedList);
    });

    await test.step('Verify that the user is able to view defined ML ', async () => {
        await scripterUserManagedListService.verifyAddedManagedListDetails(managedListName, MLTestData.IsAutoGenerated_No);
        await scripterUserManagedListService.selectTheManagedList(managedListName);
        await scripterUserquestionnaireService.navigateToTheTab(ManagedList.Tabs.Entities);
    });

    await test.step('Edit the answer code when the toggle is OFF  ', async () => {
        await scripterUserquestionnaireService.clickOnAnswerText();
        await webHelperSecond.verifyFieldReadonly(ManagedList.AriaLabel.AnswerText);

    });

});

test("[2545596] To check if ML can be deleted if there exists questions which are NOT in propagated studies but in available in ML", { tag: ['@Regression', '@MLEntity'] }, async ({ loginPage, page, browser, projectName, managedListName, guid }) => {

    test.info().annotations.push({ type: 'TestCaseId', description: '2545596' });

    const webHelper = new WebHelper(page);
    const managedListService = new ManagedListService(page);

    await test.step('associate the added question from project to ML', async () => {
        await managedListService.addQuestion(managedListName, "");
        await managedListService.verifyAddedQuestionDetails(TestData.StandardQuest_VariableName1, MLTestData.Location, projectName);
        await webHelper.saveRecord();
    });
    await test.step('Try to delete the Managed List ', async () => {
        await managedListService.deleteManagedList();
        await managedListService.VerifyErrorMessage();
        await webHelper.clickOnConfirmationPopup(Common.Text.OK)
    });
    await test.step('Now de-associate the question from Project Questionnaire by deleting the question  ', async () => {
        await managedListService.deleteQuestionsInManagedList(TestData.StandardQuestion);
    });
    await test.step('Check if delete pop up contains Cancel and Delete button and Click on Cancel button', async () => {
        await managedListService.verifyConfirmDeletionPopup();
        await webHelper.clickOnButton(Common.Text.QCCancel);
        await managedListService.verifyTheManagedList(managedListName, true);
    });
    await test.step('Click on Delete from Confirmation Popup  and Validate the Deleted ManagedList', async () => {
        await managedListService.deleteManagedList();
        await managedListService.verifyTheManagedList(managedListName, false);
        await managedListService.verifyTheTab(ManagedList.Tabs.ManagedList);
        await managedListService.verifyAddedManagedListDetails(managedListName, MLTestData.IsAutoGenerated_No, false)
    });

});
test("[2592859] Verify when the search text given in both the search boxes then proper results are displayed", { tag: ['@Regression', '@MLEntity'] }, async ({ loginPage, page, browser, projectName, managedListName, guid }) => {

    test.info().annotations.push({ type: 'TestCaseId', description: '2592859' });

    const webHelper = new WebHelper(page);
    const questionnaireService = new Questionnaireservice(page);
    const managedListService = new ManagedListService(page);
    const projectService = new ProjectService(page);
    const studyService = new StudyService(page);

    const studyName1 = StudyTestData.StudyName + Utils.generateGUID();
    const studyName2 = StudyTestData.StudyName + Utils.generateGUID();

    await test.step('associate the added question from project to ML', async () => {
        await managedListService.addQuestion(managedListName, "");
        await managedListService.verifyAddedQuestionDetails(TestData.StandardQuest_VariableName1, MLTestData.Location, projectName);
        await webHelper.saveRecord();
    });
    await test.step('Add more than one Entities to the Managed List ', async () => {
        await questionnaireService.navigateToTheTab(ManagedList.Tabs.Entities);

        const answerText: string[] = [TestData.AnswerText, TestData.AnswerText1, TestData.AnswerText2]
        for (let i = 0; i < answerText.length; i++) {
            await managedListService.clickOnNewManagedListEntityButton();
            await managedListService.verifyQuickCreateWindow();
            await managedListService.fillAnswerText(answerText[i]);
            await webHelper.saveAndCloseQuickCreateRecord();
            await webHelper.verifySaveButton();
        }
        await webHelper.saveAndCloseRecord();

    });
    await test.step('Create studies with Draft, Ready for Scripting status', async () => {
        await projectService.addScripterUserToProject();
        await studyService.CreateNewStudy(studyName1, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await webHelper.handleConfirmationPopup();
        await studyService.ValidateInitialDraftStateStudy();
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();
        await studyService.CreateNewStudy(studyName2, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await webHelper.handleConfirmationPopup();
        await studyService.ValidateInitialDraftStateStudy();
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();
    });

    await test.step('Goto Project> Managed List > Study Allocation tab', async () => {
        await managedListService.selectManagedListTab();
        await managedListService.selectTheManagedList(managedListName);
        await questionnaireService.navigateToTheTab(ManagedList.Tabs.StudyAllocation);

    });
    await test.step('Enter the search word/char in both the search boxes ', async () => {
        await managedListService.verifyEntity(TestData.AnswerText)
        await managedListService.verifyEntity(TestData.AnswerText1)
        await managedListService.verifyEntity(TestData.AnswerText2)
        await managedListService.filterEntities(TestData.AnswerCode2);
        await managedListService.verifyEntity(TestData.AnswerText, false)
        await managedListService.verifyEntity(TestData.AnswerText1, false)
        await managedListService.verifyEntity(TestData.AnswerText2)

    });

});
test("[2592756] When Select the entites then count should be updated", { tag: ['@Regression', '@MLEntity'] }, async ({ loginPage, page, browser, projectName, managedListName, guid }) => {

    test.info().annotations.push({ type: 'TestCaseId', description: '2592756' });

    const webHelper = new WebHelper(page);
    const questionnaireService = new Questionnaireservice(page);
    const managedListService = new ManagedListService(page);
    const projectService = new ProjectService(page);
    const studyService = new StudyService(page);

    const studyName1 = StudyTestData.StudyName + Utils.generateGUID();
    const studyName2 = StudyTestData.StudyName + Utils.generateGUID();
    let intialCount = 0;
    let answerText: string[];

    await test.step('associate the added question from project to ML', async () => {
        await managedListService.addQuestion(managedListName, "");
        await managedListService.validateAndVerifySave();
    });
    await test.step('Add 2 Entities to the Managed List ', async () => {
        await questionnaireService.navigateToTheTab(ManagedList.Tabs.Entities);

        answerText = [TestData.AnswerText, TestData.AnswerText1]
        for (let i = 0; i < answerText.length; i++) {
            await managedListService.clickOnNewManagedListEntityButton();
            await managedListService.verifyQuickCreateWindow();
            await managedListService.fillAnswerText(answerText[i]);
            await webHelper.saveAndCloseQuickCreateRecord();
            await webHelper.verifySaveButton();
        }
        await webHelper.saveAndCloseRecord();

    });
    await test.step('Create studies with Draft, Ready for Scripting status', async () => {
        await projectService.addScripterUserToProject();
        await studyService.CreateNewStudy(studyName1, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await webHelper.handleConfirmationPopup();
        await studyService.ValidateInitialDraftStateStudy();
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();
        await studyService.CreateNewStudy(studyName2, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await webHelper.handleConfirmationPopup();
        await studyService.ValidateInitialDraftStateStudy();
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();
    });

    await test.step('Goto Project> Managed List > Study Allocation tab', async () => {
        await managedListService.selectManagedListTab();
        await managedListService.selectTheManagedList(managedListName);
        await managedListService.clickonSaveRecord();

    });
    await test.step('Check the count of entites displayed at the study label ', async () => {
        intialCount = await managedListService.getEntitiesCountInStudyAllocation();
        await expect(intialCount).toBe(answerText.length);
    });
    await test.step('Create a new entity in the ML ', async () => {
        await questionnaireService.navigateToTheTab(ManagedList.Tabs.Entities);
        await managedListService.clickOnNewManagedListEntityButton();
        await managedListService.verifyQuickCreateWindow();
        await managedListService.fillAnswerText(TestData.AnswerCode2);
        await webHelper.saveAndCloseQuickCreateRecord();
        await managedListService.clickonSaveRecord();
    });
    await test.step('Go to Study Allocation tab and validate the newly added entity ', async () => {
        const finalcount = await managedListService.getEntitiesCountInStudyAllocation();
        await expect(finalcount).toBe(answerText.length + 1);
    });

});

test("[2553048] Check whether the CS user can edit the managed list entity when the answer is included in the Draft version of the study", { tag: ['@Regression', '@MLEntity'] }, async ({ loginPage, page, browser, projectName, managedListName, guid }) => {

    test.info().annotations.push({ type: 'TestCaseId', description: '2553048' });

    const webHelper = new WebHelper(page);
    const questionnaireService = new Questionnaireservice(page);
    const managedListService = new ManagedListService(page);
    const studyService = new StudyService(page);

    const studyName1 = StudyTestData.StudyName + Utils.generateGUID();

    await test.step('associate the added question from project to ML', async () => {
        await managedListService.addQuestion(managedListName, "");
        await managedListService.validateAndVerifySave();
    });

    await test.step('Go to Entities tab and Click on the New ML Entity button', async () => {
        await questionnaireService.navigateToTheTab(ManagedList.Tabs.Entities);
        await managedListService.clickOnNewManagedListEntityButton();
        await managedListService.verifyQuickCreateWindow();
    });
    await test.step('Enter Answer Text and check if Answer Code is AI generated', async () => {
        await managedListService.fillAnswerText(TestData.AnswerText);
        await webHelper.verifyFieldReadonly(ManagedList.AriaLabel.AnswerCode);
        await webHelper.saveAndCloseQuickCreateRecord();
    });
    await test.step('Goto entities and open an entity to edit when toggle button is ON', async () => {
        await managedListService.toggleAnswerCodeEditable();
        await webHelper.saveAndCloseRecord();
    });
    await test.step('Create a study and keep in draft state', async () => {
        await studyService.CreateNewStudy(studyName1, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await webHelper.handleConfirmationPopup();
        await studyService.ValidateInitialDraftStateStudy();
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();
    });
    await test.step('Go to managed list tab and open the managed list included in study', async () => {
        await managedListService.selectManagedListTab();
        await webHelper.verifySaveButton();
        await managedListService.selectTheManagedList(managedListName);
        await webHelper.verifySaveButton();
        await questionnaireService.navigateToTheTab(ManagedList.Tabs.Entities);
        await webHelper.verifySaveButton();
    });

    await test.step('Edit the Answer Code', async () => {
        await questionnaireService.clickOnAnswerText();
        await questionnaireService.updateAnswerCode(TestData.AnswerCode1);
    });

    await test.step('Validate the Updated Answer Code', async () => {
        const answerCode = await managedListService.validateAICodeForAnswerText();
        await expect(answerCode).toBe(TestData.AnswerCode1);

    });


});
test("[2553049] Check whether the CS user can edit the managed list entity when the answer is included in the Ready For Scripting version of the study", { tag: ['@Regression', '@MLEntity'] }, async ({ loginPage, page, browser, projectName, managedListName, guid }) => {

    test.info().annotations.push({ type: 'TestCaseId', description: '2553049' });

    const webHelper = new WebHelper(page);
    const questionnaireService = new Questionnaireservice(page);
    const managedListService = new ManagedListService(page);
    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);

    const studyName1 = StudyTestData.StudyName + Utils.generateGUID();

    await test.step('associate the added question from project to ML', async () => {
        await managedListService.addQuestion(managedListName, "");
        await managedListService.validateAndVerifySave();
    });

    await test.step('Go to Entities tab and Click on the New ML Entity button', async () => {
        await questionnaireService.navigateToTheTab(ManagedList.Tabs.Entities);
        await managedListService.clickOnNewManagedListEntityButton();
        await managedListService.verifyQuickCreateWindow();
    });
    await test.step('Enter Answer Text and check if Answer Code is AI generated', async () => {
        await managedListService.fillAnswerText(TestData.AnswerText);
        await webHelper.verifyFieldReadonly(ManagedList.AriaLabel.AnswerCode);
        await webHelper.saveAndCloseQuickCreateRecord();
    });
    await test.step('Goto entities and open an entity to edit when toggle button is ON', async () => {
        await managedListService.toggleAnswerCodeEditable();
        await webHelper.saveAndCloseRecord();
    });
    await test.step('Create a study and Change to Ready for scripting state', async () => {
        await projectService.addScripterUserToProject();
        await studyService.CreateNewStudy(studyName1, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await webHelper.handleConfirmationPopup();
        await studyService.ValidateInitialDraftStateStudy();
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();
    });

    await test.step('Go to managed list tab and open the managed list included in study and Naviagte to Entity tab', async () => {
        await managedListService.selectManagedListTab();
        await webHelper.verifySaveButton();
        await managedListService.selectTheManagedList(managedListName);
        await webHelper.verifySaveButton();
        await questionnaireService.navigateToTheTab(ManagedList.Tabs.Entities);
        await webHelper.verifySaveButton();
    });

    await test.step('Edit the Answer Code', async () => {
        await questionnaireService.clickOnAnswerText();
        await questionnaireService.updateAnswerCode(TestData.AnswerCode1);

    });
    await test.step('Validate the Updated Answer code', async () => {
        const answerCode = await managedListService.validateAICodeForAnswerText();
        await expect(answerCode).toBe(TestData.AnswerCode1);

    });


});
test("[2553050] Validate that CS user is not allowed to edit the Answer code when the study is in 'Approved For Launch'", { tag: ['@Regression', '@MLEntity'] }, async ({ loginPage, page, browser, projectName, managedListName, guid }) => {

    test.info().annotations.push({ type: 'TestCaseId', description: '2553050' });

    const webHelper = new WebHelper(page);
    const questionnaireService = new Questionnaireservice(page);
    const managedListService = new ManagedListService(page);
    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);

    const studyName1 = StudyTestData.StudyName + Utils.generateGUID();

    await test.step('associate the added question from project to ML', async () => {
        await managedListService.addQuestion(managedListName, "");
        await managedListService.validateAndVerifySave();
    });

    await test.step('Go to Entities tab and Click on the New ML Entity button', async () => {
        await questionnaireService.navigateToTheTab(ManagedList.Tabs.Entities);
        await managedListService.clickOnNewManagedListEntityButton();
        await managedListService.verifyQuickCreateWindow();
    });
    await test.step('Enter Answer Text and check if Answer Code is AI generated', async () => {
        await managedListService.fillAnswerText(TestData.AnswerText);
        await webHelper.verifyFieldReadonly(ManagedList.AriaLabel.AnswerCode);
        await webHelper.saveAndCloseQuickCreateRecord();
    });
    await test.step('Goto entities and open an entity to edit when toggle button is ON', async () => {
        await managedListService.toggleAnswerCodeEditable();
        await webHelper.saveAndCloseRecord();
    });
    await test.step('Create a study and keep in Draft state', async () => {
        await projectService.addScripterUserToProject();
        await studyService.CreateNewStudy(studyName1, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await webHelper.handleConfirmationPopup();
        await studyService.ValidateInitialDraftStateStudy();
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
        await webHelper.saveRecord();
        await studyService.updateStudyStatus(DropDownList.Status.ApprovedforLaunch);
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();
    });

    await test.step('Goto managed list tab and open the managed list included in study', async () => {
        await managedListService.selectManagedListTab();
        await webHelper.verifySaveButton();
        await managedListService.selectTheManagedList(managedListName);
        await webHelper.verifySaveButton();
        await questionnaireService.navigateToTheTab(ManagedList.Tabs.Entities);
        await webHelper.verifySaveButton();
    });

    await test.step('Check Answer Code field should be Not Editable for Approved Launch State Study', async () => {
        await questionnaireService.clickOnAnswerText();
        await webHelper.verifySaveButton();
        await webHelper.verifyNewButton();
        await webHelper.verifyFieldReadonly(ManagedList.AriaLabel.AnswerCode);
    });

});

test("[2553056] Check whether the Scripter user can edit the managed list entity when the answer is included in the Draft version of the study", { tag: ['@Regression', '@MLEntity'] }, async ({ loginPage, page, browser, projectName, managedListName, guid }) => {

    test.info().annotations.push({ type: 'TestCaseId', description: '2553056' });

    const webHelper = new WebHelper(page);
    const questionnaireService = new Questionnaireservice(page);
    const managedListService = new ManagedListService(page);
    const projectService = new ProjectService(page);
    const studyService = new StudyService(page);

    const studyName1 = StudyTestData.StudyName + Utils.generateGUID();

    await test.step('associate the added question from project to ML', async () => {
        await managedListService.addQuestion(managedListName, "");
        await managedListService.validateAndVerifySave();
    });

    await test.step('Go to Entities tab and Click on the New ML Entity button', async () => {
        await questionnaireService.navigateToTheTab(ManagedList.Tabs.Entities);
        await managedListService.clickOnNewManagedListEntityButton();
        await managedListService.verifyQuickCreateWindow();
    });
    await test.step('Create a Entity', async () => {
        await managedListService.fillAnswerText(TestData.AnswerText);
        await webHelper.verifyFieldReadonly(ManagedList.AriaLabel.AnswerCode);
        await webHelper.saveAndCloseQuickCreateRecord();
        await webHelper.saveAndCloseRecord();

    });
    await test.step('Add Scripter user to the Project', async () => {
        await projectService.addScripterUserToProject();
    });
    await test.step('Create a study and keep in draft state', async () => {
        await studyService.CreateNewStudy(studyName1, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await webHelper.handleConfirmationPopup();
        await studyService.ValidateInitialDraftStateStudy();
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();
    });

    const context = await browser.newContext();
    const scripterUserPage = await context.newPage();
    const webhelperSecond = new WebHelper(scripterUserPage);
    const newProjectService = new ProjectService(scripterUserPage);
    const scripterUserManagedListService = new ManagedListService(scripterUserPage);
    const scripterUserquestionnaireService = new Questionnaireservice(scripterUserPage);

    await test.step('Login into MDA application with Scripter User', async () => {
        await LoginToMDAWithTestUser(scripterUserPage, TestUser.ScripterUser, AppId.UC1, AppName.UC1);
    });
    await test.step('Open the project ', async () => {
        await newProjectService.searchForProject(projectName);
    });
    await test.step('Navigate to Projects -> Managed Lists tab', async () => {
        await scripterUserquestionnaireService.navigateToTheTab(ManagedList.Tabs.ManagedList);
        await webhelperSecond.verifyNewButton();
    });

    await test.step('Navigate to The Managed List and Then Entity tab ', async () => {
        await scripterUserManagedListService.selectTheManagedList(managedListName);
        await scripterUserquestionnaireService.navigateToTheTab(ManagedList.Tabs.Entities);
    });

    await test.step('Edit the AnswerCode and validate the Updated Answer Code ', async () => {
        await scripterUserquestionnaireService.clickOnAnswerText(false);
        await scripterUserquestionnaireService.updateAnswerCode(TestData.AnswerCode2);
        const answerCode = await scripterUserManagedListService.getAnswerCode();
        await expect(answerCode).toBe(TestData.AnswerCode2);


    });

});

test("[2553057] Check whether the Scripter user can edit the managed list entity when the answer is included in the Ready For Scripting version of the study", { tag: ['@Regression', '@MLEntity'] }, async ({ loginPage, page, browser, projectName, managedListName, guid }) => {

    test.info().annotations.push({ type: 'TestCaseId', description: '2553057' });

    const webHelper = new WebHelper(page);
    const questionnaireService = new Questionnaireservice(page);
    const managedListService = new ManagedListService(page);
    const projectService = new ProjectService(page);
    const studyService = new StudyService(page);

    const studyName1 = StudyTestData.StudyName + Utils.generateGUID();

    await test.step('associate the added question from project to ML', async () => {
        await managedListService.addQuestion(managedListName, "");
        await managedListService.validateAndVerifySave();
    });

    await test.step('Go to Entities tab and Click on the New ML Entity button', async () => {
        await questionnaireService.navigateToTheTab(ManagedList.Tabs.Entities);
        await managedListService.clickOnNewManagedListEntityButton();
        await managedListService.verifyQuickCreateWindow();
    });
    await test.step('Create a Entity', async () => {
        await managedListService.fillAnswerText(TestData.AnswerText);
        await webHelper.verifyFieldReadonly(ManagedList.AriaLabel.AnswerCode);
        await webHelper.saveAndCloseQuickCreateRecord();
        await webHelper.saveAndCloseRecord();

    });
    await test.step('Add Scripter user to the Project', async () => {
        await projectService.addScripterUserToProject();
    });
    await test.step('Create a study and keep in draft state', async () => {
        await studyService.CreateNewStudy(studyName1, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await webHelper.handleConfirmationPopup();
        await studyService.ValidateInitialDraftStateStudy();
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
        await webHelper.saveRecord();
    });

    const context = await browser.newContext();
    const scripterUserPage = await context.newPage();
    const webhelperSecond = new WebHelper(scripterUserPage);
    const newProjectService = new ProjectService(scripterUserPage);
    const scripterUserManagedListService = new ManagedListService(scripterUserPage);
    const scripterUserquestionnaireService = new Questionnaireservice(scripterUserPage);

    await test.step('Login into MDA application with Scripter User', async () => {
        await LoginToMDAWithTestUser(scripterUserPage, TestUser.ScripterUser, AppId.UC1, AppName.UC1);
    });
    await test.step('Open the project ', async () => {
        await newProjectService.searchForProject(projectName);
    });
    await test.step('Navigate to Projects -> Managed Lists tab', async () => {
        await scripterUserquestionnaireService.navigateToTheTab(ManagedList.Tabs.ManagedList);
        await webhelperSecond.verifyNewButton();
    });

    await test.step('Navigate to The Managed List and Then Entity tab ', async () => {
        await scripterUserManagedListService.selectTheManagedList(managedListName);
        await scripterUserquestionnaireService.navigateToTheTab(ManagedList.Tabs.Entities);
    });

    await test.step('Edit the Answer Codes of managed list entity  and Validate the Updated Answer Code ', async () => {
        await scripterUserquestionnaireService.clickOnAnswerText(false);
        await scripterUserquestionnaireService.updateAnswerCode(TestData.AnswerCode2);

        const answerCode = await scripterUserManagedListService.getAnswerCode();
        await expect(answerCode).toBe(TestData.AnswerCode2);


    });

});

test("[2592755] When deactivated the entities then count should be updated", { tag: ['@Regression', '@MLEntity'] }, async ({ loginPage, page, browser, projectName, managedListName, guid }) => {

    test.info().annotations.push({ type: 'TestCaseId', description: '2592755' });

    const webHelper = new WebHelper(page);
    const questionnaireService = new Questionnaireservice(page);
    const managedListService = new ManagedListService(page);
    const projectService = new ProjectService(page);
    const studyService = new StudyService(page);

    const studyName1 = StudyTestData.StudyName + Utils.generateGUID();
    let intialCount = 0;
    let answerText: string[];

    await test.step('associate the added question from project to ML', async () => {
        await managedListService.addQuestion(managedListName, "");
        await managedListService.validateAndVerifySave();
    });
    await test.step('Add 2 Entities to the Managed List ', async () => {
        await questionnaireService.navigateToTheTab(ManagedList.Tabs.Entities);

        answerText = [TestData.AnswerText, TestData.AnswerText1]
        for (let i = 0; i < answerText.length; i++) {
            await managedListService.clickOnNewManagedListEntityButton();
            await managedListService.verifyQuickCreateWindow();
            await managedListService.fillAnswerText(answerText[i]);
            await webHelper.saveAndCloseQuickCreateRecord();
            await webHelper.verifySaveButton();
        }
        await webHelper.saveAndCloseRecord();

    });
    await test.step('Create studies with Draft, Ready for Scripting status', async () => {
        await projectService.addScripterUserToProject();
        await studyService.CreateNewStudy(studyName1, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await webHelper.handleConfirmationPopup();
        await studyService.ValidateInitialDraftStateStudy();
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();
    });

    await test.step('Goto Project> Managed List > Study Allocation tab', async () => {
        await managedListService.selectManagedListTab();
        await managedListService.selectTheManagedList(managedListName);
        await managedListService.clickonSaveRecord();
    });
    await test.step('Check the count of entites displayed at the study label ', async () => {
        intialCount = await managedListService.getEntitiesCountInStudyAllocation();
        await expect(intialCount).toBe(answerText.length);
    });
    await test.step('Deactivate an entity from entities tab ', async () => {
        await questionnaireService.navigateToTheTab(ManagedList.Tabs.Entities);
        await questionnaireService.deactivateTheAnswer();
        await webHelper.clickGoBackArrow();
        await managedListService.clickonSaveRecord();
    });
    await test.step('Goto Study Allocation tab and Validate the Updated Count of Entities', async () => {
        const finalcount = await managedListService.getEntitiesCountInStudyAllocation();
        await expect(finalcount).toBe(answerText.length - 1);

    });

});
test("[2595051] Check the updated ML name is displayed in Questionnaire> Question", { tag: ['@Regression', '@MLEntity'] }, async ({ loginPage, page, browser, projectName, managedListName, guid }) => {

    test.info().annotations.push({ type: 'TestCaseId', description: '2595051' });

    const webHelper = new WebHelper(page);
    const questionnaireService = new Questionnaireservice(page);
    const managedListService = new ManagedListService(page);

    const updatedMLName = managedListName + "_Updated";

    await test.step('associate the added question from project to ML', async () => {
        await managedListService.addQuestion(managedListName, "");
        await managedListService.validateAndVerifySave();
        await webHelper.saveAndCloseRecord();

    });
    await test.step('Go to the Questionnaire Tab and expand the question details ', async () => {
        await questionnaireService.navigateToTheTab(Questionnaire.Tabs.Questionnaire);
        await questionnaireService.expandQuestionnaire(TestData.StandardQuestion);

    });
    await test.step('Check whether a hyperlink is available for the ML ', async () => {
        await questionnaireService.navigateToTheTab(Questionnaire.Tabs.Questionnaire);
        await managedListService.verifyAddedManagedListLinkInQuestionnaire(managedListName);
        await webHelper.verifySaveButton();

    });
    await test.step('Go to ML and update the name in ML ', async () => {
        await questionnaireService.navigateToTheTab(Questionnaire.Tabs.ManagedLists);
        await webHelper.verifySaveButton();
        await managedListService.selectTheManagedList(managedListName);
        await managedListService.updateTheMLName(updatedMLName);

    });

    await test.step('Check whether the update name is displayed in Questionnaire> Question ', async () => {
        await questionnaireService.navigateToTheTab(Questionnaire.Tabs.Questionnaire);
        await questionnaireService.expandQuestionnaire(TestData.StandardQuestion);
        await managedListService.verifyAddedManagedListLinkInQuestionnaire(managedListName);


    });

});