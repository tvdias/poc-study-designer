import { logintest } from '../Fixture/LoginFixture';
import { expect, mergeTests } from '@playwright/test';
import { createproject } from '../Fixture/CreateProjectWithStdQue';
import { LoginToMDAWithTestUser } from '../../utils/Login';
import { TestUser, AppName, AppId } from '../../constants/TestUsers.json';
import { Utils } from "../../utils/utils";
import { DropDownList } from '../../constants/DropDownList.json';
import { WebHelper } from '../../utils/WebHelper';
import { CommonTestData } from '../../Test Data/CommonTestData.json';
import { Common } from '../../selectors/CommonSelector.json';
import { Questionnaire } from '../../selectors/QuestionnaireSelector.json';
import { QuestionBankservice } from '../../services/QuestionBankService';
import { Questionnaireservice } from '../../services/QuestionnaireService';
import { TestData } from '../../Test Data/QuestionnnaireData.json';
import { deleteRecord } from '../../utils/APIHelper';
import { EntityLogicalNames } from '../../constants/CommonLogicalNames.json';
import { ProjectService } from '../../services/ProjectService';
import { ManagedListService } from '../../services/ManagedListService';
import { TestData as MLTestData } from '../../Test Data/ManagedListData.json';
import { ManagedList } from '../../selectors/ManagedListSelectors.json';
import { StudyTestData } from '../../Test Data/StudyData.json';
import { StudyService } from '../../services/StudyService';


// Using a LoginFixture and CreateProjectWithStdQue to handle setup and teardown for all test cases:
// 1. Login the application with CS user
// 2. Create the project and add a question before each test execution.
// 3. Delete the project after the each test case execution

// Merges the results of the login test and the create a Project test into a single combined test execution.
const test = mergeTests(logintest, createproject);


test("[2553796] Verify that a ML entity can be deactivated if it is not associated to any question but snapshot is generated.", { tag: ['@Regression', '@MLEntity'] }, async ({ page, browser,loginPage,projectName,guid }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2553796' });

    const webHelper = new WebHelper(page);
    const questionnaireService = new Questionnaireservice(page);
    const projectService = new ProjectService(page);
    const managedListService = new ManagedListService(page);
    const studyService = new StudyService(page);

    const questionName = TestData.StandardQuestion;
    let managedListName = MLTestData.ManagedListName + Utils.generateText();


    await test.step('Create and Validate the Manage list from Project', async () => {
        await managedListService.selectManagedListTab();
        await managedListService.create(managedListName);
        await managedListService.clickonSaveRecord();
    });

    await test.step('Associate the added question from project to ML', async () => {
        await managedListService.clickOnManagedList(managedListName);
        await managedListService.addQuestion(managedListName, "");
        await managedListService.verifyAddedQuestionDetails(TestData.StandardQuest_VariableName1, MLTestData.Location, projectName);
        await webHelper.saveRecord();
    });

    await test.step('Create Managed List Entity', async () => {
        await questionnaireService.navigateToTheTab(ManagedList.Tabs.Entities);
        await managedListService.clickOnNewManagedListEntityButton();
        await managedListService.verifyQuickCreateWindow();
        await managedListService.fillAnswerText(TestData.AnswerText1);
        await webHelper.saveAndCloseQuickCreateRecord();
        await webHelper.verifySaveButton();
        await webHelper.saveAndCloseRecord();
    });

    await test.step('Create a study', async () => {
        await projectService.addScripterUserToProject();
        const studyName = StudyTestData.StudyName + Utils.generateGUID();
        await studyService.CreateNewStudy(studyName, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await webHelper.handleConfirmationPopup();
        await studyService.ValidateInitialDraftStateStudy();
    });

    await test.step('Update study from Draft to Ready for Scritping', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();
    });

    await test.step('Delete the question from the Managed list', async () => {
        await managedListService.selectManagedListTab();
        await managedListService.clickOnManagedList(managedListName);
        await managedListService.deleteQuestionsInManagedList(questionName);

    });

    await test.step('Deactivate the Managed list Entity', async () => {
        await questionnaireService.navigateToTheTab(ManagedList.Tabs.Entities);
        await managedListService.openMLEntity(TestData.AnswerText1);
        await managedListService.deactivateManagedListEntity();
    });

});

test("[2545576] To check if ML can be deleted from the ML list if one or more question associated with it is referenced in snapshot.", { tag: ['@Regression', '@MLEntity'] }, async ({ page, browser ,loginPage,projectName,guid}) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2545576' });

    const webHelper = new WebHelper(page);
    const projectService = new ProjectService(page);
    const managedListService = new ManagedListService(page);
    const studyService = new StudyService(page);

    const studyName = StudyTestData.StudyName + Utils.generateGUID();
    const questionName = TestData.StandardQuestion;
    let managedListName = MLTestData.ManagedListName + Utils.generateText();

    await test.step('Create a study', async () => {
        await projectService.addScripterUserToProject();
        await studyService.CreateNewStudy(studyName, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await webHelper.handleConfirmationPopup();
        await studyService.ValidateInitialDraftStateStudy();
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();
    });

    await test.step('Create the Manage list from Project', async () => {
        await managedListService.selectManagedListTab();
        await managedListService.create(managedListName);
        await managedListService.clickonSaveRecord();
    });

    await test.step('Associate the added question from project to ML', async () => {
        await managedListService.addQuestion(managedListName, questionName);
        await managedListService.verifyAddedQuestionDetails(TestData.StandardQuest_VariableName1, MLTestData.Location, projectName);
        await webHelper.saveRecord();
        await webHelper.saveAndCloseRecord();
    });

    await test.step('Update study from Draft to Ready for Scritping', async () => {
        await studyService.openStudy(studyName);
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
        await webHelper.saveRecord();
    });

    await test.step('Validate snapshots are generated correctly & questions order', async () => {
        await studyService.ValidateStudySnapshotQuestions();
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();

    });

    await test.step('Try to delete the Managed List by opening the ML form ', async () => {
        await managedListService.selectManagedListTab();
        await managedListService.clickOnManagedList(managedListName);
        await managedListService.deleteManagedList();
        await managedListService.VerifyErrorMessage();
        await webHelper.clickOnConfirmationPopup(Common.Text.OK)
        await webHelper.saveRecord();
        await webHelper.clickGoBackArrow();
        await webHelper.verifySaveButton();
    });

    await test.step('Verify if Delete Button is visible in the contextual menu', async () => {
        await managedListService.selectTheManagedListInGrid(managedListName);
        await managedListService.verifyDeleteButtonFromMLContextMenu();
    });

});
