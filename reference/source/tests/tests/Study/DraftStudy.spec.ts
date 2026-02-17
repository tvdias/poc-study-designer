import { expect, test } from '@playwright/test';
import { ProjectService } from '../../services/ProjectService';
import { LoginToMDAWithTestUser } from '../../utils/Login';
import { Utils } from "../../utils/utils";
import { TestUser, AppName, AppId } from '../../constants/TestUsers.json';
import { StudyTestData } from '../../Test Data/StudyData.json';
import { DropDownList } from '../../constants/DropDownList.json';
import { StudyService } from '../../services/StudyService';
import { Questionnaireservice } from '../../services/QuestionnaireService';
import { WebHelper } from '../../utils/WebHelper';
import { deleteRecord } from '../../utils/APIHelper';
import { StudySelectors } from '../../selectors/StudySelectors.json';
import { TestData } from '../../Test Data/ProjectData.json';
import { EntityLogicalNames } from '../../constants/CommonLogicalNames.json';
import { Common } from '../../selectors/CommonSelector.json';
import { TestData as QuestionnaireTestData } from '../../Test Data/QuestionnnaireData.json';


test("[2459166,2363340,2374346,2374931] Create a Draft study & validate state reason, version number, questionnaire lines & buttons", { tag: ['@Regression', '@Study'] }, async ({ page }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2459166' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2363340' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2374346' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2374931' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);
    var guid: string;

    const projectName = TestData.ProjectName + Utils.generateGUID();

    await test.step('Navigating to URL', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
    });

    await test.step('Create a project', async () => {
        await projectService.CreateCustomProject(projectName, DropDownList.Project.CATI, TestData.Client, TestData.CommissioningMarket, projectName);
        guid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Add ProductTemplate to the Project', async () => {
        await projectService.addProductTemplate(TestData.Product1);
        await projectService.applyProductTemplate();
    });

    await test.step('Create a study & Compare questionnaire lines with master questionnaire lines', async () => {
        var masterQuestionnaireLinesCount = await projectService.getMasterQuestionnaireLinesCount();
        const masterQuestionnaireLinesVariableNames: string[] = await projectService.getQuestionnaireLinesVariableName();
        const studyName = StudyTestData.StudyName + Utils.generateGUID();
        await studyService.CreateNewStudy(studyName, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await studyService.ValidateInitialDraftStateStudy();
        await studyService.compareMasterQuestionnairesWithStudyQuestionnairesLines(masterQuestionnaireLinesCount, masterQuestionnaireLinesVariableNames);
    });

    await test.step('Validate Draft study visible buttons', async () => {
        const buttons: string[] = StudySelectors.ExpectedResults.DraftStudyButtons;
        await studyService.ValidateStudyButtons(buttons, 'Visible');
    });

    await test.step('Validate export to word document functionality', async () => {
        await studyService.ValidateCreateDocumentFunctionality();
    });

    await test.step('Clean up created project record', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});


test("[2445492,2445489,2445490] Create a Draft study & validate Field Work Language", { tag: ['@Regression', '@Study'] }, async ({ page }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2445492' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2445489' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2445490' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);
    var guid: string;

    const projectName = TestData.ProjectName + Utils.generateGUID();

    await test.step('Navigating to URL', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
    });

    await test.step('Create a project', async () => {
        await projectService.CreateCustomProject(projectName, DropDownList.Project.CATI, TestData.Client, TestData.CommissioningMarket, projectName);
        guid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Add ProductTemplate to the Project', async () => {
        await projectService.addProductTemplate(TestData.Product1);
        await projectService.applyProductTemplate();
    });

    await test.step('Create a study & Compare questionnaire lines with master questionnaire lines', async () => {
        var masterQuestionnaireLinesCount = await projectService.getMasterQuestionnaireLinesCount();
        const masterQuestionnaireLinesVariableNames: string[] = await projectService.getMasterQuestionnaireLinesVariableName();
        const studyName = StudyTestData.StudyName + Utils.generateGUID();
        await studyService.CreateNewStudy(studyName, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await studyService.ValidateInitialDraftStateStudy();
    });

    await test.step('Add the Fieldwork Languages', async () => {
        await studyService.AddFieldWorkMarketLanguage(StudyTestData.Langauge1);
        await studyService.AddFieldWorkMarketLanguage(StudyTestData.Langauge2);

    });

    await test.step('Validate Draft study the Fieldwork Languages', async () => {
        await studyService.ValidateFieldworkMarketLanguages(StudyTestData.Langauge1, 'visible');
        await studyService.ValidateFieldworkMarketLanguages(StudyTestData.Langauge2, 'visible');

    });

    await test.step('Clean up created project record', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});
test("[2580140] Check the deactivated question is not appearing in new study ", { tag: ['@Regression', '@Study'] }, async ({ page }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2580140' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);
    var guid: string;

    const projectName = TestData.ProjectName + Utils.generateGUID();
    const studyName = StudyTestData.StudyName + Utils.generateGUID();
    let deactivatedQuestion = "";

    await test.step('Navigating to URL', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
    });

    await test.step('Create a project', async () => {
        await projectService.CreateCustomProject(projectName, DropDownList.Project.CATI, TestData.Client, TestData.CommissioningMarket, projectName);
        guid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Add ProductTemplate to the Project', async () => {
        await projectService.addProductTemplate(TestData.Product1);
        await projectService.applyProductTemplate();
    });

    await test.step('Create a study ', async () => {
        var masterQuestionnaireLinesCount = await projectService.getMasterQuestionnaireLinesCount();
        await studyService.CreateNewStudy(studyName, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await studyService.ValidateInitialDraftStateStudy();
        const questionCount = await studyService.getMasterQuestionnaireLinesCount();
        expect(masterQuestionnaireLinesCount).toBe(questionCount);

    });

    await test.step('Deactivate the Question from Questionnaire', async () => {
        await studyService.goToProject(projectName);
        await projectService.expandFirstQuestionFromQuestionnaire();
        await webHelper.clickonStartWithText("Edit");
        deactivatedQuestion = await projectService.clickonButton(Common.Text.Deactivate);
        await page.goBack();
    });

    await test.step('Verify the Deactivated Question in Study', async () => {
        await studyService.openStudy(studyName);
        await webHelper.verifyTheLabeltext(deactivatedQuestion, false);
        await webHelper.verifyTheEntity(deactivatedQuestion, false);
    });

    await test.step('Clean up created project record', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});

test("[2582211] Deactivate a question from study and check question appears in deactivated study questionnaire", { tag: ['@Regression', '@Study'] }, async ({ page }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2582211' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const questionnaireService = new Questionnaireservice(page);


    const webHelper = new WebHelper(page);
    var guid: string;

    const projectName = TestData.ProjectName + Utils.generateGUID();
    const studyName = StudyTestData.StudyName + Utils.generateGUID();
    let deactivatedQuestion = "";

    await test.step('Navigating to URL', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
    });

    await test.step('Create a project', async () => {
        await projectService.CreateCustomProject(projectName, DropDownList.Project.CATI, TestData.Client, TestData.CommissioningMarket, projectName);
        guid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Add Question to the Project', async () => {
        await questionnaireService.addQuestion(TestData.M_QuestionName, TestData.M_QuestionName);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.M_QuestionName, TestData.M_QuestionType, TestData.M_QuestionText);
    });

    await test.step('Create a study and Verify Question from Questionniare', async () => {
        var masterQuestionnaireLinesCount = await projectService.getMasterQuestionnaireLinesCount();
        await studyService.CreateNewStudy(studyName, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await studyService.ValidateInitialDraftStateStudy();
        await webHelper.verifySaveButton();
        const questionCount = await studyService.getMasterQuestionnaireLinesCount();
        expect(masterQuestionnaireLinesCount).toBe(questionCount);
    });

    await test.step('Deactivate the Question from Study', async () => {
        await studyService.deactivateTheQuestionFromStudy(TestData.M_QuestionName)
    });

    await test.step('Verify the Deactivated Question in Inactive study questionnaire', async () => {
        await studyService.verifytheQuestionInSelectedView(TestData.M_QuestionName, StudySelectors.Text.InactiveStudy);
    });

    await test.step('Clean up created project record', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});

test("[2566632, 2427933] Verify that study QL sort order is in sync with master questionnaire", { tag: ['@Regression', '@Study'] }, async ({ page }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2566632' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2427933' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);
    var guid: string;

    const projectName = TestData.ProjectName + Utils.generateGUID();

    await test.step('Navigating to URL', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
    });

    await test.step('Create a project', async () => {
        await projectService.CreateCustomProject(projectName, DropDownList.Project.CATI, TestData.Client, TestData.CommissioningMarket, projectName);
        guid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Add ProductTemplate to the Project', async () => {
        await projectService.addProductTemplate(TestData.Product1);
        await projectService.applyProductTemplate();
    });

    await test.step('Create a study & Compare questionnaire lines with master questionnaire lines', async () => {
        var masterQuestionnaireLinesCount = await projectService.getMasterQuestionnaireLinesCount();
        const masterQuestionnaireLinesVariableNames: string[] = await projectService.getQuestionnaireLinesVariableName();
        const studyName = StudyTestData.StudyName + Utils.generateGUID();
        await studyService.CreateNewStudy(studyName, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await studyService.ValidateInitialDraftStateStudy();
        await studyService.compareMasterQuestionnairesWithStudyQuestionnairesLines(masterQuestionnaireLinesCount, masterQuestionnaireLinesVariableNames);
    });

    await test.step('Clean up created project record', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});


test("[2312195] Verify the user is allowed add back the question deactivated", { tag: ['@Regression', '@Study'] }, async ({ page }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2312195' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const questionnaireService = new Questionnaireservice(page);


    const webHelper = new WebHelper(page);
    var guid: string;

    const projectName = TestData.ProjectName + Utils.generateGUID();
    const studyName = StudyTestData.StudyName + Utils.generateGUID();
    let deactivatedQuestion = "";

    await test.step('Navigating to URL', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
    });

    await test.step('Create a project', async () => {
        await projectService.CreateCustomProject(projectName, DropDownList.Project.CATI, TestData.Client, TestData.CommissioningMarket, projectName);
        guid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Add Question to the Project', async () => {
        await questionnaireService.addQuestion(TestData.M_QuestionName, TestData.M_QuestionName);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.M_QuestionName, TestData.M_QuestionType, TestData.M_QuestionText);
    });

    await test.step('Create a study', async () => {
        await studyService.CreateNewStudy(studyName, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await studyService.ValidateInitialDraftStateStudy();
    });

    await test.step('Deactivate the Question from Study', async () => {
        await studyService.deactivateTheQuestionFromStudy(TestData.M_QuestionName)
    });

    await test.step('Verify the Deactivated Question in Inactive study questionnaire', async () => {
        await studyService.verifytheQuestionInSelectedView(TestData.M_QuestionName, StudySelectors.Text.InactiveStudy);
    });

    await test.step('Verify the Inactive Question in Inactive study questionnaire is reactivated', async () => {
        await studyService.activateTheQuestionFromStudy(TestData.M_QuestionName)
    });

    await test.step('Verify the Reactivated Question in study Question', async () => {
        await webHelper.clickOnTab(Common.Tabs.General);
        const studyQuestion: string[] = await studyService.getStudyQuestionnaireLinesVaribaleName();
        expect(studyQuestion).toContain(TestData.M_QuestionName);
    });

    await test.step('Clean up created project record', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});

test("[2427920] Verify if newly added questions / modules appear in all existing draft studies", { tag: ['@Regression', '@Study'] }, async ({ page }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2427920' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const questionnaireService = new Questionnaireservice(page);
    const webHelper = new WebHelper(page);
    var guid: string;

    const projectName = TestData.ProjectName + Utils.generateGUID();
    const studyName = StudyTestData.StudyName + Utils.generateGUID();

    await test.step('Navigating to URL', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
    });

    await test.step('Create a project', async () => {
        await projectService.CreateCustomProject(projectName, DropDownList.Project.CATI, TestData.Client, TestData.CommissioningMarket, projectName);
        guid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Add Question to the Project', async () => {
        await questionnaireService.addQuestion(TestData.M_QuestionName, TestData.M_QuestionName);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.M_QuestionName, TestData.M_QuestionType, TestData.M_QuestionText);
    });

    await test.step('Create a study', async () => {
        await studyService.CreateNewStudy(studyName, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await studyService.ValidateInitialDraftStateStudy();
        await webHelper.saveRecord();
        await webHelper.verifyNewButton();
        await webHelper.clickGoBackArrow();
    });

    await test.step('Add Question again to the Project after study is created and is in Draft state', async () => {
        await questionnaireService.addQuestion(QuestionnaireTestData.StandardQuestion, QuestionnaireTestData.StandardQuestion);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(QuestionnaireTestData.StandardQuestion, DropDownList.QuestionBank.SmallTextInput, QuestionnaireTestData.StandardQuestion);
    });

    await test.step('Compare questionnaire lines with master questionnaire lines', async () => {
        var masterQuestionnaireLinesCount = await projectService.getMasterQuestionnaireLinesCount();
        const masterQuestionnaireLinesVariableNames: string[] = await projectService.getQuestionnaireLinesVariableName();
        await studyService.openStudy(studyName);
        await studyService.compareMasterQuestionnairesWithStudyQuestionnairesLines(masterQuestionnaireLinesCount, masterQuestionnaireLinesVariableNames);
    });

    await test.step('Clean up created project record', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});

test("[2445493] Verify Languages sub-grid record are deleted when Fieldwork market lookup value updated and saved the study", { tag: ['@Regression', '@Study'] }, async ({ page }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2445493' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const questionnaireService = new Questionnaireservice(page);
    const webHelper = new WebHelper(page);
    var guid: string;

    const projectName = TestData.ProjectName + Utils.generateGUID();
    const studyName = StudyTestData.StudyName + Utils.generateGUID();

    await test.step('Navigating to URL', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
    });

    await test.step('Create a project', async () => {
        await projectService.CreateCustomProject(projectName, DropDownList.Project.CATI, TestData.Client, TestData.CommissioningMarket, projectName);
        guid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Add Question to the Project', async () => {
        await questionnaireService.addQuestion(TestData.M_QuestionName, TestData.M_QuestionName);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.M_QuestionName, TestData.M_QuestionType, TestData.M_QuestionText);
    });

    await test.step('Create a study', async () => {
        await studyService.CreateNewStudy(studyName, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await studyService.ValidateInitialDraftStateStudy();
    });

    await test.step('Add the Fieldwork Languages', async () => {
        await studyService.AddFieldWorkMarketLanguage(StudyTestData.Langauge1);
    });

    await test.step('Validate Draft study the Fieldwork Languages', async () => {
        await studyService.ValidateFieldworkMarketLanguages(StudyTestData.Langauge1, 'visible');

    });

    await test.step('Update Fieldwork Market', async () => {
        await studyService.UpdateFieldWorkMarket(StudyTestData.FieldWorkMarket1, StudyTestData.FieldWorkMarket);
    });

    await test.step('Validate Draft study the Fieldwork Languages are updated', async () => {
        await studyService.ValidateFieldworkMarketLanguages(StudyTestData.Langauge1, 'hidden');

    });

    await test.step('Clean up created project record', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});

test("[2359674] Verify CS user can create initial study verison with 1, Status Reason as Draft & below fields are editable Status Reason  Name, Category  Fieldwork Market  Scripter Notes.", { tag: ['@Regression', '@Study'] }, async ({ page }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2359674' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const questionnaireService = new Questionnaireservice(page);
    const webHelper = new WebHelper(page);
    var guid: string;

    const projectName = TestData.ProjectName + Utils.generateGUID();
    const studyName = StudyTestData.StudyName + Utils.generateGUID();

    await test.step('Navigating to URL', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
    });

    await test.step('Create a project', async () => {
        await projectService.CreateCustomProject(projectName, DropDownList.Project.CATI, TestData.Client, TestData.CommissioningMarket, projectName);
        guid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Add Question to the Project', async () => {
        await questionnaireService.addQuestion(TestData.M_QuestionName, TestData.M_QuestionName);
        await questionnaireService.validateQuestionsAddedInQuestionnaire(TestData.M_QuestionName, TestData.M_QuestionType, TestData.M_QuestionText);
    });


    await test.step('Create a study & Validate study state', async () => {
        await projectService.addScripterUserToProject();
        await studyService.CreateNewStudy(studyName, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await webHelper.handleConfirmationPopup();
        await studyService.ValidateInitialDraftStateStudy();
        await webHelper.saveRecord();
    });

    await test.step('Add the Fieldwork Languages', async () => {
        await studyService.AddFieldWorkMarketLanguage(StudyTestData.Langauge1);
    });

    await test.step('Validate the Editable fields in the Draft study', async () => {
        await studyService.ValidateEditableFieldsInStudy();
    });

    await test.step('Clean up created project record', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });

});
